; example2.nsi
;
; This script is based on example1.nsi, but it remember the directory, 
; has uninstall support and (optionally) installs start menu shortcuts.
;
; It will install example2.nsi into a directory that the user selects,


;Include Modern UI
  !include "MUI.nsh"
;--------------------------------
;Include MUI_EXTRAPAGES header
;  !include "MUI_EXTRAPAGES.nsh"  

!include x64.nsh
!include "FileAssociation.nsh"


!define MUI_ICON "..\Assets\cotangentApp\Resources\logo\logo1.ico"
!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "..\Assets\cotangentApp\Resources\logo\logo1.png"
!define MUI_HEADERIMAGE_RIGHT


; [RMS] include our macros
;!include "installer_macros.nsh"
;!include "DotNetChecker.nsh"
;!include "GetWindowsVersion.nsh"


;--------------------------------


; The name of the installer
Name "Cotangent"

; The file to write
OutFile "Cotangent_installer_win64.exe"

; The default installation directory
InstallDir $PROGRAMFILES64\Cotangent

; Registry key to check for directory (so if you install again, it will 
; overwrite the old one automatically)
InstallDirRegKey HKLM "Software\Cotangent" "Install_Dir"

; Request application privileges for Windows Vista
RequestExecutionLevel admin

;--------------------------------

; [RMS] this allows us to see the install log
; !define MUI_FINISHPAGE_NOAUTOCLOSE

;--------------------------------
;Installer Pages
  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_LICENSE "eula/eula-english-v1.txt"
;  !insertmacro MUI_PAGE_LICENSE "privacy_policy_current.txt"  
 
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  
  !insertmacro MUI_PAGE_FINISH
   
  
;--------------------------------
;Uninstaller Pages
  !insertmacro MUI_UNPAGE_WELCOME
 
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  !insertmacro MUI_UNPAGE_FINISH
  
  
  
  
;--------------------------------
;Languages
  ;Add 1st language
  !insertmacro MUI_LANGUAGE "English"
 


;--------------------------------



Function .onInit
  ; todo: auto-uninstall? http://nsis.sourceforge.net/Auto-uninstall_old_before_installing_new

  ; enable 64-bit registry view
  SetRegView 64	  

 
  ; check that we are in 64-bit
  ${If} ${RunningX64} 
  ${Else}
    MessageBox MB_OK|MB_ICONSTOP "Unfortunately, Cotangent requires a 64-bit version of Windows, and this machine is running 32-bit Windows. We apologize for the inconvenience. The installer will now exit."
	Abort "Unfortunately, Cotangent requires a 64-bit version of Windows, and this machine is running 32-bit Windows. We apologize for the inconvenience. The installer will now exit."
  ${EndIf}
  
FunctionEnd


; The stuff to install
Section "Cotangent Application"

  SectionIn RO
  
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  File "cotangent.exe" 
  File "UnityPlayer.dll"
  File "UnityCrashHandler64.exe"
  File /r "cotangent_Data"
  File /r "MonoBleedingEdge"
  File /r "utilities"
 
  ; Write the installation path into the registry
  WriteRegStr HKLM SOFTWARE\Cotangent "Install_Dir" "$INSTDIR"
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Cotangent" "DisplayName" "Cotangent"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Cotangent" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Cotangent" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Cotangent" "NoRepair" 1
  WriteUninstaller "uninstall.exe"
  
  ; delete registry keys for resolution because this is messed up sometimes...
  DeleteRegValue HKCU "Software\gradientspace\cotangent" "Screenmanager Fullscreen mode_h3630240806"
  DeleteRegValue HKCU "Software\gradientspace\cotangent" "Screenmanager Is Fullscreen mode_h3981298716"
  
  ${registerExtension} "$INSTDIR\cotangent.exe" ".cota" "Cotangent File"  
  
  
  ; install VS2015 runtime if required
  ; [RMS] just always installing. is fast and better to avoid problems...
;  ReadRegStr $1 HKLM "SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64" "Installed"
;  IfErrors 0 vs2015_installed
;  ClearErrors
;  ExecWait "sub_installers\vs2015r3_runtime\vc_redist.x64.exe /quiet"
;  SetOutPath "$PLUGINSDIR\sub_installers"
;  File "sub_installers\vs2015r3_runtime\vc_redist.x64.exe"
;  ExecWait "$PLUGINSDIR\sub_installers\vc_redist.x64.exe /quiet"
;vs2015_installed:

SectionEnd





; Optional section (can be disabled by the user)
Section "Start Menu Shortcuts"

  CreateDirectory "$SMPROGRAMS\Cotangent"
  CreateShortcut "$SMPROGRAMS\Cotangent\Uninstall.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
  CreateShortcut "$SMPROGRAMS\Cotangent\Cotangent.lnk" "$INSTDIR\cotangent.exe" "" "$INSTDIR\cotangent.exe" 0
  
SectionEnd



Section "Desktop Shortcut"
    SetShellVarContext current
    CreateShortCut "$DESKTOP\Cotangent.lnk" "$INSTDIR\cotangent.exe"
SectionEnd




;--------------------------------

; Uninstaller

Section "Uninstall"
  
  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Cotangent"
  DeleteRegKey HKLM SOFTWARE\Cotangent

  ; Remove files and uninstaller
  Delete $INSTDIR\cotangent.exe
  Delete $INSTDIR\UnityPlayer.dll
  Delete $INSTDIR\UnityCrashHandler64.exe
  Delete $INSTDIR\uninstall.exe

  RMDir /r $INSTDIR\utilities
  RMDir /r $INSTDIR\cotangent_Data
  RMDir /r $INSTDIR\MonoBleedingEdge

 
  ; Remove shortcuts, if any
  Delete "$SMPROGRAMS\Cotangent\*.*"
  Delete "$DESKTOP\Cotangent*.lnk"
  
  ; Remove directories used
  RMDir "$SMPROGRAMS\Cotangent"
  RMDir "$INSTDIR"

  ; remove file extension association
  ${unregisterExtension} ".cota" "Cotangent File"    
  
SectionEnd
















