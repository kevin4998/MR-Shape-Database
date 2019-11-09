:: Go up the directory where the application is.
cd ..\
:: Compare all the items in the database with each other.
ShapeDatabase.exe query --exit --queryinput internal --querysize class
:: Allow the user to view the shapes from the query results.
ShapeDatabase.exe view
:: Finish the application since view requires an exit command.
exit