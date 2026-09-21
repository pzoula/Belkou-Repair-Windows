using Belkou.Core;

namespace Belkou.Diagnostics;

internal static class EventDiagnostic
{
    public static void Analyze(bool interactive = true)
    {
        Console.WriteLine("EVENT LOG ANALYSIS");
        Console.WriteLine(new string('─', 28));
        Console.WriteLine();

        var (_, counts) = ProcessRunner.CapturePowerShell(@"
$start=(Get-Date).AddDays(-7)
$logs=@('System','Application')
$crit=0;$err=0;$warn=0;$info=0
foreach($l in $logs){
  $crit += @(Get-WinEvent -FilterHashtable @{LogName=$l; Level=1; StartTime=$start} -ErrorAction SilentlyContinue).Count
  $err  += @(Get-WinEvent -FilterHashtable @{LogName=$l; Level=2; StartTime=$start} -ErrorAction SilentlyContinue).Count
  $warn += @(Get-WinEvent -FilterHashtable @{LogName=$l; Level=3; StartTime=$start} -ErrorAction SilentlyContinue).Count
  $info += @(Get-WinEvent -FilterHashtable @{LogName=$l; Level=4; StartTime=$start} -ErrorAction SilentlyContinue | Select-Object -First 5000).Count
}
""$crit|$err|$warn|$info""
");
        var parts = counts.Split('|');
        int.TryParse(parts.ElementAtOrDefault(0), out var crit);
        int.TryParse(parts.ElementAtOrDefault(1), out var err);
        int.TryParse(parts.ElementAtOrDefault(2), out var warn);
        int.TryParse(parts.ElementAtOrDefault(3), out var info);

        Console.WriteLine($"Critical     : {crit}");
        Console.WriteLine($"Errors       : {err}");
        Console.WriteLine($"Warnings     : {warn}");
        Console.WriteLine($"Information  : {info}+ (sampled)");
        Console.WriteLine();

        var (_, top) = ProcessRunner.CapturePowerShell(@"
$start=(Get-Date).AddDays(-7)
Get-WinEvent -FilterHashtable @{LogName='System','Application'; Level=2; StartTime=$start} -ErrorAction SilentlyContinue |
  Group-Object ProviderName | Sort-Object Count -Descending | Select-Object -First 5 |
  ForEach-Object { ""$($_.Count)|$($_.Name)"" }
");
        Console.WriteLine("Top recurring errors:");
        var lines = top.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var i = 1;
        foreach (var line in lines.Take(5))
        {
            var p = line.Split('|', 2);
            Console.WriteLine($"[{i}] {(p.Length > 1 ? p[1] : line)} ({(p.Length > 0 ? p[0] : "?")} occurrences)");
            i++;
        }

        if (lines.Length > 0 && lines[0].Contains("Kernel-Power", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine();
            Console.WriteLine("Kernel-Power");
            Console.WriteLine("Possible causes:");
            Console.WriteLine("- Unexpected shutdown");
            Console.WriteLine("- Power interruption");
            Console.WriteLine("- Hardware instability");
        }

        if (!interactive) return;

        Console.WriteLine();
        Console.WriteLine("[1] Investigate top provider");
        Console.WriteLine("[2] Export recent errors");
        Console.WriteLine("[3] Back");
        Console.Write("Select: ");
        var choice = Console.ReadLine()?.Trim();
        if (choice == "1" && lines.Length > 0)
        {
            var provider = lines[0].Split('|', 2).ElementAtOrDefault(1) ?? "";
            ProcessRunner.RunInteractive("powershell.exe",
                $"-NoProfile -ExecutionPolicy Bypass -Command \"Get-WinEvent -FilterHashtable @{{LogName='System';'Application'; ProviderName='{provider.Replace("'", "''")}'; Level=2; StartTime=(Get-Date).AddDays(-7)}} -ErrorAction SilentlyContinue | Select-Object -First 20 TimeCreated,Id,Message | Format-List\"");
        }
        else if (choice == "2")
        {
            var dir = Path.Combine(Path.GetTempPath(), "Belkou-Events");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, $"errors-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
            var (_, dump) = ProcessRunner.CapturePowerShell(
                "Get-WinEvent -FilterHashtable @{LogName='System','Application'; Level=2; StartTime=(Get-Date).AddDays(-7)} -ErrorAction SilentlyContinue | Select-Object -First 200 TimeCreated,ProviderName,Id,Message | Format-List | Out-String");
            File.WriteAllText(path, dump);
            Console.WriteLine($"Exported: {path}");
            ProcessRunner.StartShell("explorer.exe", dir);
        }
    }

    public static CheckResult QuickCheck()
    {
        var (_, output) = ProcessRunner.CapturePowerShell(
            "@(Get-WinEvent -FilterHashtable @{LogName='System'; Level=1; StartTime=(Get-Date).AddDays(-3)} -ErrorAction SilentlyContinue).Count");
        _ = int.TryParse(output.Trim(), out var crit);
        return new CheckResult
        {
            Id = "event-logs",
            Title = "Event logs",
            Passed = crit == 0,
            Severity = crit == 0 ? Severity.Ok : crit > 5 ? Severity.High : Severity.Medium,
            Summary = crit == 0 ? "No critical events (3 days)" : $"{crit} critical event(s) in 3 days",
            Recommendation = crit == 0 ? null : "Run belkou events for detailed analysis."
        };
    }
}
