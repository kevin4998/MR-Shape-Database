
/**
	Source file for psbTable.

	The psbTable application calculates statistics about retrieval results	
	for analysis purposes.  A .cla file specifies the order of models and
	their grouping into classes.  A binary dissimilarity matrix of floats
	is analyzed in terms of nearest neighbor matches, first tier matches,
	second tier matches, and discounted cumulative gain.

	The analysis can be performed at several levels of granularity, statistics
	can be calculated for each model, each class, for all models, or over the
	all classes.  Averaging over all models is called micro averaging,
	and averaging over all classes is called macro averaging.

	Philip Shilane
  */



#include <stdio.h>
#include <stdlib.h>
#include <assert.h>
#include <string.h>
#include <math.h>
#include <sys/stat.h>
#include <sys/types.h>
#include <fcntl.h>
#include "PSBClaParse.h"
#include <float.h>

///////////// # Defines
#define CUTOFF 2
#define SAMPLE 100
#define B 0.5f
// B value is used in E-Measure. 0.5 indicates 
// that precision and recall are equally valuable
#define K 32
// K is the max cutoff in the retrieval list for E and F measures

#define MAX(A, B) (A) > (B) ? (A) : (B)
#define MIN(A, B) (A) < (B) ? (A) : (B)
///////////// structs
typedef struct{
  int _index;
  float _value;
} Rank;


typedef struct{
	int _num;  //class size
	float _nn;  // first neighbor
	float _firstTier;  // class size neighbors
	float _secondTier; // 2 * class size neighbors
	float _dcg;  // discounted cumulative gain
	float _e; // E-Measure
}Table;

//////////////// function prototypes
void parseArgs(int argc, char *argv[]);
void printUsage();
void readMatrix();
int compareRanks(const void* v1,const void* v2);
void createModelClassMapping();
void calcTable();
void calcModelTable();
void calcTableTier(int model);
void calcDCG(int model);
void calcEF(int model);
void calcClassTable();
void calcAvgTable(Table *table, int tableSize);
double log2(double input);

// variables
static bool _class, _model, _micro; // command line args
static char* _matrixFile, * _classFile;
static PSBCategoryList* _categories;
static int _numModels;
static Rank** _ranks;
static int* _idToClass;
static Table *_modelTable, *_classTable;


int main(int argc, char *argv[]){

	parseArgs(argc, argv);
	// parse the .cla file
	_categories = parseFile(_classFile, false);
	createModelClassMapping();	

	readMatrix();
	
	// calculate statistics per model
	calcModelTable();
	if (_model) return 1;
	if (_micro){
		// calculate micro average over all models
		calcAvgTable(_modelTable, _numModels);
		return 1;
	}
	if (!_micro || _class){ 
		// calculate statistics per class
		calcClassTable();	
		if (_class) return 1;
	}

	// calculate macro average over all classes
	calcAvgTable(_classTable, _categories->_numCategories);

	return 1;
}

void parseArgs(int argc, char *argv[]){
	if (argc < 3){
		printUsage();
	}

	_class = _model = false;
	_micro = true;

	_classFile = argv[1];

	_matrixFile = argv[2];

	
	if(argc == 3) return;
	if(argc == 4){
			if (!strcmp(argv[3], "-class")){
				_class = true;
				_micro = false;
			}else if (!strcmp(argv[3], "-model")){
				_model = true;
				_micro = false;
			}else if (!strcmp(argv[3], "-macro")){
				_micro = false;
			}else{
				printUsage();
			}
		
	}else{
		printUsage();
	}

}

void printUsage(){
	printf("psbTable classfile matrix [-macro|-class|-model]\n");
	printf("Default settings creates a single table with micro averaging.\n");
	printf("micro averaging combines results from all models, this is the default.\n");
	printf("-macro: average over all classes\n");
	printf("-class: create table for each class\n");
	printf("-model: create table for each model\n");
	printf("-macro,-class, and -model are mutually exclusive\n");
	exit(1);
	
}

// map from model position to category
void createModelClassMapping(){
  int i,j, idNum;
  int numCategory = _categories->_numCategories;
	
	_numModels = 0;
	for(i = 0; i < numCategory; ++i){
		_numModels += _categories->_categories[i]->_numModels;
	}


	_idToClass=(int*)malloc(_numModels * sizeof(int));
	assert(_idToClass);
  
	idNum = 0;
	for(i = 0; i < numCategory; ++i){
		for(j = 0; j < _categories->_categories[i]->_numModels; ++j){
			_idToClass[idNum] = i;
			++idNum;
		}	
	}
	
	//printf("IDs: %d\n",idNum);
	assert(idNum == _numModels);

}

/*
	The matrix is assumed to have _numModels X _numModel float entries. 
  */
void readMatrix(){
  int i,j;
  FILE* fp;
  float fScore;

  _ranks=(Rank**)malloc(_numModels * sizeof(Rank*));
  assert(_ranks);
  for(i=0;i<_numModels;i++){
    _ranks[i]=(Rank*)malloc(_numModels * sizeof(Rank));
    assert(_ranks[i]);
  }
  fp=fopen(_matrixFile,"rb");
  if (fp == NULL){
	fprintf(stderr, "unable to open %s\n", _matrixFile);
	exit(2);
  }

  //printf("reading %s\n", _matrixFile);

  for(i=0;i<_numModels;i++){
    for(j=0;j<_numModels;j++){
		if (fread(&fScore,sizeof(float),1,fp)!= 1){
			printf("file %s has an incorrect size\n", _matrixFile);
			exit(-2);
		}
		_ranks[i][j]._index=j;  // record model position for use after sorting
		_ranks[i][j]._value=fScore;  // record dissimilarity
    }
	// sort each row by smallest dissimilarity
	_ranks[i][i]._value = FLT_MAX;
    qsort(_ranks[i],_numModels,sizeof(Rank),compareRanks);
  }
  fclose(fp);
  //printf("finished reading %s\n", _matrixFile);
}

// used in sorting the matrix of dissimilarity.
// if the value is the same, then order by model order to
// preserve stability of sort
// otherwise, the lowest dissimilarity value comes first
int compareRanks(const void* v1,const void* v2){
  Rank *r1,*r2;
  r1=(Rank*)v1;
  r2=(Rank*)v2;

  if(r1->_value==r2->_value){return r1->_index < r2->_index ? -1 : 1;}
  else if (r1->_value<r2->_value){
	  return -1;
  }
  else {
	  return 1;
  }
}

/**
	Calculate statistics per model.
*/
void calcModelTable(){
	int i, classIndex, posInClass, lastClassIndex;
	Table *table;
	
	//printf("calculating table per model\n");
	_modelTable = (Table*)malloc(_numModels * sizeof(Table));
	assert(_modelTable);
	memset(_modelTable, 0, _numModels * sizeof(Table));

	posInClass = 0;
	lastClassIndex = -1;
	for(i = 0; i < _numModels; ++i){
		classIndex = _idToClass[i];
		if (!strcmp(_categories->_categories[classIndex]->_name, MISC_CLASS)){
			// miscellaneous class
			_modelTable[i]._num = 0;
			continue;
		}

		
		table = &_modelTable[i];
		table->_num = _categories->_categories[classIndex]->_numModels;
		// calculate NN, first tier, and second tier
		calcTableTier(i);
		// calculate discounted cumulative gain
		calcDCG(i);
		//calculate E and F Measures
		calcEF(i);
		
		if (_model){
			// create a statistics file for this model
			classIndex = _idToClass[i];
			if (lastClassIndex != classIndex){
				lastClassIndex = classIndex;
				posInClass = 0;
			}else{
				++posInClass;
			}
			
			printf("%-50s %s %12.3f %12.3f %12.3f %12.3f %12.3f\n", _categories->_categories[classIndex]->_fullName, 
				_categories->_categories[classIndex]->_models[posInClass], 
				table->_nn, table->_firstTier, table->_secondTier, 
				table->_e, table->_dcg);
			
		}
	}

}

/**
	Calculate nearest neighbor, first tier, and second tier score
	for one model
  */
void calcTableTier(int model){
	int i, firstTier, secondTier;
	int count;
	Table *table;

	table = &_modelTable[model];
	firstTier = _categories->_categories[_idToClass[model]]->_numModels - 1;
	secondTier = firstTier * 2 < _numModels ? firstTier * 2 : _numModels-1;

	// nearest neighbor is 1 or 0 for a given model
	table->_nn = (float)(_idToClass[_ranks[model][0]._index] == _idToClass[model] ? 1 : 0);

	if (firstTier == 0){
		table->_firstTier = 0;
		table->_secondTier = 0;
		return;
	}

	count = 0;
	// count how many models in the first tier are in the correct class
	for(i = 0; i < firstTier; ++i){
		if (_idToClass[_ranks[model][i]._index] == _idToClass[model]){
			count++;
		}
	}

	table->_firstTier = count / (float)firstTier;


	// count how many models in the combined first and
	// second tier are in the correct class
	for(i = firstTier; i < secondTier; ++i){
		if (_idToClass[_ranks[model][i]._index] == _idToClass[model]){
			count++;
		}
	}
	table->_secondTier = count / (float)firstTier; // divide by class size

}

/**
	Calculate the discounted cumulative gain for a model.
	DCG is the 1 + Sum 1/lg(i) if the ith model is in the
	correct class.  This sum is then normalized by the 
	maximum possible value if the first C models
	were all in the correct class where C is the size 
	of the class.
*/
void calcDCG(int model){
	int i;
	float total, max;
	int correctClass, classSize;

	correctClass = _idToClass[model];
	classSize = _categories->_categories[correctClass]->_numModels - 1;

	total = (float)(_idToClass[_ranks[model][0]._index] == correctClass ? 1 : 0);

	for(i = 1; i < _numModels-1; ++i){
		if (_idToClass[_ranks[model][i]._index] == correctClass){
			total += (float)(1 / (double)log2((double)(i+1)));
		}
	}

	max = 1;
	for(i = 1; i < classSize; ++i){
		max += (float)(1 / (double)log2((double)(i+1)));
	}


	_modelTable[model]._dcg = total / max;
	
	//printf("%1.6f %1.6f %1.6f\n", total, max, _modelTable[model]._dcg);
}

double log2(double input){
	return log10(input)/0.3010299957;
}

void calcEF(int model){
	float recall, precision;
	int kPosition;
	int i, count, classSize, correctClass; 
	kPosition = _numModels-1 < K ? _numModels-1 : K;

	correctClass = _idToClass[model];
	classSize = _categories->_categories[correctClass]->_numModels - 1;

	count = 0;
	for(i = 0; i < kPosition; ++i){
		if (_idToClass[_ranks[model][i]._index] == correctClass){
			++count;
		}
	}

	recall = (float)count / classSize;
	precision = (float)count / kPosition;

	if (recall == 0 || precision == 0){
		_modelTable[model]._e = 0;
	}else{
		_modelTable[model]._e = 2.0f/(1.0f/recall + 1.0f/precision);
	}

	
}

/**
	Calculate the class statistics by averaging over all
	models in the class.
  */
void calcClassTable(){
	int i, modelCount, j, classSize;
	Table *table, *modelTable;
	//float nnVar, ftVar, stVar, dcgVar, temp;


	_classTable = (Table*)malloc(_categories->_numCategories * sizeof(Table));
	assert(_classTable);
	memset(_classTable, 0, _categories->_numCategories * sizeof(Table));

	modelCount = 0;

	for(i = 0; i < _categories->_numCategories; ++i){
		if (!strcmp(_categories->_categories[i]->_name, MISC_CLASS)){
			// miscellaneous class
			_classTable[i]._num = 0;
			continue;
		}
		classSize = _categories->_categories[i]->_numModels;

		table = &_classTable[i];
		table->_num = classSize;
		if (classSize <= CUTOFF){
			modelCount += classSize;		
			continue;
		}
		//nnVar = ftVar = stVar = dcgVar = 0;

		
		for(j = 0; j < classSize; ++j){
			modelTable = &_modelTable[modelCount];
			table->_nn += modelTable->_nn;
			table->_firstTier+= modelTable->_firstTier;
			table->_secondTier += modelTable->_secondTier;
			table->_dcg += modelTable->_dcg;
			table->_e += modelTable->_e;

			++modelCount;
		}

		table->_nn /= classSize;
		table->_firstTier /= classSize;
		table->_secondTier /= classSize;
		table->_dcg /= classSize;
		table->_e /= classSize;
	/*
		for(j = 0; j < classSize; ++j){
			modelTable = &_modelTable[modelCount];
			temp = modelTable->_nn - table->_nn;
			nnVar+= temp * temp;

			temp = modelTable->_firstTier - table->_nn;
			ftVar += temp * temp;

			temp = modelTable->_secondTier - table->_secondTier;
			stVar += temp * temp;
			
			temp = modelTable->_dcg - table->_dcg;
			dcgVar += temp * temp;
		}


		nnVar = (float)sqrt(nnVar / classSize);
		ftVar = (float)sqrt(ftVar / classSize);
		stVar = (float)sqrt(stVar / classSize);
		dcgVar = (float)sqrt(dcgVar / classSize);
		*/
		if (_class){
		printf("%-50s %12.3f %12.3f %12.3f %12.3f %12.3f\n", 
				_categories->_categories[i]->_fullName, 
				table->_nn, table->_firstTier, table->_secondTier, 
				table->_e, table->_dcg);
		/*
			printf("%-40s %12.3f %12.3f %12.3f %12.3f %12.3f %12.3f %12.3f %12.3f\n", 
				_categories->_categories[i]->_fullName, 
				table->_nn, table->_firstTier, table->_secondTier, table->_dcg,
				nnVar, ftVar, stVar, dcgVar);*/
		}

	}
	

}
/**
	Calculate the averages over all table elements.
	This may be micro averaging if the model table is the parameter,
	or macro averaging if the class table is used.
  */
void calcAvgTable(Table *table, int tableSize){
	int i, valid;
	float meanNN, meanFt, meanSt, meanDcg, meanE;
	
	meanNN = meanFt = meanSt = meanDcg = meanE = 0;

	valid=0;
	for(i=0;i<tableSize; ++i){

		if(table[i]._num<=CUTOFF){	
			continue;
		}
		++valid;
		
		meanNN += table[i]._nn;
		meanFt += table[i]._firstTier;
		meanSt += table[i]._secondTier;
		meanDcg += table[i]._dcg;
		meanE += table[i]._e;
	}
	meanNN /= valid;
	meanFt /= valid;
	meanSt /= valid;
	meanDcg /= valid;
	meanE /= valid;


	printf("%12.3f %12.3f %12.3f %12.3f %12.3f\n", 
		meanNN, meanFt, meanSt, meanE, meanDcg);
}

