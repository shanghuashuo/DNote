[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-DNOTE2026STICKY}
AppName=DNote
AppVersion=1.0.0
AppPublisher=DNote
DefaultDirName={autopf}\DNote
DefaultGroupName=DNote
OutputDir=..\publish\installer
OutputBaseFilename=DNote-Setup-1.0.0
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
UninstallDisplayName=DNote

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "startupicon"; Description: "Start with Windows"; GroupDescription: "Startup:"

[Files]
Source: "..\publish\portable\DNote.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\DNote"; Filename: "{app}\DNote.exe"
Name: "{group}\{cm:UninstallProgram,DNote}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\DNote"; Filename: "{app}\DNote.exe"; Tasks: desktopicon
Name: "{userstartup}\DNote"; Filename: "{app}\DNote.exe"; Tasks: startupicon

[Run]
Filename: "{app}\DNote.exe"; Description: "{cm:LaunchProgram,DNote}"; Flags: nowait postinstall skipifsilent
