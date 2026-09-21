using Belkou.Backup;
using Belkou.Core;
using Belkou.Diagnostics;
using Belkou.Reports;

namespace Belkou.Repair;

internal static class RepairOrchestrator
{
    public static void RunInteractive()
    {
        Console.WriteLine("BELKOU REPAIR WORKFLOW");
        Console.WriteLine(new string('─', 40));
        Console.WriteLine();
        Console.WriteLine("This will:");
        Console.WriteLine("  1. Create a restore point");
        Console.WriteLine("  2. DISM ScanHealth");
        Console.WriteLine("  3. DISM RestoreHealth");
        Console.WriteLine("  4. SFC /scannow");
        Console.WriteLine("  5. Component Cleanup");
        Console.WriteLine("  6. Windows Update check");
        Console.WriteLine("  7. Final verification + report");
        Console.WriteLine();
        ConsoleUi.Warn("These operations can take a long time.");
        if (!ConsoleUi.Confirm("Proceed with repair workflow?", false))
            return;

        Console.WriteLine();
        Console.WriteLine("== Create Restore Point ==");
        BackupService.CreateRestorePoint("Belkou Repair Workflow");

        Console.WriteLine();
        Console.WriteLine("== DISM ScanHealth ==");
        DismRepair.ScanHealth();

        Console.WriteLine();
        Console.WriteLine("== DISM RestoreHealth ==");
        if (ConsoleUi.Confirm("Run DISM RestoreHealth now?", true))
            DismRepair.RestoreHealth();

        Console.WriteLine();
        Console.WriteLine("== SFC /scannow ==");
        if (ConsoleUi.Confirm("Run SFC /scannow now?", true))
            SfcRepair.ScanNow();

        Console.WriteLine();
        Console.WriteLine("== Component Cleanup ==");
        if (ConsoleUi.Confirm("Run component store cleanup?", true))
            DismRepair.ComponentCleanup();

        Console.WriteLine();
        Console.WriteLine("== Windows Update ==");
        WindowsUpdateRepair.Check();

        Console.WriteLine();
        Console.WriteLine("== Final verification ==");
        var report = DiagnosticEngine.Run(printProgress: true);
        var paths = ReportWriter.WriteAll(report);
        Console.WriteLine();
        ConsoleUi.Ok($"Report saved: {paths.HtmlPath}");
    }
}
