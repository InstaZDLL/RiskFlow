; Script Inno Setup pour RiskFlow (application WinUI 3 non packagée, self-contained).
; La version peut être passée en ligne de commande : ISCC /DMyAppVersion=1.2.3 RiskFlow.iss

#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif

#define MyAppName "RiskFlow"
#define MyAppPublisher "InstaZDLL"
#define MyAppURL "https://github.com/InstaZDLL/RiskFlow"
#define MyAppExeName "RiskFlow.exe"

[Setup]
; AppId identifie l'application de façon unique (ne pas changer entre les versions).
AppId={{8F3C2A1E-7B4D-4E9A-9C2F-1A6B5D8E3F70}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=..\installer-output
OutputBaseFilename=RiskFlow-Setup
SetupIconFile=..\src\RiskFlow.App\Assets\RiskFlow.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent
