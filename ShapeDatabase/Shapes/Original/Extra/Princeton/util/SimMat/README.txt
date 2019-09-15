OVERVIEW
========

SimMat is a program for generating images of distance matrix and color
images of the tiers of retrieval results.


EXECUTION
=========

To run the program from a Windows computer, type:

SimMat.bat <classifiation>.cla <method>.matrix image.gif [-distance] [-screen]

The program reads a classification (.cla) and a distance matrix
(.matrix) and outputs one or more precision-recall plots (.plot).  The
default execution creates a single .gif image of the color retrieval
tiers. Optional flags control the output.

       -distance creates the black and white image of the distance of
        each model in a row to each model in the columns.  The closer
        to black the pixel is colored, the less distance between the
        models.

        -screen adjusts the color tier image to have a more
         appropriate color scheme for paper submission.  The
         background is white with red for the first tier results and
         blue for the second tier results.  

Optional Execution
==================
For operation on other platforms, the command line should be adjusted
to:
java -Xms128M -Xmx512M -cp SimMat.jar psb.SimMat <classifiation>.cla <method>.matrix image.gif [-distance] [-screen]

The amount of memory allocated to the Java Virtual Machine can be
controlled with the first two parameters. 

Java
====
It is necessary to have a Java Virtual Machine installed to run this
application.  
Java can be downloaded from http://java.sun.com/j2se/1.4.2/download.html


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


DISCLAIMER
==========

This program and source code is provided as-is for non-commercial use.
It is not supported at all. The author is not responsible for any 
bugs/errors/etc. or harm from its use.









