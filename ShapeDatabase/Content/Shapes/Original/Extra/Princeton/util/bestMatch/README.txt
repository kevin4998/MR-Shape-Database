OVERVIEW
========

bestMatch is a program for generating web pages to see the results of
each query.  The output is a series of .html pages.  The index.html
page is an overview page similar to the classification that has
links to each model's page of query results.



EXECUTION
=========

To run the program, type:

bestMatch.exe <classification>.cla <method>.matrix outDir

The program reads a classification (.cla) and a distance matrix
(.matrix) and outputs .html pages to the out put directory.  The query
model is colored in green, models of the correct class are colored
in blue, and models of incorrect classes are colored in red.  The
models are ordered left to right and top to bottom based on distance
from the query.


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



COMPILATION
===========

bestMatch.exe contains a Microsoft Win32 executable.
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









