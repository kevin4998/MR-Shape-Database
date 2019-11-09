:: Go up the directory where the application is.
cd ..\
:: First clean the directory for a brand new start
ShapeDatabase.exe clean --exit --settings
:: Measure all the shapes before execution of refinement.
ShapeDatabase.exe measure --exit --directory "Content/Shapes/All"
:: Refine the shapes in the All map to be used in the process.
ShapeDatabase.exe refine --exit --directory "Content/Shapes/All" --overwrite
:: Measure all the shapes after execution of refinement.
ShapeDatabase.exe measure --exit --directory "Content/Shapes/All"
:: Calculate the feature vectors and safe them using caching.
ShapeDatabase.exe feature --exit
:: Compare all the items in the database with each other.
ShapeDatabase.exe query --exit --queryinput internal --querysize class
:: Evaluate the results to determine the quality.
ShapeDatabase.exe evaluate --exit --evalmode aggregated
:: Do not view the shapes as the exported files are important.
:: ShapeDatabase.exe view
:: Exit the application instantly as it is done.
 exit