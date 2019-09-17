// Program to print a matrix



// Include files

#include <stdio.h>
#include <stdlib.h>
#include <assert.h>



int
ReadClassification(char *classname)
{
  FILE *file;
  char magic[64];
  int version;
  int nclasses;
  int nmodels;

  // Open file
  file = fopen(classname, "r");
  if (!file) {
    fprintf(stderr, "Unable to open classification file %s\n", classname);
    return 0;
  }

  // Read magic keyword and version
  if (fscanf(file, "%s%d%", magic, &version) != 2) {
    fprintf(stderr, "Unable to read classification file\n");
    return 0;
  }
    
  // Read number of classes and models
  if (fscanf(file, "%d%d%", &nclasses, &nmodels) != 2) {
    fprintf(stderr, "Unable to read classification file\n");
    return 0;
  }

  fclose(file);
  // Return number of models
  return nmodels;
}



float *
ReadMatrix(char *matrixname, int nmodels)
{
  FILE *file;
  float *matrix;

  // Open file
  file = fopen(matrixname, "rb");
  if (!file) {
    fprintf(stderr, "Unable to open matrix file %s\n", matrixname);
    return NULL;
  }

  // Allocate matrix
  matrix = malloc(nmodels * nmodels * sizeof(float));
  assert(matrix);

  // Read matrix from file
  if (fread(matrix, sizeof(float), nmodels * nmodels, file) != nmodels * nmodels) {
    fprintf(stderr, "Unable to read file %s\n", matrixname);
    fclose(file);
    free(matrix);
    return NULL;
  }

  fclose(file);
  // Return matrix
  return matrix;
}



void PrintMatrix(float *matrix, int nmodels)
{
  float *matrixp;
  int i, j;

  // Print matrix entries as plain text
  for (i = 0; i < nmodels; i++) {
    for (j = 0; j < nmodels; j++) {
      if ((j > 0) && ((j%8) == 0)) printf("\n");
      printf(" %7.3f", matrix[i*nmodels+j]);
    }
    printf("\n");
  }
}



int
main(int argc, char **argv)
{
  int nmodels;
  float *matrix;

  // Parse args
  if(argc != 3) {
    fprintf(stderr, "usage: printmatrix classification matrix\n");
    exit(-1);
  }

  // Read the classification file
  nmodels = ReadClassification(argv[1]);
  if (nmodels == 0) exit(-1);

  // Read the matrix
  matrix = ReadMatrix(argv[2], nmodels);
  if (!matrix) exit(-1);

  // Print the matrix
  PrintMatrix(matrix, nmodels);

  // Return success
  return 0;
}








