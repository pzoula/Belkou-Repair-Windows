using Belkou.Core;

namespace Belkou.Hardware;

internal static class HardwareInventory
{
    public static void Print()
    {
        ConsoleUi.Section("CPU");
        Console.WriteLine(CpuInfo.Summary());
        Console.WriteLine();

        ConsoleUi.Section("MEMORY");
        Console.WriteLine(MemoryInfo.Get().Summary);
        Console.WriteLine();

        ConsoleUi.Section("GPU");
        Console.WriteLine(GpuInfo.Summary());
        Console.WriteLine();

        ConsoleUi.Section("STORAGE");
        Console.WriteLine(DiskInfo.Inventory());
        Console.WriteLine();

        ConsoleUi.Section("MOTHERBOARD");
        var (_, board) = ProcessRunner.CapturePowerShell(
            "$b=Get-CimInstance Win32_BaseBoard; $bios=Get-CimInstance Win32_BIOS; \"Manufacturer: $($b.Manufacturer)`nModel: $($b.Product)`nBIOS: $($bios.SMBIOSBIOSVersion)\"");
        Console.WriteLine(string.IsNullOrWhiteSpace(board) ? "Unavailable" : board.Trim());
    }

    public static Dictionary<string, string> ToDictionary()
    {
        var mem = MemoryInfo.Get();
        var (free, total, drive) = DiskInfo.SystemDriveSpace();
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["cpu"] = CpuInfo.Summary().Replace('\n', ' '),
            ["memoryTotalGb"] = mem.TotalGb.ToString("0.#"),
            ["memoryAvailableGb"] = mem.AvailableGb.ToString("0.#"),
            ["gpu"] = GpuInfo.Summary(),
            ["storageHealth"] = DiskInfo.HealthSummary(),
            ["systemDrive"] = drive,
            ["systemDriveFreeGb"] = (free / 1024d / 1024d / 1024d).ToString("0.#"),
            ["systemDriveTotalGb"] = (total / 1024d / 1024d / 1024d).ToString("0.#")
        };
    }
}
