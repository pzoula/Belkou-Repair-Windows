using Belkou.Core;
using Belkou.Hardware;
using Belkou.Security;

namespace Belkou.Diagnostics;

internal static class DiagnosticEngine
{
    public static DiagnosticReport Run(bool printProgress = true)
    {
        var report = new DiagnosticReport();
        if (printProgress)
        {
            Console.WriteLine("BELKOU DIAGNOSTIC ENGINE");
            Console.WriteLine(new string('─', 40));
            Console.WriteLine();
        }

        void Add(CheckResult check)
        {
            report.Checks.Add(check);
            if (printProgress) ConsoleUi.WriteCheck(check);
        }

        if (printProgress) Console.WriteLine("Analyzing...");
        Console.WriteLine();

        Add(SystemDiagnostic.WindowsVersion());

        var cpu = CpuInfo.Summary();
        Add(new CheckResult
        {
            Id = "cpu",
            Title = "CPU",
            Passed = !cpu.Equals("Unavailable", StringComparison.OrdinalIgnoreCase),
            Severity = Severity.Ok,
            Summary = cpu.Replace('\n', ' ')
        });

        var mem = MemoryInfo.Get();
        var memOk = mem.AvailableGb > 1;
        Add(new CheckResult
        {
            Id = "ram",
            Title = "RAM",
            Passed = memOk,
            Severity = memOk ? Severity.Ok : Severity.Medium,
            Summary = $"{mem.TotalGb:0.#} GB total, {mem.AvailableGb:0.#} GB available",
            Recommendation = memOk ? null : "Close heavy applications or add memory."
        });

        Add(StorageDiagnostic.HealthCheck());
        Add(StorageDiagnostic.DiskSpaceCheck());

        // Full SFC verifyonly is slow; diagnose uses DISM CheckHealth as the integrity signal.
        // Deep SFC remains available via menu / belkou repair.
        Add(DismProbe());
        Add(SystemFilesFromComponentStore(report));

        var net = NetworkDiagnostic.RunEngine(print: false);
        foreach (var c in net.Checks.Where(c => c.Id is "adapter" or "dns" or "internet"))
            Add(c);

        Add(WindowsUpdateProbe());
        Add(SystemDiagnostic.FailedServices());
        Add(SystemDiagnostic.DriversQuick());
        Add(EventDiagnostic.QuickCheck());
        Add(DefenderStatus.Check());

        foreach (var issue in report.Issues)
        {
            if (!string.IsNullOrWhiteSpace(issue.Recommendation) &&
                !report.Recommendations.Contains(issue.Recommendation))
                report.Recommendations.Add(issue.Recommendation);
        }

        foreach (var kv in HardwareInventory.ToDictionary())
            report.Meta[kv.Key] = kv.Value;

        report.Meta["os"] = report.Checks.FirstOrDefault(c => c.Id == "windows-version")?.Summary;
        report.Meta["risk"] = ConsoleUi.RiskLabel(report.OverallRisk);

        if (printProgress)
            PrintSummary(report);

        return report;
    }

    public static void PrintSummary(DiagnosticReport report)
    {
        Console.WriteLine();
        Console.WriteLine("Potential issues detected:");
        Console.WriteLine();
        var issues = report.Issues.ToList();
        if (issues.Count == 0)
        {
            ConsoleUi.Ok("No significant issues detected.");
        }
        else
        {
            foreach (var issue in issues)
                ConsoleUi.Warn($"{issue.Title}: {issue.Summary}");
        }

        Console.WriteLine();
        Console.ForegroundColor = report.OverallRisk >= Severity.High ? ConsoleColor.Red :
            report.OverallRisk >= Severity.Medium ? ConsoleColor.Yellow : ConsoleColor.Green;
        Console.WriteLine($"Risk: {ConsoleUi.RiskLabel(report.OverallRisk)}");
        Console.ResetColor();
        Console.WriteLine();

        if (report.Recommendations.Count == 0) return;

        Console.WriteLine("Recommended actions:");
        for (var i = 0; i < report.Recommendations.Count; i++)
            Console.WriteLine($"  [{i + 1}] {report.Recommendations[i]}");
    }

    static CheckResult SystemFilesFromComponentStore(DiagnosticReport report)
    {
        var store = report.Checks.FirstOrDefault(c => c.Id == "component-store");
        if (store is null)
        {
            return new CheckResult
            {
                Id = "system-files",
                Title = "System files",
                Passed = true,
                Severity = Severity.Inconclusive,
                Summary = "Deferred to repair workflow"
            };
        }

        return new CheckResult
        {
            Id = "system-files",
            Title = "System files",
            Passed = store.Passed,
            Severity = store.Severity,
            Summary = store.Passed ? "Healthy (via component store check)" : "Possible integrity issue — run SFC after DISM",
            Recommendation = store.Passed ? null : "Run DISM RestoreHealth before running SFC again.",
            RepairAction = store.Passed ? null : "dism-sfc"
        };
    }

    static CheckResult DismProbe()
    {
        var (code, output) = ProcessRunner.CaptureCmd("DISM /Online /Cleanup-Image /CheckHealth", timeoutMs: 180_000);
        var repairable = output.Contains("repairable", StringComparison.OrdinalIgnoreCase);
        var noIssue = output.Contains("No component store corruption detected", StringComparison.OrdinalIgnoreCase);
        return new CheckResult
        {
            Id = "component-store",
            Title = "Component store",
            Passed = noIssue && !repairable,
            Severity = repairable ? Severity.High : noIssue ? Severity.Ok : Severity.Inconclusive,
            Summary = repairable ? "Repairable corruption" : noIssue ? "Healthy" : $"Check exit {code}",
            Details = Truncate(output),
            Recommendation = repairable ? "Repair component store (DISM RestoreHealth)." : null,
            RepairAction = repairable ? "dism" : null
        };
    }

    static CheckResult WindowsUpdateProbe()
    {
        var (_, output) = ProcessRunner.CapturePowerShell(
            "try { (New-Object -ComObject Microsoft.Update.AutoUpdate).Results.LastSearchSuccessDate } catch { 'n/a' }");
        return new CheckResult
        {
            Id = "windows-update",
            Title = "Windows Update",
            Passed = true,
            Severity = Severity.Info,
            Summary = string.IsNullOrWhiteSpace(output) ? "Status available via Settings" : $"Last search: {output.Trim()}",
            Recommendation = null
        };
    }

    static string Truncate(string text, int max = 800) =>
        string.IsNullOrEmpty(text) ? "" : text.Length <= max ? text : text[..max] + "...";
}
