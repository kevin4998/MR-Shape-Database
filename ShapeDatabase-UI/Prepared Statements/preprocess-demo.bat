:: Go up the directory where the application is.
cd ..\
:: First clean the directory for a brand new start.
ShapeDatabase.exe clean --exit --settings
:: Refine the shapes in the Demo map to be used in the process.
ShapeDatabase.exe refine --exit --directory "Content/Shapes/Demo"
:: Calculate the feature vectors beforehand for quicker usage.
ShapeDatabase.exe feature --exit
:: Pause can be used to notify the user that is is finished rather than instantly exiting.
:: pause
exit