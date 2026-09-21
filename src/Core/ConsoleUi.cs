namespace Belkou.Core;

internal static class ConsoleUi
{
    public static void WriteBanner()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                         BELKOU                               ║");
        Console.WriteLine("║              WINDOWS DIAGNOSTIC & RECOVERY                  ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  {AppInfo.Product}  v{AppInfo.Version}");
        Console.ResetColor();
        Console.WriteLine();
    }

    public static void Section(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($" {title}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(" ─────────────────────────────────────────────");
        Console.ResetColor();
    }

    public static void Ok(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("[✓] ");
        Console.ResetColor();
        Console.WriteLine(message);
    }

    public static void Warn(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("[!] ");
        Console.ResetColor();
        Console.WriteLine(message);
    }

    public static void Fail(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("[✗] ");
        Console.ResetColor();
        Console.WriteLine(message);
    }

    public static void Info(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("[i] ");
        Console.ResetColor();
        Console.WriteLine(message);
    }

    public static void WriteCheck(CheckResult check)
    {
        if (check.Passed)
            Ok(check.Title + (string.IsNullOrWhiteSpace(check.Summary) ? "" : $" — {check.Summary}"));
        else if (check.Severity is Severity.High or Severity.Critical)
            Fail(check.Title + (string.IsNullOrWhiteSpace(check.Summary) ? "" : $" — {check.Summary}"));
        else
            Warn(check.Title + (string.IsNullOrWhiteSpace(check.Summary) ? "" : $" — {check.Summary}"));
    }

    public static bool Confirm(string prompt, bool defaultYes = false)
    {
        Console.Write($"{prompt} {(defaultYes ? "[Y/n]" : "[y/N]")} ");
        var input = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(input)) return defaultYes;
        return input.Equals("y", StringComparison.OrdinalIgnoreCase) ||
               input.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }

    public static void Pause()
    {
        Console.WriteLine();
        Console.Write("Press any key to continue...");
        Console.ReadKey(true);
    }

    public static string RiskLabel(Severity severity) => severity switch
    {
        Severity.Ok => "LOW",
        Severity.Info => "LOW",
        Severity.Low => "LOW",
        Severity.Medium => "MEDIUM",
        Severity.High => "HIGH",
        Severity.Critical => "CRITICAL",
        _ => "UNKNOWN"
    };
}
