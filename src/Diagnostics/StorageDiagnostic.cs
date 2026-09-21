using Belkou.Core;
using Belkou.Hardware;

namespace Belkou.Diagnostics;

internal static class StorageDiagnostic
{
    public static void PrintDiskHealth() => Console.WriteLine(DiskInfo.Inventory());

    public static void PrintSmart()
    {
        var (_, output) = ProcessRunner.CapturePowerShell(
            "Get-PhysicalDisk | Select-Object FriendlyName,MediaType,HealthStatus,OperationalStatus,Size | Format-List | Out-String");
        Console.WriteLine(string.IsNullOrWhiteSpace(output) ? "SMART/health data unavailable." : output.Trim());
    }

    public static void PrintDiskSpace()
    {
        var (_, output) = ProcessRunner.CapturePowerShell(
            "Get-Volume | Where-Object DriveLetter | Select-Object DriveLetter,@{N='SizeGB';E={[math]::Round($_.Size/1GB,1)}},@{N='FreeGB';E={[math]::Round($_.SizeRemaining/1GB,1)}},HealthStatus | Format-Table -AutoSize | Out-String");
        Console.WriteLine(output.Trim());
    }

    public static void PrintWindowsStorage() =>
        ProcessRunner.StartShell("ms-settings:storagesense");

    public static void FileSystemCheck()
    {
        ConsoleUi.Warn("chkdsk on the system drive may require a reboot.");
        if (!ConsoleUi.Confirm("Schedule chkdsk /f /r on system drive?", false))
            return;
        var root = (Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\").TrimEnd('\\');
        ProcessRunner.RunInteractive("chkdsk", $"{root} /f /r");
    }

    public static CheckResult DiskSpaceCheck()
    {
        var (free, total, drive) = DiskInfo.SystemDriveSpace();
        var freeGb = free / 1024d / 1024d / 1024d;
        var pct = total == 0 ? 0 : (double)free / total * 100;
        var ok = freeGb >= 20 && pct >= 10;
        var severity = freeGb < 5 ? Severity.High : freeGb < 20 ? Severity.Medium : Severity.Ok;
        return new CheckResult
        {
            Id = "disk-space",
            Title = "Storage",
            Passed = ok,
            Severity = severity,
            Summary = $"{freeGb:0.#} GB free on {drive}",
            Recommendation = ok ? null : "Free disk space (Storage Sense / Disk Cleanup).",
            RepairAction = "storage-cleanup"
        };
    }

    public static CheckResult HealthCheck()
    {
        var health = DiskInfo.HealthSummary();
        var bad = health.Contains("Warning", StringComparison.OrdinalIgnoreCase) ||
                  health.Contains("Unhealthy", StringComparison.OrdinalIgnoreCase);
        return new CheckResult
        {
            Id = "disk-health",
            Title = "Disk health",
            Passed = !bad && !health.Equals("Unavailable", StringComparison.OrdinalIgnoreCase),
            Severity = bad ? Severity.High : Severity.Ok,
            Summary = health,
            Recommendation = bad ? "Back up data and investigate failing disks." : null
        };
    }
}
