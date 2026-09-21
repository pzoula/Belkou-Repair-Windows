using Belkou.Core;

namespace Belkou.Hardware;

internal static class CpuInfo
{
    public static string Summary()
    {
        var (_, output) = ProcessRunner.CapturePowerShell(
            "(Get-CimInstance Win32_Processor | Select-Object -First 1 Name,NumberOfCores,NumberOfLogicalProcessors | ForEach-Object { \"$($_.Name)|$($_.NumberOfCores)|$($_.NumberOfLogicalProcessors)\" })");
        var parts = output.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 3)
            return $"{parts[0]}\n{parts[1]} Cores / {parts[2]} Threads";
        return string.IsNullOrWhiteSpace(output) ? "Unavailable" : output;
    }
}
