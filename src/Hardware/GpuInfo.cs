using Belkou.Core;

namespace Belkou.Hardware;

internal static class GpuInfo
{
    public static string Summary()
    {
        var (_, output) = ProcessRunner.CapturePowerShell(
            "(Get-CimInstance Win32_VideoController | ForEach-Object { $_.Name }) -join ', '");
        return string.IsNullOrWhiteSpace(output) ? "Unavailable" : output.Trim();
    }
}
