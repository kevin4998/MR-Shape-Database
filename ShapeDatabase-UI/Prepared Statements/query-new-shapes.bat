:: Go up the directory where the application is.
cd ..\
:: Compare with items from the query folder returning the amount of items as specified in the settings.ini file.
ShapeDatabase.exe query --exit --queryinput refine --querysize kbest
:: Allow the user to view the shapes from the query results.
ShapeDatabase.exe view
:: Finish the application since view requires an exit command.
exit