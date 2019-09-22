OVERVIEW
========

offstats is a simple program for printing information about 
3D models described in the Geometry Center's OFF file format.  
Information about OFF can be found at the following Web sites: 
http://astronomy.swin.edu.au/~pbourke/geomformats/oogl/#OFF
http://www.dcs.ed.ac.uk/home/mxr/gfx/3d/OOGL.spec
http://www.neuro.sfc.keio.ac.jp/~aly/polygon/format/off.html

The purpose of offstats is mainly to provide example code 
for parsing OFF files.



COMPILATION
===========

offstats.exe contains a Microsoft Win32 executable.
So, for most Windows users, compilation is not necessary.
However, ...

To compile the program with Microsoft Visual C++, type:

  nmake -f Makefile.win32

To compile the program with a UNIX compiler, type:

  make -f Makefile.unix



EXECUTION
=========

To run the program, type:

  offstats <filename>

The program should print some simple information about the 
3D model to stdout.



DISCLAIMER
==========

This program and source code is provided as-is for non-commercial use.
It is not supported at all. The author is not responsible for any 
bugs/errors/etc. or harm from its use.




