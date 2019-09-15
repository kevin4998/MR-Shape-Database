OVERVIEW
========

psbPlot is a program for generating precision-recall plots from a
classification and a distance matrix.  Plots can be created 
for each model, each class, or an overall average (the default).


EXECUTION
=========

To run the program, type:

psbPlot.exe <classification>.cla <method>.matrix [-macro|-class|-model]

The program reads a classification (.cla) and a distance matrix
(.matrix) and outputs one or more precision-recall plots (.plot).  The
default execution creates a single plot file, <method>.plot,
containing recall and precision values micro-averaged over all
classified models.  Optional flags allow creation of other plot files:

  -macro: creates the file <method>.macro.plot containing 
          precision values macro-averaged over all classes.
  -class: creates the directory <method>.classes with a separate
          plot file per class named <class>.plot containing recall 
          and precision values averaged over all models with the <class>.
  -model: creates the directory <method>.models with a separate 
          plot file per model with the name <class>_<modelId>.plot
  -model, -class, and -macro are mutually exclusive

Details about the formats of the files specifying the classification, 
distance matrix, and precision-recall plots follow.


CLASSIFICATION FILE
===================

The classification file specifies two important pieces of information
for psbPlot.  First, it indicates the order that models appear in the
rows and columns of the dissimilarity matrix.  Second, it provides a
grouping of the models so that psbPlot can determine relevant matches
(models in the same class as the query) from irrelevant ones.  
The PSB classification format is described in detail at:
http://shape.cs.princeton.edu/benchmark/documentation/classification_format.html
and free example software is provided for parsing classification files at: 
http://shape.cs.princeton.edu/benchmark/util.html


DISTANCE MATRIX
===============

The distance matrix represents the the dissimilarity of all pairs of
models in the database.  For N models, the matrix is a sequence of N
by N floating point numbers in binary format, where the number at
position i*N+j represents the distance between models i and j.  A
value of zero indicates that the descriptors are identical, and larger
values indicate greater dissimilarity between the 3D models.  All
distance values should be positive and can be arbitrarily large.
The PSB distance matrix format is described in detail at:
http://shape.cs.princeton.edu/benchmark/documentation/matrix_format.html
and free example software is provided for parsing matrix files at: 
http://shape.cs.princeton.edu/benchmark/util.html


PLOT FILES
==========
Plot files are ASCII files generated to be easily read by GNUPlot, 
Excel, or other plotting tools.  The plot file consists of recall 
values in the first column and precision values in the second column.  
The columns are separated by spaces.

When calculating the plot for each model, the query model is not
considered part of the retrieval results.  So although there are N
models in a class, for the model and class plots, there are N-1
distinct precision-recall values.  Average precision is computed at 20
distinct recall values [0.05, 0.10, etc.], where precision values are
interpolated between the nearest recall values below and above the
desired value.  In the recall range [0, 1/(classSize-1)], where no
meaningful precision values exist, the results will not be averaged
into the overall results.  For this reason, at very low recall values,
the precision reflects only the results achieved on the largest
classes.

With the -model flag, a plot is created for each model in the
classification file.  Below is an example output for a query shape in a
class that has six shapes.  There are five recall values, since there
are five shapes that can be retrieved (not considering the query shape
itself).  The left column is the recall = (number retrieved in class)/(class
size - 1).  The right column is the precision at that recall
value, precision = (number retrieved in class)/(position in retrieval
list).  

0.200000 1.000000
0.400000 0.500000
0.600000 0.300000
0.800000 0.097561
1.000000 0.113636

In this example, the first row (0.200000 1.000000) indicates that the
top match was a member of the query's class.  The second row (0.400000
0.500000) indicates that the second model found from the query's class
(a hit) was not found until the four best matches had been considered
(recall=2/5, precision=2/4).  Similarly, the third hit was ranked 10th
(recall = 3/5, precision=3/10), etc.


COMPILATION
===========

psbTable.exe contains a Microsoft Win32 executable.
So, for most Windows users, compilation is not necessary.
However, ...

To compile the program with Microsoft Visual C++, type:

  nmake -f Makefile.win32

To compile the program with a UNIX compiler, type:

  make -f Makefile.unix


DISCLAIMER
==========

This program and source code is provided as-is for non-commercial use.
It is not supported at all. The author is not responsible for any 
bugs/errors/etc. or harm from its use.









