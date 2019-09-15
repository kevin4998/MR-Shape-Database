

/**
	Source file for psbPlot.

	The psbPlot application calculates the precision versus
	recall.  A .cla file specifies the order of models and
	their grouping into classes.  A binary dissimilarity matrix of floats
	is analyzed in terms of the precision value at varying recalls.

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
#ifdef _WIN32
#include <direct.h>
#endif



///////////// # Defines
#define CUTOFF 2
#define SAMPLE 20
#define USE_QUERY 1
///////////// structs

// stores binary matrix information
typedef struct{
  int _index;  // original model position
  float _value;  // dissimilarity value
} Rank;

typedef struct{
	int _num;  // number of entries in _perf list
	float *_perf;  // precision at respective recall values
}Perf;


//////////////// function prototypes
void parseArgs(int argc, char *argv[]);
void printUsage();
void getBaseName();
void readMatrix();
int compareRanks(const void* v1,const void* v2);
void createModelClassMapping();
void calcModelPerf();
void calcClassPerf();
void calcAvgPerf(Perf *perf, int size, char *file);
float interpolatePerf(Perf *perf, float pos);
void createDir(char *file);

// variables
static bool _class, _model, _micro; // command line args
static char* _matrixFile, * _classFile, *_baseFile;
static PSBCategoryList* _categories;
static int _numModels;
static Rank** _ranks;
static int* _idToClass, *_models;
static Perf* _modelPerf, *_classPerf;


int main(int argc, char *argv[]){
	char file[128];

	parseArgs(argc, argv);
	// parse the .cla file that specifies model order
	// and class membership
	_categories = parseFile(_classFile, false);
	// create mapping from model position to class
	createModelClassMapping();	

	// create folder name
	getBaseName();
		
	readMatrix();
	// calculate performance per model
	calcModelPerf();
	if (_model) return 1;
	if (_micro){
		// calculate micro average over all models
		sprintf(file, "%s.plot", _baseFile);
		calcAvgPerf(_modelPerf, _numModels, file);
		return 1;
	}
	if (_class || !_micro){ 
		// calculate performance per class
		calcClassPerf();
		if (_class) return 1;
	}
	// calculate macro average over all classes
	
	sprintf(file, "%s.macro.plot", _baseFile);
	calcAvgPerf(_classPerf, _categories->_numCategories, file);
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
	printf("psbPlot classfile matrix [-macro|-class|-model]\n");
	printf("psbPlot classfile -random [-macro|-class|-model]\n");
	printf("Default settings creates a single plot with micro averaging.\n");
	printf("micro averaging combines results from all models, this is the default.\n");
	printf("-macro: average over all classes\n");
	printf("-class: create plot for each class\n");
	printf("-model: create plot for each model\n");
	printf("-macro,-class, and -model are mutually exclusive\n");
	printf("-random gives a random plot for the model, class, micro, or macro average\n");
	exit(1);
	
}


/*  Create a table mapping from model position to class index.
*/
void createModelClassMapping(){
  int i,j, idNum;
  int numCategory = _categories->_numCategories;
	
	_numModels = 0;
	for(i = 0; i < numCategory; ++i){
		_numModels += _categories->_categories[i]->_numModels;
	}


	_idToClass=(int*)malloc(_numModels * sizeof(int));
	assert(_idToClass);
 	_models = (int*)malloc(_numModels * sizeof(int));
	assert(_models); 
	idNum = 0;
	for(i = 0; i < numCategory; ++i){
		for(j = 0; j < _categories->_categories[i]->_numModels; ++j){
			_idToClass[idNum] = i;
			_models[idNum] = atoi(_categories->_categories[i]->_models[j]);
			++idNum;
		}	
	}
	
	//printf("IDs: %d\n",idNum);
	assert(idNum == _numModels);

}

/*
	Determine the base folder name to create for the outputs.
	The matrix name minus .matrix will be used.  The full path
	will also be removed so the folder will be created
	locally.
  */
void getBaseName(){
	char buffer[128], copy[128];

	char *pos = strstr(_matrixFile, ".matrix");
	int length;

	if (pos == NULL){
		strcpy(buffer, _matrixFile);
	}else{
		length = pos - _matrixFile;
		strncpy(buffer, _matrixFile, length * sizeof(char));
		buffer[length] = '\0';
	}

	pos = strrchr(buffer, '\\');  // windows specific
	if (pos == NULL){
		pos = strrchr(buffer, '/'); // Unix specific
	}
	if (pos != NULL){
		++pos; // past slash
		strcpy(copy, pos);
	}else{
		strcpy(copy, buffer);
	}

	_baseFile = strdup(copy);

}

/**
	A binary matrix of dissimilarity values is read.  
	It is assumed that the matrix
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
  if(_matrixFile == NULL){
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
		_ranks[i][j]._index=j;  // record model position
		_ranks[i][j]._value=fScore; // record dissimilarity value
    }
	// sort per row
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

/*
	For each model, calculate the precision and recall values.
*/
void calcModelPerf(){
	FILE* fp;
	char folderName[128], file[128];
	int i,classSize, modelIndex;
	int count=0;
	int classIndex;
	int posInClass;
	int lastClassIndex;

	//printf("calculating performance per model\n");
	
	if (_model){
		// create a folder for each model's plot
		//<matrix>/modelResults/<modelname>.plot
		sprintf(folderName, "%s.models/", _baseFile);
		createDir(folderName);
		//printf("creating plot file for each model\n");
	}
	_modelPerf = (Perf *)malloc(_numModels * sizeof(Perf));
	assert(_modelPerf);

	posInClass = 0;
	lastClassIndex = -1;
	for(modelIndex=0;modelIndex<_numModels;++modelIndex){
		classIndex = _idToClass[modelIndex];
		if (!strcmp(_categories->_categories[classIndex]->_name, MISC_CLASS)){
			// miscellaneous class
			_modelPerf[modelIndex]._num = 0;
			continue;
		}
		if (lastClassIndex != classIndex){
			lastClassIndex = classIndex;
			posInClass = 0;
		}else{
			++posInClass;
		}

		classSize=_categories->_categories[classIndex]->_numModels;
#if USE_QUERY
		int cSize=classSize;
#else
		int cSize=classSize-1;
#endif

		if (_model){
			// create a plot for the model
			sprintf(file, "%s%s_%s.plot", folderName, 
				_categories->_categories[classIndex]->_fullName, 
				_categories->_categories[classIndex]->_models[posInClass]);
			fp = fopen(file, "w");
			assert(fp);
			//fprintf(fp, "%d\n", classSize-1);
		}

		_modelPerf[modelIndex]._num = cSize;
		_modelPerf[modelIndex]._perf = (float*)malloc(cSize * sizeof(float));
		assert(_modelPerf[modelIndex]._perf);


		count = 0;
		for(i=0;i<_numModels;i++){
			//if (mid==_models[modelIndex])
			//modelIndex != i 
			int mid = _models[_ranks[modelIndex][i]._index];
#if USE_QUERY
#else
			if(mid==_models[modelIndex]){continue;}
#endif
			if(_idToClass[modelIndex]==_idToClass[_ranks[modelIndex][i]._index]){
				_modelPerf[modelIndex]._perf[count] = ((float)(count+1))/(i+1);
				count++;
				if (_model){
					fprintf(fp,"%f %f\n",((float)count)/(cSize),((float)count)/(i+1));
				}			
			}
		}

		if(count!=cSize){
			printf("%d %d\n",count,cSize);
			assert(0);
		}
	
		if (_model){
			fclose(fp);
		}
	}

	//printf("finished calculating performance per model\n");
}

/**
	Calculate performance per class by averaging over
	all models in the class
  */
void calcClassPerf(){
	FILE* fp;
	char folderName[128], file[128];
	int classIndex, numClasses, classSize, modelIndex, perfIndex, modelCount;
	bool writeFile;

	//printf("calculating performance per class\n");
	
	
	if (_class){
		// create a folder for the class plots
		//<matrix>/classResults/<classname>.plot
		sprintf(folderName, "%s.classes/", _baseFile);
		createDir(folderName);
		//printf("creating plot file for each class\n");	
	}

	numClasses = _categories->_numCategories;
	_classPerf = (Perf *)malloc(numClasses * sizeof(Perf));
	assert(_classPerf);


	modelCount = 0;
	for(classIndex=0;classIndex < numClasses; ++classIndex){
		classSize= _categories->_categories[classIndex]->_numModels;
#if USE_QUERY
		int cSize=classSize;
#else
		int cSize=classSize-1;
#endif
		if (!strcmp(_categories->_categories[classIndex]->_name, MISC_CLASS) || classSize <= CUTOFF){
			// miscellaneous class
			_classPerf[classIndex]._num = 0;
			modelCount += classSize;
			continue;
		}	
		_classPerf[classIndex]._num = cSize;
		_classPerf[classIndex]._perf = (float *)malloc(cSize * sizeof(float));
		assert(_classPerf[classIndex]._perf);
		memset(_classPerf[classIndex]._perf, 0, cSize * sizeof(float));

		// outter loop is over all models
		for(modelIndex = 0; modelIndex < cSize; ++modelIndex){
			// inner loop is over performance which is 0 to orig class size - 1
			for(perfIndex = 0; perfIndex < cSize; ++perfIndex){
				_classPerf[classIndex]._perf[perfIndex] += _modelPerf[modelCount]._perf[perfIndex];
			}
			++modelCount;
		}

		writeFile = _class && cSize > CUTOFF;
		if (writeFile){
			// create a file for each plot
			sprintf(file, "%s%s.plot", folderName, 
				_categories->_categories[classIndex]->_name);
			fp = fopen(file, "w");
			assert(fp);
			//fprintf(fp, "%d\n", classSize);
		}

		for(perfIndex = 0; perfIndex < cSize; ++perfIndex){
			_classPerf[classIndex]._perf[perfIndex] /= cSize;  // classSize + 1 to account for query model
			if (writeFile){
				fprintf(fp,"%f %f\n",((float)perfIndex+1)/cSize, 
						_classPerf[classIndex]._perf[perfIndex]);
			}			
		}

		if (writeFile){
			fclose(fp);
		}
	}
	
	//printf("finished calculating performance per class\n");

}
/** calculate average performance.
	If the model performance is the first parameter, then the
	micro average will be calculated.
	If the class performance is the first parameter, then hte
	macro average will be calculated.
*/
void calcAvgPerf(Perf *perf, int size, char *file){
	int i,j,valid;
	float temp;
	float mean[SAMPLE];
	FILE *fp;

	for(i=0;i<SAMPLE;i++){
		mean[i]=0;
	}

	for(j = 0; j < SAMPLE; ++j){
		valid = 0;
		for(i = 0; i < size; ++i){
			// only consider classes of a valid size
			// and only average over real interpolated results
			// this means avoid classes between precision 1 and 
			// 1/(classsize-1)
			if (perf[i]._num < CUTOFF || (perf[i]._num) < (SAMPLE / (float)(j+1))){continue;}
			temp = interpolatePerf(&perf[i], ((float)j+1)/SAMPLE);
			mean[j]+=temp;
			valid++;
		}
		if (valid > 0) mean[j] /= valid;
	}


	fp = fopen(file, "w");
	assert(fp);	
	for(j=0;j<SAMPLE;j++){
		fprintf(fp, "%f\t%f\n", (float)(j+1)/SAMPLE, mean[j]);
	}

	fclose(fp);
	
}

/**
	The precision at a specified recall level will be 
	bilinearly interpolated from neighboring precision values.
  */
float interpolatePerf(Perf *perf, float index){
	int x1,x2;
    float dx,xx;
	int bins;
	float *values;

	bins = perf->_num;
	values = perf->_perf;

    if(bins==0){return 0;}
	
    xx=index*bins-1;
    x1=(int)xx;
    x2=x1+1;
    dx=xx-x1;
    if(x1<0){x1=0;}
    if(x2<0){x2=0;}
    if(x1>=bins){x1=bins-1;}
    if(x2>=bins){x2=bins-1;}
    
    return values[x1]*(1-dx)+values[x2]*dx;
  
}

/**
	If the directory does not exist, then create it.
*/
void createDir(char *file){
	struct stat fileStat;
	if (stat(file, &fileStat) == -1){
		//printf("creating folder %s\n", file);
#ifdef _WIN32
		mkdir(file);
#else
		mkdir(file, 0x01C0);
#endif
	}
}
