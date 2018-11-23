del /f Cotangent_installer_win64.exe
REM call "%VS140COMNTOOLS%\vsvars32.bat"

REM add /debug after sign to get debug output
codesign\signtool sign /a /f "codesign/gradientspace_codesign_April2018.pfx" /p gradientX12Y cotangent.exe
codesign\signtool sign /a /f "codesign/gradientspace_codesign_April2018.pfx" /p gradientX12Y utilities/gpx.exe

NSIS\makensis /V4 cotangent_installer.nsi

REM wait for installer to write to disk
timeout 2

codesign\signtool sign /a /f "codesign/gradientspace_codesign_April2018.pfx" /p gradientX12Y Cotangent_installer_win64.exe

pause
