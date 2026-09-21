using Belkou.Core;

namespace Belkou.Hardware;

internal static class MemoryInfo
{
    public static (double TotalGb, double AvailableGb, string Summary) Get()
    {
        var (_, output) = ProcessRunner.CapturePowerShell(
            "$os=Get-CimInstance Win32_OperatingSystem; $t=[math]::Round($os.TotalVisibleMemorySize/1MB,1); $f=[math]::Round($os.FreePhysicalMemory/1MB,1); \"$t|$f\"");
        var parts = output.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2 &&
            double.TryParse(parts[0], out var total) &&
            double.TryParse(parts[1], out var free))
        {
            return (total, free, $"{total} GB\nAvailable: {free} GB");
        }

        return (0, 0, string.IsNullOrWhiteSpace(output) ? "Unavailable" : output);
    }
}
