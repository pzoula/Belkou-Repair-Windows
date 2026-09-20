# Belkou Recovery Repair Toolkit

Belkou is a Windows command-line recovery and repair toolkit.

## CLI

```powershell
belkou
belkou --version
belkou --help
```

The default command opens the interactive repair menu.

## Build

Requires .NET 8 SDK:

```powershell
dotnet publish .\src\Belkou.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish
```

Then compile `installer\Belkou.iss` with Inno Setup.

The ASCII banner is intentionally styled like modern CLI tools, with BELKOU shown as an outline/point-style terminal logo.


## Branding
The terminal banner spells the brand exactly as **BELKOU**.
