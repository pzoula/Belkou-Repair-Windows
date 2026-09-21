using Belkou.Core;

namespace Belkou.Repair;

internal static class DismRepair
{
    public static int ScanHealth() =>
        ProcessRunner.RunInteractive("DISM", "/Online /Cleanup-Image /ScanHealth");

    public static int RestoreHealth() =>
        ProcessRunner.RunInteractive("DISM", "/Online /Cleanup-Image /RestoreHealth");

    public static int ComponentCleanup() =>
        ProcessRunner.RunInteractive("DISM", "/Online /Cleanup-Image /StartComponentCleanup");

    public static int CheckHealth() =>
        ProcessRunner.RunInteractive("DISM", "/Online /Cleanup-Image /CheckHealth");
}
