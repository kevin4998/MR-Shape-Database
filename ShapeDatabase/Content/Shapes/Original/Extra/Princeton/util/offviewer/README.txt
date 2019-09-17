OVERVIEW
========

offviewer is a simple program for viewing 3D models
described in the Geometry Center's OFF file format.  
Information about OFF can be found at the following Web sites: 
http://astronomy.swin.edu.au/~pbourke/geomformats/oogl/#OFF
http://www.dcs.ed.ac.uk/home/mxr/gfx/3d/OOGL.spec
http://www.neuro.sfc.keio.ac.jp/~aly/polygon/format/off.html

offviewer provides example code for: 
(1) using GLUT and OpenGL to build an interactive viewing 
program and (2) parsing OFF files.



COMPILATION
===========

offviewer.exe contains a Microsoft Win3D executable.
So, for most Windows users, compilation is not necessary.
However, ...

To compile the program with Microsoft Visual C++, type:

  nmake -f Makefile.win32

To compile the program with a UNIX compiler, type:

  make -f Makefile.unix

The program depends on the GLUT user interface library. 
If your system does not already have GLUT installed, you can download it at:
http://www.opengl.org/developers/documentation/glut/index.html



EXECUTION
=========

To run the program, type:

  offviewer <filename>

Once the program starts, it pops up a viewing window.
Dragging the left/middle/right mouse in the window 
rotates/scales/translates the view.  Hitting the ESC key
exits the program.



DISCLAIMER
==========

This program and source code is provided as-is for non-commercial use.
It is not supported at all. The author is not responsible for any 
bugs/errors/etc. or harm from its use.



