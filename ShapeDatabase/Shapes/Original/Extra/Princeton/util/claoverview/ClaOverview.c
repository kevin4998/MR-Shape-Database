
/** Utility application to create overview web pages for .cla files.
    Usage
    ./ClaOverview file.cla <outputDir>

    With only the first argument, the .cla file is parsed and verified, and the program
    exits.  With an output directory specified, the program generates a web page overview
    in the output directory.  The images for the overview are linked from the 
    Princeton Shape Benchmark directory.  

    info.cgi and cgi-lib.pl must be placed in the output directory in order to be
    able to click on models for an expanded view.

    Philip Shilane
    
    Copyright 2003 Princeton University

*/
#include <stdio.h>
#include <string.h>
#include <stdlib.h>

#include "PSBClaParse.h"


static void printWebPages(const char *outDirectory, PSBCategoryList *categories);
static void printCategoryPages(const char *outDirectory, PSBCategoryList * categories);
static void printMainPage(const char* filename, PSBCategoryList *categories, const char* firstLine);
static int alphaSort(const void *, const void *);
static int sizeSort(const void *, const void *);




int main(int argc, char *argv[]){
  PSBCategoryList* categories;

  if (argc < 2 || argc > 3){
    printf("./ClaOverview file.cla webDirectory\n");
    printf("webDirectory is the location to print web pages\n");
    printf("if webDirectory is not specified, the program only verifies the file.cla\n");
    return 1;

  }
  
  categories = parseFile(argv[1]);

  if (argc == 3){
    printWebPages(argv[2], categories);
  }

}


/** 
    Prints out all of the overview web pages.
    Two overview pages will be created, the index.html page will be arranged alphabetically,
    and the _Size.html page will be ordered by the size of the categories.
    Each category will also have a page of its own.
*/
static void printWebPages(const char *outDirectory, PSBCategoryList *categoryList){
  char filename[256], links[256];
  int i, numCategories, numModels;

  printf("Writing index page %s\n", outDirectory);

  numCategories = 0;
  numModels = 0;
  for(i = 0; i < categoryList->_numCategories; ++i){
    if (categoryList->_categories[i]->_numModels > 0){
      ++numCategories;
      numModels += categoryList->_categories[i]->_numModels;
    }
  }

  // sort the categories alphabetically
  qsort(categoryList->_categories, categoryList->_numCategories, sizeof(PSBCategory *), alphaSort);
  sprintf(filename, "%s/index.html", outDirectory);
  sprintf(links, "<h2>Alphabetical Order</h2><p><h3>%d Categories, %d Models</h3><p><a href=\"_Size.html\"> To view categories ordered by size.</a>\n<br>\n<p>", numCategories, numModels);
  // print the overview page by alphabetical order
  printMainPage(filename, categoryList, links);

  // sort the categories by size
  qsort(categoryList->_categories, categoryList->_numCategories, sizeof(PSBCategory *), sizeSort);
  sprintf(filename, "%s/_Size.html", outDirectory);
  sprintf(links, "<h2>Size Order</h2><p><h3>%d Categories, %d Models</h3><p><a href=\"index.html\"> To view categories ordered by name.</a>\n<br>\n<p>", numCategories, numModels);
  // print the overview page by size
  printMainPage(filename, categoryList, links);

  // print all of the category pages
  printCategoryPages(outDirectory, categoryList);

  printf("Finished writing web pages\n");
}


/**
   Print out an overview page at the given filename, with all of the categories in the order
   specified by the CategoryList.  The overview page will print the fullname for each category
   with a link to that fullname.html that is assumed to be in the same folder.

   The firstLine string will be printed at the top of the page.  This can be a link to other
   URL's.
*/
static void printMainPage(const char* filename, PSBCategoryList *categoryList, const char* firstLine){
  FILE *file;
  int i;
  PSBCategory *category;

  file = fopen(filename, "w");
  if (file == NULL){
    fprintf(stderr, "unable to create %s\n", filename);
    exit(1);
  }

  fprintf(file, "<html><body>\n<p>\n");
  fprintf(file, "%s", firstLine);


  for(i = 0; i < categoryList->_numCategories; ++i){
    category = categoryList->_categories[i];
    // do not display categories with zero elements
    if (category->_numModels == 0) continue;
    fprintf(file, "<a href=\"%s.html\">%s</a> ( %d ) <br>\n", category->_fullName, category->_fullName, categoryList->_categories[i]->_numModels);
    
  }

  fprintf(file, "</body></html>\n");
  fclose(file);
}


/**
   Iterates through the category list creating web pages for each category that show one thumbnail
   for each model.  The thumbnail is a link through to a cgi script that shows several images of
   the model.
*/

static void printCategoryPages(const char *outDirectory, PSBCategoryList * categoryList){  
  FILE *categoryFile;
  int i, j, mid, subdir;
  PSBCategory *category;
  char buffer[256];


  for(i = 0; i < categoryList->_numCategories; ++i){
    printf("%d of %d\n", (i+1), categoryList->_numCategories);
    category = categoryList->_categories[i];
    // do not display categories with zero elements
    if (category->_numModels == 0) continue;

    sprintf(buffer, "%s/%s.html", outDirectory, category->_fullName);
    categoryFile = fopen(buffer, "w");
    if (categoryFile == NULL){
      fprintf(stderr, "Unable to create %s\n", buffer);
      exit(1);
    }
    
    fprintf(categoryFile, "<html><head><title>category %s, %d models</title></head>\n", category->_name, category->_numModels);;
    fprintf(categoryFile, "<body>\n");
    fprintf(categoryFile, "<h4>category %s has %d models</h4>\n", category->_name, category->_numModels);
    
    fprintf(categoryFile, "<table border=1 width=\"100%%\" cellpadding=2 cellspacing=2>\n");
    fprintf(categoryFile, "<tr>\n");
    
    for(j=0; j < category->_numModels; j++) {
      mid = category->_models[j];
      subdir = mid / 100;
      
      fprintf(categoryFile, "<td align=center valign=center><tt>%d, m%d </tt><br>\n", j, mid);
      fprintf(categoryFile, "<a href=\"javascript:void(window.open('./info.cgi?mid=%d', 'title', 'scrollbars=1,loaction=0,status=0,width=800,height=580'))\">", mid);

      sprintf(buffer, "http://shape.cs.princeton.edu/benchmark/thumbnails/%d/m%d/new_small0.jpg", subdir, mid);
      fprintf(categoryFile, "<img src=\"%s\"></a>\n", buffer);
      fprintf(categoryFile, "</td>\n");
      if (((j + 1) % 4) == 0) {
        fprintf(categoryFile, "</tr><tr>\n");
      }
    }
    fprintf(categoryFile, "</tr></table></body></html>\n");
    fclose(categoryFile);
  }
}



/** Sor the categories alphabetically. 
 */
static int alphaSort(const void *f, const void *s){
  PSBCategory *first, *second;
  first = *(PSBCategory**)f;
  second = *(PSBCategory**)s;

  return strcmp(first->_fullName, second->_fullName);

}

/* sort by size of category */
static int sizeSort(const void *f, const void *s){
  PSBCategory *first, *second;
  first = *(PSBCategory**)f;
  second = *(PSBCategory**)s;

  return (first->_numModels > second->_numModels ? -1 : 1);

}

