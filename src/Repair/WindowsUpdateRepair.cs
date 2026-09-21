using Belkou.Core;

namespace Belkou.Repair;

internal static class WindowsUpdateRepair
{
    public static void OpenSettings() =>
        ProcessRunner.StartShell("ms-settings:windowsupdate");

    public static void Check()
    {
        Console.WriteLine("Opening Windows Update...");
        OpenSettings();
        var (_, output) = ProcessRunner.CapturePowerShell(
            "try { UsoClient StartInteractiveScan 2>$null; 'Scan requested' } catch { $_.Exception.Message }");
        if (!string.IsNullOrWhiteSpace(output))
            Console.WriteLine(output.Trim());
    }
}
