del /Q /S .net*.cs
if exist FreeLibSetSrc.7z del FreeLibSetSrc.7z
7za.exe a -r -x!Others\*.* -x!TemporaryGeneratedFile_*.* FreeLibSetSrc.7z *.sln *.cs *.resx *.csproj *.user *.bmp *.png *.cur *._gif *.ico *.bat app.config *.settings *.htm *.nunit ExtDB\TemplateFiles\*.*
7za.exe a -r FreeLibSetSrc.7z Tests\ExtTools.tests\DBF\Samples\*.*
7za.exe a -r FreeLibSetSrc.7z Tests\ExtDB.tests\TestTables\*.*

if exist FreeLibSetOthers.7z del FreeLibSetOthers.7z
7za.exe a -r FreeLibSetOthers.7z Others\*.*



