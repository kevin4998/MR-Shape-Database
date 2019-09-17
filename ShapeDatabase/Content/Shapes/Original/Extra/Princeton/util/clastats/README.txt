OVERVIEW
========

ClaStats is a simple program for parsing .cla classification 
files and printing summary statistics.

PSBClaParse provides interfaces for verifying the format of .cla files
and returning a data structure of categories and models.


COMPILATION
===========

ClaStats.exe contains a Microsoft Win32 executable.
So, for most Windows users, compilation is not necessary.
However, ...

To compile the program with Microsoft Visual C++, type:

  nmake -f Makefile.win32

To compile the program with a UNIX compiler, type:

  make -f Makefile.unix


EXECUTION
=========

To run the program, type:

ClaStats.exe classification.cla

Summary statistics for the classification will be printed, 
if the file is in a valid .cla format.

DISCLAIMER
==========

This program and source code is provided as-is for non-commercial use.
It is not supported at all. The author is not responsible for any 
bugs/errors/etc. or harm from its use.
