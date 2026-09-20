[Setup]
AppName=Belkou Recovery Repair Toolkit
AppVersion=1.0.0
AppPublisher=Belkou
DefaultDirName={autopf}\Belkou
DefaultGroupName=Belkou
OutputDir=.
OutputBaseFilename=Belkou-Setup-1.0.0
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
Compression=lzma
SolidCompression=yes

[Files]
Source: "..\publish\belkou.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Belkou Recovery Repair Toolkit"; Filename: "{app}\belkou.exe"

[Run]
Filename: "{app}\belkou.exe"; Description: "Launch Belkou"; Flags: nowait postinstall skipifsilent
