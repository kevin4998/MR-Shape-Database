OVERVIEW
========

ClaOverview is a simple program for parsing .cla classification 
files and creating overview web pages to view the classification.

PSBClaParse provides interfaces for verifying the format of .cla files
and returning a data structure for categories and models.


COMPILATION
===========

ClaOverview.exe contains a Microsoft Win32 executable.
So, for most Windows users, compilation is not necessary.
However, ...

To compile the program with Microsoft Visual C++, type:

  nmake -f Makefile.win32

To compile the program with a UNIX compiler, type:

  make -f Makefile.unix


EXECUTION
=========

To run the program, type:

ClaOverview.exe classification.cla <outputDir>

where outputDir is an optional argument.  Without specifying
outputDir, the application only verifies the .cla file.
Semi-helpful error messages are printed if the .cla file
is invalid.

cgi-lib.pl and info.cgi should be copied into the output 
directory.  These files provide support for viewing multiple
thumbnails of each model.


DISCLAIMER
==========

This program and source code is provided as-is for non-commercial use.
It is not supported at all. The author is not responsible for any 
bugs/errors/etc. or harm from its use.
