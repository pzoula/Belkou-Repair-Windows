[Setup]
AppId={{A7C3E91B-4F2D-4B8A-9E15-6D1C8F0A2B47}
AppName=Belkou Windows Diagnostic & Recovery
AppVersion=1.1.0
AppPublisher=Belkou
AppPublisherURL=https://github.com/pzoula/Belkou-Repair-Windows
AppSupportURL=https://github.com/pzoula/Belkou-Repair-Windows/issues
AppUpdatesURL=https://github.com/pzoula/Belkou-Repair-Windows/releases
DefaultDirName={autopf}\Belkou
DefaultGroupName=Belkou
OutputDir=.
OutputBaseFilename=Belkou-Setup-1.1.0
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
ChangesEnvironment=yes
Compression=lzma
SolidCompression=yes
WizardStyle=modern
UninstallDisplayName=Belkou Windows Diagnostic & Recovery
VersionInfoVersion=1.1.0
VersionInfoCompany=Belkou
VersionInfoDescription=Belkou Windows Diagnostic & Recovery
VersionInfoProductName=Belkou

[Files]
Source: "..\publish\belkou.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Belkou"; Filename: "{app}\belkou.exe"
Name: "{group}\Uninstall Belkou"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Belkou"; Filename: "{app}\belkou.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional icons:"; Flags: unchecked

[Registry]
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\Session Manager\Environment"; \
    ValueType: expandsz; ValueName: "Path"; ValueData: "{olddata};{app}"; \
    Check: NeedsAddPath(ExpandConstant('{app}'))

[Run]
Filename: "{app}\belkou.exe"; Description: "Launch Belkou"; Flags: nowait postinstall skipifsilent

[Code]
function NeedsAddPath(Param: string): Boolean;
var
  OrigPath: string;
begin
  if not RegQueryStringValue(HKEY_LOCAL_MACHINE,
    'SYSTEM\CurrentControlSet\Control\Session Manager\Environment',
    'Path', OrigPath) then
  begin
    Result := True;
    exit;
  end;
  { Look for the path with leading and trailing semicolon }
  Result := Pos(';' + Param + ';', ';' + OrigPath + ';') = 0;
end;
