del .\DicomTools.sln
del .\module\lib\ 
dotnet publish --configuration release --framework net10.0 --output .\module\DicomTools\lib\
