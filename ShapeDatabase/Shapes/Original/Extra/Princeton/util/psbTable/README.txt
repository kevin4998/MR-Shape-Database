OVERVIEW
========

psbTable is a program for calculating retrieval statistics (nearest
neighbor, first tier, second tier, E F Measure, and discounted cumulative gain) for
a given classification and distance matrix


EXECUTION
=========

To run the program, type:

psbTable.exe <classification>.cla <method>.matrix [-macro|-class|-model]

The program reads a classification (.cla) and a distance matrix
(.matrix) and prints to standard output (the terminal) four retrieval
statistics micro-averaged over all classified models.  Optional flags
allow printing of other statistics:

  -macro: prints statistics macro-averaged over all classes
  -class: prints a table of statistics with each row containing the
          class name followed by four statistics 
          averaged over all models within the class
  -model: prints a table of statistics with each row containing the
          class name, model id, and four statistics 
  -model, -class, and -macro are mutually exclusive

The first three printed statistics (nearest neighbor, first-tier, and
second-tier) indicate the percentage of the top $K$ matches that
belong to the same class as the query.  For the nearest neighbor
statistic, $K=1$, providing an indication to how well a nearest
neighbor classifier would perform.  For the first-tier and second-Tier
statistics, $K$ is $C-1$ and $2(C-1)$, respectively, where $C$ is the
size of the query's class.  The first tier statistic indicates the
recall for the smallest $K$ that could possibly include 100\% of the
models in the query class.  The second tier statistic provides the
same type of result, but is a little less stringent (i.e., $K$ is
twice as big).  It is similar to the ``Bulls Eye Percentage Score''
($K=2C$).  In all three cases, an ideal matching result (where all the
other models within the query's class appear as the top matches) gives
a score of 100%.

The fourth statistic is the E-Measure,
which is a composite measure of precision and recall for
a fixed number of retrieved results.  The intuition is that a user of
a search engine is more interested in the first page of query results
than in later pages.  So, this measure consider only the first 32
retrieved models for every query and calculate the precision and
recall over those results.  The E-Measure is defined as:

E = 2 / (1/P + 1/R)

The fifth statistic (discounted cumulative gain) gives a sense of how
well the overall retrieval would be viewed by a human.  Correct shapes
near the front of the list are more likely to be seen than correct
shapes near the end of the list.  With this rationale, discounted
cumulative gain is calculated as: 1 + sum 1/lg(i) if the ith shape is
in the correct class.  This sum is then normalized by the maximum
possible value if the first C-1 models were all in the correct class
where C-1 is the size of the class without the query.


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


EXAMPLE EXECUTIONS
==================

psbTable train.cla foo.matrix

  0.773      0.820      0.960      0.280        0.974

  where the five numbers indicate, from left to right, 
  the nearest neighbor, first-tier, second-tier, E-measure, and dcgain 
  statistics averaged over all models.


psbTable train.cla foo.matrix -macro

  0.773      0.820      0.960      0.350        0.974

  where the five numbers indicate the same statistics
  averaged over all *classes*.


psbTable train.cla foo.matrix -class

  animal___biped___human      0.915      0.524      0.695    0.460      0.863
  animal___biped___trex       0.833       0.639     0.750    0.447      0.818
  etc.

  where the five numbers indicate average statistics for each class.


psbTable train.cla foo.matrix -model

  animal___biped___human   127   1.000      0.820      0.960      0.530         0.974
  etc.

  where the five numbers indicate the model id followed by the
  four statistics for that model.


#! /bin/bash
printf "%30s %12s %12s %12s %12s\n" "Matrix" "NN" "1stTier" "2ndTier"
"EMEASURE" "DCGain"
printf "----------------------------------------------------------------\n"
for matrix in *.matrix; do
  printf "%-30s " $matrix 
  psbTable base_train.cla $matrix
done

  Matrix              NN   1stTier   2ndTier   EMEASURE    DCGain
  ----------------------------------------------------------------
  m1.matrix       0.320     0.243     0.336     0.230      0.529
  m2.matrix       0.347     0.251     0.341     0.243      0.531
  m3.matrix       0.560     0.353     0.461     0.301      0.620
  m4.matrix       0.600     0.349     0.446     0.307      0.627
 


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
