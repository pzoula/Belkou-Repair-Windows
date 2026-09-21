using Belkou.Backup;
using Belkou.Core;
using Belkou.Diagnostics;
using Belkou.Hardware;
using Belkou.Repair;
using Belkou.Reports;
using Belkou.Security;

namespace Belkou.CLI;

internal static class Commands
{
    public static void PrintHelp()
    {
        Console.WriteLine(AppInfo.Product);
        Console.WriteLine($"Version {AppInfo.Version}");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  belkou                         Open interactive menu");
        Console.WriteLine("  belkou diagnose [--json]       Auto diagnostic engine");
        Console.WriteLine("  belkou repair                  Guided repair workflow");
        Console.WriteLine("  belkou system                  System information");
        Console.WriteLine("  belkou hardware                Hardware inventory");
        Console.WriteLine("  belkou network                 Network diagnostics");
        Console.WriteLine("  belkou storage                 Storage / disk health");
        Console.WriteLine("  belkou events                  Event log analysis");
        Console.WriteLine("  belkou report [--json]         Generate diagnostic report");
        Console.WriteLine("  belkou backup drivers          Export drivers");
        Console.WriteLine("  belkou backup network          Backup network config");
        Console.WriteLine("  belkou backup registry         Backup key registry paths");
        Console.WriteLine("  belkou --version               Show version");
        Console.WriteLine("  belkou --help                  Show help");
    }

    public static int Execute(ParsedArgs args)
    {
        if (args.Version)
        {
            Console.WriteLine($"{AppInfo.Product} v{AppInfo.Version}");
            return 0;
        }

        if (args.Help)
        {
            PrintHelp();
            return 0;
        }

        return args.Command switch
        {
            "menu" => RunMenu(),
            "diagnose" => Diagnose(args.Json),
            "repair" => Repair(),
            "system" => System(),
            "hardware" => Hardware(),
            "network" => Network(),
            "storage" => Storage(),
            "events" => Events(),
            "report" => Report(args.Json),
            "backup" => Backup(args.SubCommand),
            "security" => Security(),
            _ => Unknown(args.Command)
        };
    }

    static int RunMenu()
    {
        Menu.Run();
        return 0;
    }

    static int Diagnose(bool json)
    {
        var report = DiagnosticEngine.Run(printProgress: !json);
        if (json)
        {
            Console.WriteLine(ReportWriter.ToJson(report));
            return 0;
        }

        Console.WriteLine();
        if (ConsoleUi.Confirm("Generate HTML/JSON/text report?", true))
        {
            var paths = ReportWriter.WriteAll(report);
            ConsoleUi.Ok(paths.HtmlPath);
            ProcessRunner.StartShell("explorer.exe", paths.Directory);
        }

        var repairable = report.Issues.FirstOrDefault(i => i.RepairAction is "dism" or "dism-sfc");
        if (repairable is not null &&
            ConsoleUi.Confirm("Would you like Belkou to repair detected component/system issues automatically?", false))
        {
            RepairOrchestrator.RunInteractive();
        }

        return 0;
    }

    static int Repair()
    {
        RepairOrchestrator.RunInteractive();
        return 0;
    }

    static int System()
    {
        SystemDiagnostic.PrintSystemInfo();
        return 0;
    }

    static int Hardware()
    {
        HardwareInventory.Print();
        return 0;
    }

    static int Network()
    {
        NetworkDiagnostic.RunEngine(print: true);
        return 0;
    }

    static int Storage()
    {
        StorageDiagnostic.PrintDiskHealth();
        Console.WriteLine();
        StorageDiagnostic.PrintSmart();
        return 0;
    }

    static int Events()
    {
        EventDiagnostic.Analyze(interactive: true);
        return 0;
    }

    static int Report(bool json)
    {
        var report = DiagnosticEngine.Run(printProgress: !json);
        if (json)
        {
            Console.WriteLine(ReportWriter.ToJson(report));
            return 0;
        }

        var paths = ReportWriter.WriteAll(report);
        ConsoleUi.Ok($"HTML: {paths.HtmlPath}");
        ConsoleUi.Ok($"JSON: {paths.JsonPath}");
        ConsoleUi.Ok($"TEXT: {paths.TextPath}");
        ProcessRunner.StartShell("explorer.exe", paths.Directory);
        return 0;
    }

    static int Backup(string? sub)
    {
        switch (sub)
        {
            case "drivers":
                BackupService.ExportDrivers();
                break;
            case "network":
                BackupService.BackupNetwork();
                break;
            case "registry":
                BackupService.BackupRegistryKeys();
                break;
            case "system":
            case "systeminfo":
                BackupService.ExportSystemInfo();
                break;
            default:
                Console.WriteLine("Usage: belkou backup [drivers|network|registry|system]");
                BackupService.ExportSystemInfo();
                break;
        }
        return 0;
    }

    static int Security()
    {
        ConsoleUi.Section("WINDOWS DEFENDER");
        DefenderStatus.Print();
        ConsoleUi.Section("FIREWALL");
        FirewallStatus.Print();
        return 0;
    }

    static int Unknown(string command)
    {
        Console.WriteLine($"Unknown command: {command}");
        Console.WriteLine("Run belkou --help for usage.");
        return 1;
    }
}
