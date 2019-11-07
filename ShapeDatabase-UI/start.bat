ShapeDatabase.exe clean --exit --settings
ShapeDatabase.exe refine --exit --directory "Content/Shapes/Demo"
ShapeDatabase.exe feature --exit 
ShapeDatabase.exe query --exit --queryinput internal --querysize class
ShapeDatabase.exe evaluate --exit --evalmode aggregated
ShapeDatabase.exe view
pause
:: exit