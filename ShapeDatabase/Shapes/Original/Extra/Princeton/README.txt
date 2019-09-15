
PRINCETON SHAPE BENCHMARK
http://shape.cs.princeton.edu/benchmark/
Version 1, November 24, 2003


INTRODUCTION
============

The Princeton Shape Benchmark (PSB) provides a repository of 3D models
and software tools for evaluating shape-based retrieval and analysis
algorithms.  The motivation is to promote the use of standardized data
sets and evaluation methods for research in matching, classification,
clustering, and recognition of 3D models.  Researchers are encouraged
to use these resources to produce comparisons of competing algorithms
in future publications.


3D MODELS 
=========

The benchmark contains a database of 3D polygonal models collected
from the World Wide Web.  The files are located in the "db"
subdirectory.  For each 3D model, there is an Object File Format
(.off) file with the polygonal geometry of the model, a model
information file (e.g., the URL from whence it came), and a JPEG image
file with a thumbnail view of the model.  Version 1 of the benchmark
contains 1,814 models.

For ease of parsing, all models have been converted into the Geometry
Center's Object File Format (.off).  Documentation about the .off
format can be found in documentation/off_format.html, and sample
source code for parsing .off files can be found in util/offstats.

Each model has a unique identifier referred to as the "model id,"
which is a positive integer.  Because of the large number of models,
they are split into subdirectories to limit the number of files in any
single directory.  For a given model id, its folder is in
db/<folderId>/m<modelId>/, where <folderId> is calculated as follows
<folderId> = floor(<modelId> / 100).  As an example, files for model
812 are in db/8/m812/, and files for model 7 are in db/0/m7/.
Note that there is no model 762.


TRAINING AND TEST DATABASES
===========================

The benchmark set of models has been split into a training database
and a test database. Algorithms should be trained on the training
database (without influence of the test database).  Then, after all
exploration has been completed and all algorithmic parameters have
been frozen, results should be reported for the test database. In
Version 1, the training database contains 907 models, and the test
database contains 907 models.


CLASSIFICATIONS
===============

In order to enable evaluation of retrieval and classification
algorithms, the benchmark includes a simple mechanism to specify
partitions of the 3D models into classes.  We expect that many
possible classifications are possible for a given database of 3D
models.  Therefore, we provide a simple ASCII file format to describe
hierarchicies of classes and their members.  Documentation for the
classification file format (.cla) can be found in
documentation/classification_format.html.

In Version 1, we provide a "base" classification that reflects
primarily the function of each object and secondarily its form.  The
base training classification contains 90 classes, and the base test
classification contains 92 classes.  We expect to provide alternate
classifications in the near future, and we encourage other researchers
to submit interesting classifications for inclusion in future versions
of the benchmark.



SOFTWARE
========

We provide free source code to help parsing and working with the
benchmark files.  For instance, we provide sample code for: (1)
parsing Object File Format (.off) files, (2) parsing classification
(.cla) files, (3) visualizing .off files in an interactive OpenGL
viewer, (4) visualizing classifications with interactive Web
pages, (5) creating plots of precision and recall for a shape
retrieval, and (6) analyzing the retrieval results by a variety of
statistics.  Source code and Windows executables can be found in 
the util subdirectory.



OVERVIEW OF PRINCETON SHAPE BENCHMARK DIRECTORIES
=================================================

README.txt     - this file.
db             - subdirectory containing an OFF file, JPEG image, and ASCII text file for each 3D model
documentation  - subdirectory containing HTML files describing file formats using in the PSB.
classification - subdirectory containing CLA files specifying hierarchical groupings of the models
util           - subdirectory containing example source code for parsing .off and .cla files.


RESTRICTIONS
============

Please be sure to acknowledge the source of the models and source code
you take from this repository.  We provide the data and software to
promote research in shape-base retrieval and analysis.  However, they
are not supported, and they can only be used for academic purposes and
cannot be used for commercial products without permission.


ACKNOWLEDGMENTS
===============

The Princeton Benchmark Database was built by a number of people in
the Princeton University Computer Science Department including Phil
Shilane, Thomas Funkhouser, Patrick Min, and Alex Halderman.


FEEDBACK
========

Please send email to shape@cs.princeton.edu if you have feedback about
the Princeton Shape Benchmark, have found it useful for your research,
or wish to submit data to be included in future versions. 


Copyright 2003 by Princeton University



