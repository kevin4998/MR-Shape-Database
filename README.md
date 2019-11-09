# Shape Database

A content-based 3D shape retrieval system that, given a 3D shape, finds the most similar shapes in a given 3D shape database build for the University of Utrecht Multimedia Retrieval course.

## Purpose

This program was originally constructed for the Multimedia Retrieval course from the University of Utrecht, Game and Media master classes. The point of the assignment was to build a content-based 3D shape retrieval system that, given a 3D shape, finds the most similar shapes in a given 3D shape database. This would teach us the following skills while making the progam:
- Build up practical skills in choosing, customizing, and applying specific techniques and algorithms to construct a realistic end-to-end MR system;
- Learn the pro's and con's of different techniques, and also subtle practical constraints these have, such as robustness, ease of use, and computational scalability;
- Make concrete design and implementation choices based on real-world output created by their system;
- Get exposed to all steps of the MR pipeline (data representation, cleaning, feature extraction, matching, evaluation, and presentation);

## Workings

For more details on the inner workins of this program we would like to redirect you to the paper Building a content-based3D shape retrieval system by K.J.J. WesterBaan and G. de Jonge.

## Getting Started

The program has all its interaction with the console, it is advised to call the application's .exe file through a console or batch file as well for easier usage. Some premade batch files are present in the application "Prepared Statements" folder and these also show insights in how you can configure your personal call to the program. If this is not clear than you can call the application with the help verb to get more information on how it can be used. "ShapeDatabase.exe help"

### Prerequisites

A good understanding of C# and Visual Studio is required to code for this program.

### Installing

There are 2 ways to use the application: One is to go to the releases section and download the database fully set-up and configured including .ini file weights; The other one is to clone this repository and build it manually with visual studio. The downside of this second approach is that the calculated weights from the .ini file are not present so you can figure these out on your own. For more information on how to clone a github repository check out the [Visual Studio Github tutorial](https://github.com/github/VisualStudio/blob/master/docs/using/cloning-a-repository-to-visual-studio.md)

## Build With

This program wasn't possible without the following public packages from authors around Github.
- *Accord.Net:* for their statistical data processing, in particular their PCA system.
- *Geometry3Sharp:* for their 3D mesh computational algorithms.
- *HNSW.Net:* for their approximate neirest neighbour search program.
- *OpenTK:* for their visualisation code in C#.
- *CommandLineParser:* for easier console usage.
- *CsvHelper:* for their Csv reader and writers.
- *IniParser:* for their ini readers and writers.

## Contributors / Authors

The people who made this original repository and brought this code to the world were:
 - @kevin4998
 - @guusdejonge
 
## Acknolwedgements

And finally we would like to thank all the public library authors who made it possible for us to construct this program, the C# team for this programming language and our professor prof. dr. Alexandru C. Telea for teaching us how to design such an extensive system.