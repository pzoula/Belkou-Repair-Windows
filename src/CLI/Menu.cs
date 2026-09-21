using Belkou.Backup;
using Belkou.Core;
using Belkou.Diagnostics;
using Belkou.Hardware;
using Belkou.Repair;
using Belkou.Reports;
using Belkou.Security;

namespace Belkou.CLI;

internal static class Menu
{
    public static void Run()
    {
        while (true)
        {
            ConsoleUi.WriteBanner();

            ConsoleUi.Section("SYSTEM");
            Console.WriteLine(" [1] System Information");
            Console.WriteLine(" [2] Hardware Inventory");
            Console.WriteLine(" [3] Windows Version");
            Console.WriteLine(" [4] Drivers");
            Console.WriteLine(" [5] Services");
            Console.WriteLine(" [6] Startup Programs");

            ConsoleUi.Section("REPAIR");
            Console.WriteLine(" [7] SFC");
            Console.WriteLine(" [8] DISM");
            Console.WriteLine(" [9] Component Store");
            Console.WriteLine(" [10] Windows Update");
            Console.WriteLine(" [11] System Restore");
            Console.WriteLine(" [12] WinRE");

            ConsoleUi.Section("NETWORK");
            Console.WriteLine(" [13] Network Diagnostics");
            Console.WriteLine(" [14] DNS");
            Console.WriteLine(" [15] Winsock");
            Console.WriteLine(" [16] TCP/IP");
            Console.WriteLine(" [17] Internet Connectivity");

            ConsoleUi.Section("STORAGE");
            Console.WriteLine(" [18] Disk Health");
            Console.WriteLine(" [19] SMART");
            Console.WriteLine(" [20] Disk Space");
            Console.WriteLine(" [21] Windows Storage");
            Console.WriteLine(" [22] File-System Check");

            ConsoleUi.Section("PERFORMANCE");
            Console.WriteLine(" [23] CPU");
            Console.WriteLine(" [24] RAM");
            Console.WriteLine(" [25] GPU");
            Console.WriteLine(" [26] Disk Performance");
            Console.WriteLine(" [27] Battery");

            ConsoleUi.Section("SECURITY");
            Console.WriteLine(" [28] Windows Defender");
            Console.WriteLine(" [29] Firewall");
            Console.WriteLine(" [30] Security Status");
            Console.WriteLine(" [31] Windows Security Events");

            ConsoleUi.Section("REPORTS");
            Console.WriteLine(" [32] Quick Diagnostic");
            Console.WriteLine(" [33] Full Diagnostic");
            Console.WriteLine(" [34] Export Report");

            ConsoleUi.Section("BACKUP");
            Console.WriteLine(" [35] Create Restore Point");
            Console.WriteLine(" [36] Restore Point Information");
            Console.WriteLine(" [37] Backup Network Configuration");
            Console.WriteLine(" [38] Backup Important Registry Keys");
            Console.WriteLine(" [39] Export Drivers");

            Console.WriteLine();
            Console.WriteLine(" [Q] Exit");
            Console.WriteLine();
            Console.Write(" Select [1-39/Q]: ");

            var choice = Console.ReadLine()?.Trim().ToUpperInvariant();
            if (choice is "Q" or "QUIT" or "EXIT")
                return;

            try
            {
                RunChoice(choice);
            }
            catch (Exception ex)
            {
                ConsoleUi.Fail(ex.Message);
            }

            ConsoleUi.Pause();
        }
    }

    static void RunChoice(string? choice)
    {
        switch (choice)
        {
            case "1": SystemDiagnostic.PrintSystemInfo(); break;
            case "2": HardwareInventory.Print(); break;
            case "3": SystemDiagnostic.PrintWindowsVersion(); break;
            case "4": SystemDiagnostic.PrintDrivers(); break;
            case "5": SystemDiagnostic.PrintServices(); break;
            case "6": SystemDiagnostic.PrintStartup(); break;

            case "7": SfcMenu(); break;
            case "8": DismMenu(); break;
            case "9": DismRepair.CheckHealth(); break;
            case "10": WindowsUpdateRepair.Check(); break;
            case "11": ProcessRunner.StartShell("rstrui.exe"); break;
            case "12": ProcessRunner.RunInteractive("reagentc", "/info"); break;

            case "13": NetworkDiagnostic.RunEngine(true); break;
            case "14": NetworkDiagnostic.FlushDns(); break;
            case "15": NetworkRepair.ResetWinsock(); break;
            case "16": NetworkRepair.ResetTcpIp(); break;
            case "17": NetworkDiagnostic.PrintConnectivity(); break;

            case "18": StorageDiagnostic.PrintDiskHealth(); break;
            case "19": StorageDiagnostic.PrintSmart(); break;
            case "20": StorageDiagnostic.PrintDiskSpace(); break;
            case "21": StorageDiagnostic.PrintWindowsStorage(); break;
            case "22": StorageDiagnostic.FileSystemCheck(); break;

            case "23": Console.WriteLine(CpuInfo.Summary()); break;
            case "24": Console.WriteLine(MemoryInfo.Get().Summary); break;
            case "25": Console.WriteLine(GpuInfo.Summary()); break;
            case "26": ProcessRunner.RunInteractive("winsat", "disk -drive c"); break;
            case "27":
            {
                var path = Path.Combine(Path.GetTempPath(), "Belkou-BatteryReport.html");
                ProcessRunner.RunInteractive("powercfg", $"/batteryreport /output \"{path}\"");
                Console.WriteLine($"Battery report: {path}");
                break;
            }

            case "28": DefenderStatus.Print(); break;
            case "29": FirewallStatus.Print(); break;
            case "30":
                DefenderStatus.Print();
                Console.WriteLine();
                FirewallStatus.Print();
                break;
            case "31": EventDiagnostic.Analyze(true); break;

            case "32": DiagnosticEngine.Run(true); break;
            case "33":
            {
                var report = DiagnosticEngine.Run(true);
                var paths = ReportWriter.WriteAll(report);
                ConsoleUi.Ok(paths.HtmlPath);
                ProcessRunner.StartShell("explorer.exe", paths.Directory);
                break;
            }
            case "34":
            {
                var report = DiagnosticEngine.Run(true);
                var paths = ReportWriter.WriteAll(report);
                ConsoleUi.Ok($"HTML: {paths.HtmlPath}");
                ConsoleUi.Ok($"JSON: {paths.JsonPath}");
                ConsoleUi.Ok($"TEXT: {paths.TextPath}");
                ProcessRunner.StartShell("explorer.exe", paths.Directory);
                break;
            }

            case "35": BackupService.CreateRestorePoint(); break;
            case "36": BackupService.PrintRestorePoints(); break;
            case "37": BackupService.BackupNetwork(); break;
            case "38": BackupService.BackupRegistryKeys(); break;
            case "39": BackupService.ExportDrivers(); break;

            default:
                Console.WriteLine("Invalid option.");
                break;
        }
    }

    static void SfcMenu()
    {
        Console.WriteLine("[1] SFC /scannow");
        Console.WriteLine("[2] SFC /verifyonly + analyze");
        Console.Write("Select: ");
        var c = Console.ReadLine()?.Trim();
        if (c == "1") SfcRepair.ScanNow();
        else SfcRepair.AnalyzeAndOfferRepair();
    }

    static void DismMenu()
    {
        Console.WriteLine("[1] ScanHealth");
        Console.WriteLine("[2] RestoreHealth");
        Console.WriteLine("[3] Component Cleanup");
        Console.Write("Select: ");
        switch (Console.ReadLine()?.Trim())
        {
            case "2": DismRepair.RestoreHealth(); break;
            case "3": DismRepair.ComponentCleanup(); break;
            default: DismRepair.ScanHealth(); break;
        }
    }
}
