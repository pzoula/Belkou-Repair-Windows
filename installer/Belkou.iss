[Setup]
AppId={{A7C3E91B-4F2D-4B8A-9E15-6D1C8F0A2B47}
AppName=Belkou Recovery Repair Toolkit
AppVersion=1.0.0
AppPublisher=Belkou
AppPublisherURL=https://github.com/pzoula/Belkou-Repair-Windows
AppSupportURL=https://github.com/pzoula/Belkou-Repair-Windows/issues
AppUpdatesURL=https://github.com/pzoula/Belkou-Repair-Windows/releases
DefaultDirName={autopf}\Belkou
DefaultGroupName=Belkou
OutputDir=.
OutputBaseFilename=Belkou-Setup-1.0.0
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
ChangesEnvironment=yes
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Files]
Source: "..\publish\belkou.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Belkou Recovery Repair Toolkit"; Filename: "{app}\belkou.exe"
Name: "{group}\Uninstall Belkou"; Filename: "{uninstallexe}"

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
