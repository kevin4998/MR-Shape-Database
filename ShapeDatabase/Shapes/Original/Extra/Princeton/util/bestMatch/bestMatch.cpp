
#include <stdio.h>
#include <assert.h>
#include <stdlib.h>
#include <string.h>
#include "PSBClaParse.h"



// stores binary matrix information
typedef struct{
  int _index;  // original model position
  float _value;  // dissimilarity value
} Rank;


void readMatrix(const char *matrixFile);
int compareRanks(const void* v1,const void* v2);
void doModelQuery(const char* folder);
void createModelClassMapping();
void printMainPage(const char* folder);


static PSBCategoryList* _categories;
static Rank** _ranks;
static int _numModels;
static int* _idToClass;
static int* _models;

int main(int argc, char *argv[]){
	
	if (argc != 4){
		printf("bestMatch classFile.cla matrix outDir\n");
		return -1;
	}

	_categories = parseFile(argv[1], false);
	createModelClassMapping();
	readMatrix(argv[2]);
	printMainPage(argv[3]);
	return 0;
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

/**
	A binary matrix of dissimilarity values is read.  
	It is assumed that the matrix
  */
void readMatrix(const char *matrixFile){
  int i,j;
  FILE* fp;
  float fScore;

  _ranks=(Rank**)malloc(_numModels * sizeof(Rank*));
  assert(_ranks);
  for(i=0;i<_numModels;i++){
    _ranks[i]=(Rank*)malloc(_numModels * sizeof(Rank));
    assert(_ranks[i]);
  }
  fp=fopen(matrixFile,"rb");
  if (fp == NULL){
	fprintf(stderr, "unable to open %s\n", matrixFile);
	exit(2);
  }


  for(i=0;i<_numModels;i++){
    for(j=0;j<_numModels;j++){
		if (fread(&fScore,sizeof(float),1,fp)!= 1){
			printf("file %s has an incorrect size\n", matrixFile);
			exit(-2);
		}
		_ranks[i][j]._index=j;  // record model position
		_ranks[i][j]._value=fScore; // record dissimilarity value
    }
	// sort per row
    qsort(_ranks[i],_numModels,sizeof(Rank),compareRanks);
  }
  fclose(fp);
  
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


static void printMainPage(const char* folder){
  FILE *file;
  int i;
  char buffer[256];
  PSBCategory *category;

	sprintf(buffer, "%s\\index.html", folder);
  file = fopen(buffer, "w");
  if (file == NULL){
    fprintf(stderr, "unable to create %s\n", buffer);
    exit(1);
  }

  fprintf(file, "<html><body>\n<p>\n");


  for(i = 0; i < _categories->_numCategories; ++i){
    category = _categories->_categories[i];
    // do not display categories with zero elements
    if (category->_numModels == 0) continue;
    fprintf(file, "%s ( %d ) <br>\n", category->_fullName, category->_numModels);
    for(int j = 0; j < category->_numModels; ++j){
		fprintf(file, "<a href=\"%s__%s.html\">%s<br></a>", category->_fullName, category->_models[j], category->_models[j]);
	}
	fprintf(file, "<p>\n");
  }

  fprintf(file, "</body></html>\n");
  fclose(file);

  doModelQuery(folder);
}



/*
	For each model, calculate the precision and recall values.
*/
void doModelQuery(const char* folder){
	FILE* fp;
	char file[256], buffer[256];
	int i,classSize, modelIndex;
	int count=0;
	int classIndex;
	int posInClass;
	int lastClassIndex;
	PSBCategory* category;


	posInClass = 0;
	lastClassIndex = -1;
	for(modelIndex=0;modelIndex<_numModels;++modelIndex){
		classIndex = _idToClass[modelIndex];
		if (!strcmp(_categories->_categories[classIndex]->_name, MISC_CLASS)){
			// miscellaneous class
			continue;
		}
		if (lastClassIndex != classIndex){
			lastClassIndex = classIndex;
			posInClass = 0;
		}else{
			++posInClass;
		}
		category = _categories->_categories[classIndex];
		classSize = category->_numModels;


		// create a plot for the model
		sprintf(file, "%s\\%s__%s.html", folder, 
			category->_fullName, category->_models[posInClass]);
		fp = fopen(file, "w");
		assert(fp);
		fprintf(fp, "<html><head><title>%s, model %s, class size %d</title></head>\n", 
			category->_fullName, category->_models[posInClass], category->_numModels);;
		fprintf(fp, "<body>\n");
		fprintf(fp, "<font color=\"green\">Query</font><br>\n");
		fprintf(fp, "<font color=\"blue\">Correct Class</font><br>\n");
		fprintf(fp, "<font color=\"red\">Wrong Class</font><br>\n");
		fprintf(fp, "<table border=2 width=\"100%%\" cellpadding=2 cellspacing=2>\n");
		fprintf(fp, "<tr>\n");
		count = 0;

		for(i=0;i<_numModels;i++){
			
			int mid = _models[_ranks[modelIndex][i]._index];
			int subdir = mid / 100;
			char *color;
			if (mid==_models[modelIndex]){
				color = "green";
			}else if(_idToClass[modelIndex]==_idToClass[_ranks[modelIndex][i]._index]){	
				color = "blue";
			}else{
				color = "red";
			}

			fprintf(fp, "<td bgcolor=\"%s\"bordercolor=\"%s\" align=center valign=center><tt>%d, m%d, distance=%.3f </tt><br>\n", 
				color, color, i+1, mid, _ranks[modelIndex][i]._value);
			fprintf(fp, "<a href=\"javascript:void(window.open('./info.cgi?mid=%d', 'title', 'scrollbars=1,loaction=0,status=0,width=800,height=580'))\">", mid);

			sprintf(buffer, "http://shape.cs.princeton.edu/benchmark/thumbnails/%d/m%d/new_small0.jpg", subdir, mid);
			fprintf(fp, "<img border=\"2\" src=\"%s\"></a>\n", buffer);
			fprintf(fp, "</td>\n");
			if (((i + 1) % 4) == 0) {
				fprintf(fp, "</tr><tr>\n");
			}
		}
		
		fprintf(fp, "</tr></table></body></html>\n");			


		
	
		fclose(fp);
	}
	//printf("finished calculating performance per model\n");
}