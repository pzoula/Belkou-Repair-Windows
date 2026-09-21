using Belkou.Core;

namespace Belkou.Hardware;

internal static class DiskInfo
{
    public static string Inventory()
    {
        var (_, physical) = ProcessRunner.CapturePowerShell(
            "Get-PhysicalDisk | Select-Object FriendlyName,MediaType,Size,HealthStatus | Format-Table -AutoSize | Out-String");
        var (_, volumes) = ProcessRunner.CapturePowerShell(
            "Get-Volume | Where-Object DriveLetter | Select-Object DriveLetter,FileSystemLabel,FileSystem,Size,SizeRemaining,HealthStatus | Format-Table -AutoSize | Out-String");
        return (physical + Environment.NewLine + volumes).Trim();
    }

    public static (long FreeBytes, long TotalBytes, string Drive) SystemDriveSpace()
    {
        var root = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
        var di = new DriveInfo(root);
        return (di.AvailableFreeSpace, di.TotalSize, di.Name);
    }

    public static string HealthSummary()
    {
        var (_, output) = ProcessRunner.CapturePowerShell(
            "(Get-PhysicalDisk | ForEach-Object { \"$($_.FriendlyName): $($_.HealthStatus)\" }) -join '; '");
        return string.IsNullOrWhiteSpace(output) ? "Unavailable" : output.Trim();
    }
}
