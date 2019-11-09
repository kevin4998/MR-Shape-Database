:: Go up the directory where the application is.
cd ..\
:: First clean the directory for a brand new start.
ShapeDatabase.exe clean --exit --settings
:: Refine the shapes in the Small map to be used in the process.
ShapeDatabase.exe refine --exit --directory "Content/Shapes/Small"
:: Calculate the feature vectors beforehand for quicker usage.
ShapeDatabase.exe feature --exit
:: Pause can be used to notify the user that is is finished rather than instantly exiting.
:: pause
exit