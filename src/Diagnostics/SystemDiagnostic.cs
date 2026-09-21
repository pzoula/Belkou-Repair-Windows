using Belkou.Core;

namespace Belkou.Diagnostics;

internal static class SystemDiagnostic
{
    public static CheckResult WindowsVersion()
    {
        var (_, output) = ProcessRunner.CapturePowerShell(
            "$i=Get-ComputerInfo; \"$($i.WindowsProductName)|$($i.WindowsVersion)|$($i.OsHardwareAbstractionLayer)|$($i.OsArchitecture)\"");
        var parts = output.Split('|', StringSplitOptions.TrimEntries);
        var summary = parts.Length >= 2 ? $"{parts[0]} (build/version {parts[1]})" : output;
        return new CheckResult
        {
            Id = "windows-version",
            Title = "Windows version",
            Passed = !string.IsNullOrWhiteSpace(output),
            Severity = string.IsNullOrWhiteSpace(output) ? Severity.Inconclusive : Severity.Ok,
            Summary = string.IsNullOrWhiteSpace(summary) ? "Unavailable" : summary.Trim(),
            Details = output
        };
    }

    public static void PrintSystemInfo() => ProcessRunner.RunInteractive("systeminfo");

    public static void PrintWindowsVersion()
    {
        var check = WindowsVersion();
        Console.WriteLine(check.Summary);
        if (!string.IsNullOrWhiteSpace(check.Details))
            Console.WriteLine(check.Details);
    }

    public static void PrintDrivers() =>
        ProcessRunner.RunInteractive("powershell.exe",
            "-NoProfile -ExecutionPolicy Bypass -Command \"Get-CimInstance Win32_PnPSignedDriver | Select-Object DeviceName,DriverVersion,DriverDate,Manufacturer | Sort-Object DeviceName | Format-Table -AutoSize\"");

    public static void PrintServices() =>
        ProcessRunner.RunInteractive("powershell.exe",
            "-NoProfile -ExecutionPolicy Bypass -Command \"Get-Service | Sort-Object Status,DisplayName | Format-Table Status,StartType,Name,DisplayName -AutoSize\"");

    public static void PrintStartup() =>
        ProcessRunner.RunInteractive("powershell.exe",
            "-NoProfile -ExecutionPolicy Bypass -Command \"Get-CimInstance Win32_StartupCommand | Select-Object Name,Command,Location,User | Format-Table -AutoSize\"");

    public static CheckResult FailedServices()
    {
        var (_, output) = ProcessRunner.CapturePowerShell(
            "@(Get-Service | Where-Object { $_.Status -eq 'Stopped' -and $_.StartType -eq 'Automatic' } | Select-Object -ExpandProperty Name) -join ', '");
        var names = string.IsNullOrWhiteSpace(output)
            ? Array.Empty<string>()
            : output.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var count = names.Length;
        return new CheckResult
        {
            Id = "services",
            Title = "Services",
            Passed = count == 0,
            Severity = count == 0 ? Severity.Ok : count > 3 ? Severity.High : Severity.Medium,
            Summary = count == 0 ? "No failed automatic services" : $"{count} failed automatic service(s)",
            Details = count == 0 ? "" : string.Join(", ", names.Take(20)),
            Recommendation = count == 0 ? null : "Analyze stopped automatic services and restart critical ones.",
            RepairAction = "services"
        };
    }

    public static CheckResult DriversQuick()
    {
        var (_, output) = ProcessRunner.CapturePowerShell(
            "@(Get-PnpDevice | Where-Object { $_.Status -ne 'OK' } | Select-Object -ExpandProperty FriendlyName) -join ', '");
        var bad = string.IsNullOrWhiteSpace(output)
            ? Array.Empty<string>()
            : output.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return new CheckResult
        {
            Id = "drivers",
            Title = "Drivers",
            Passed = bad.Length == 0,
            Severity = bad.Length == 0 ? Severity.Ok : Severity.Medium,
            Summary = bad.Length == 0 ? "No problem devices reported" : $"{bad.Length} device(s) not OK",
            Details = string.Join(", ", bad.Take(15)),
            Recommendation = bad.Length == 0 ? null : "Review Device Manager for devices that are not OK."
        };
    }
}
