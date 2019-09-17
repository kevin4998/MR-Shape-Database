OVERVIEW
========

printmatrix is a simple program for printing the contents of a
distance matrix.  It is provided mainly as a sample of code
for parsing matrix files.



COMPILATION
===========

printmatrix.exe contains a Microsoft Win32 executable.
So, for most Windows users, compilation is not necessary.
However, ...

To compile the program with Microsoft Visual C++, type:

  nmake -f Makefile.win32

To compile the program with a UNIX compiler, type:

  make -f Makefile.unix



EXECUTION
=========

To run the program, type:

  printmatrix <classification> <matrix>

The program should print NxN distance values read 
from the matrix, where N is the number of models
found in the classification file.



DISCLAIMER
==========

This program and source code is provided as-is for non-commercial use.
It is not supported at all. The author is not responsible for any 
bugs/errors/etc. or harm from its use.





