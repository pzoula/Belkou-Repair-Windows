# Belkou Windows Diagnostic & Recovery

Belkou is a modular Windows diagnostic, repair, and reporting toolkit for IT technicians.

## CLI

```powershell
belkou                         # Interactive categorized menu
belkou diagnose [--json]       # Auto diagnostic engine
belkou repair                  # Guided repair workflow
belkou system
belkou hardware
belkou network
belkou storage
belkou events
belkou report [--json]
belkou backup drivers
belkou backup network
belkou backup registry
belkou --version
belkou --help
```

## Menu categories

SYSTEM · REPAIR · NETWORK · STORAGE · PERFORMANCE · SECURITY · REPORTS · BACKUP

## Build

Requires .NET 8 SDK:

```powershell
dotnet publish .\src\Belkou.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish
```

Then compile `installer\Belkou.iss` with Inno Setup to produce `Belkou-Setup-1.1.0.exe`.

## Reports

Diagnostic reports are written to Documents under `Belkou-Reports\` (HTML, JSON, text).  
Backups go to `Belkou-Backup\`.

## Branding

The terminal banner spells the brand exactly as **BELKOU**.
