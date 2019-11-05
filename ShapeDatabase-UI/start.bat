ShapeDatabase.exe clean --exit --settings
ShapeDatabase.exe refine --exit --directory "Content/Shapes/Small"
ShapeDatabase.exe feature --exit 
ShapeDatabase.exe query --exit --queryinput internal --querysize class
ShapeDatabase.exe evaluate --exit --evalmode aggregated
pause
:: exit