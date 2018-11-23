setlocal enabledelayedexpansion

rmdir /Q /S ..\installer\cotangent_Data
del /Q /F ..\installer\cotangent.exe
del /Q /F ..\installer\UnityPlayer.dll
rmdir /Q /S ..\installer\utilities

copy /Y %1 ..\installer\cotangent.exe
copy /Y UnityPlayer.dll ..\installer\UnityPlayer.dll

set SEARCHTEXT=.exe
set REPLACETEXT=_Data

SET string=%1
SET modified=!string:%SEARCHTEXT%=%REPLACETEXT%!

xcopy /Y /E %modified% ..\installer\cotangent_Data\

xcopy /Y /E utilities ..\installer\utilities\

REM pause