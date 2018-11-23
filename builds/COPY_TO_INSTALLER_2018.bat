setlocal enabledelayedexpansion

rmdir /Q /S ..\installer\cotangent_Data
del /Q /F ..\installer\cotangent.exe
rmdir /Q /S ..\installer\MonoBleedingEdge
del /Q /F ..\installer\UnityPlayer.dll
del /Q /F ..\installer\UnityCrashHandler64.exe
rmdir /Q /S ..\installer\utilities

REM if we drag-dropped path, it is full path C:\...\cotangentXX\. This strips all but last folder
for %%f in (%1) do set XXX=%%~nxf

REM echo %XXX%
REM pause

copy /Y %XXX%\%XXX%.exe ..\installer\cotangent.exe
copy /Y %XXX%\UnityCrashHandler64.exe ..\installer\UnityCrashHandler64.exe
copy /Y %XXX%\UnityPlayer.dll ..\installer\UnityPlayer.dll


xcopy /Y /E %XXX%\MonoBleedingEdge ..\installer\MonoBleedingEdge\
xcopy /Y /E %XXX%\%XXX%_Data ..\installer\cotangent_Data\

xcopy /Y /E utilities ..\installer\utilities\

REM pause