using System.Diagnostics;
using System.Text;

namespace Belkou.Core;

internal static class ProcessRunner
{
    public static int RunInteractive(string fileName, string arguments = "")
    {
        var psi = new ProcessStartInfo(fileName, arguments)
        {
            UseShellExecute = true
        };
        using var p = Process.Start(psi);
        p?.WaitForExit();
        return p?.ExitCode ?? -1;
    }

    public static void StartShell(string fileName, string arguments = "")
    {
        Process.Start(new ProcessStartInfo(fileName, arguments) { UseShellExecute = true });
    }

    public static (int ExitCode, string Output) Capture(string fileName, string arguments, int timeoutMs = 120_000)
    {
        var psi = new ProcessStartInfo(fileName, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var p = Process.Start(psi)!;
        var stdout = p.StandardOutput.ReadToEnd();
        var stderr = p.StandardError.ReadToEnd();
        if (!p.WaitForExit(timeoutMs))
        {
            try { p.Kill(entireProcessTree: true); } catch { /* ignore */ }
            return (-1, stdout + Environment.NewLine + stderr + Environment.NewLine + "[timeout]");
        }

        return (p.ExitCode, (stdout + Environment.NewLine + stderr).Trim());
    }

    public static (int ExitCode, string Output) CaptureCmd(string command, int timeoutMs = 120_000) =>
        Capture("cmd.exe", "/c " + command, timeoutMs);

    public static (int ExitCode, string Output) CapturePowerShell(string script, int timeoutMs = 120_000) =>
        Capture(
            "powershell.exe",
            "-NoProfile -ExecutionPolicy Bypass -Command " + Quote(script),
            timeoutMs);

    static string Quote(string value) =>
        "\"" + value.Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";
}
