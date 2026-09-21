using Belkou.Core;

namespace Belkou.Backup;

internal static class BackupService
{
    public static string BackupRoot
    {
        get
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Belkou-Backup");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    public static void CreateRestorePoint(string description = "Belkou Restore Point")
    {
        var (_, output) = ProcessRunner.CapturePowerShell(
            $"Checkpoint-Computer -Description '{description.Replace("'", "''")}' -RestorePointType MODIFY_SETTINGS -ErrorAction SilentlyContinue; 'done'");
        Console.WriteLine(string.IsNullOrWhiteSpace(output) ? "Restore point requested." : output.Trim());
        ConsoleUi.Info("If System Protection is disabled, enable it in System Properties.");
    }

    public static void PrintRestorePoints()
    {
        var (_, output) = ProcessRunner.CapturePowerShell(
            "Get-ComputerRestorePoint | Select-Object SequenceNumber,Description,CreationTime,RestorePointType | Format-Table -AutoSize | Out-String");
        Console.WriteLine(string.IsNullOrWhiteSpace(output) ? "No restore points found." : output.Trim());
    }

    public static void BackupNetwork()
    {
        var dir = Path.Combine(BackupRoot, "Network", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        Directory.CreateDirectory(dir);
        var (c1, ip) = ProcessRunner.CaptureCmd("ipconfig /all");
        File.WriteAllText(Path.Combine(dir, "ipconfig.txt"), ip);
        var (_, routes) = ProcessRunner.CaptureCmd("route print");
        File.WriteAllText(Path.Combine(dir, "routes.txt"), routes);
        var (_, dns) = ProcessRunner.CapturePowerShell("Get-DnsClientServerAddress | Format-List | Out-String");
        File.WriteAllText(Path.Combine(dir, "dns.txt"), dns);
        ConsoleUi.Ok($"Network backup saved: {dir}");
    }

    public static void BackupRegistryKeys()
    {
        var dir = Path.Combine(BackupRoot, "Registry", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        Directory.CreateDirectory(dir);
        var keys = new[]
        {
            @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion",
            @"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters"
        };
        foreach (var key in keys)
        {
            var safe = string.Join("_", key.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
            var path = Path.Combine(dir, safe + ".reg");
            ProcessRunner.CaptureCmd($"reg export \"{key}\" \"{path}\" /y");
        }
        ConsoleUi.Ok($"Registry backup saved: {dir}");
    }

    public static void ExportDrivers()
    {
        var dir = Path.Combine(BackupRoot, "Drivers", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        Directory.CreateDirectory(dir);
        Console.WriteLine("Exporting third-party drivers (this may take a while)...");
        var (code, output) = ProcessRunner.CaptureCmd($"dism /online /export-driver /destination:\"{dir}\"", timeoutMs: 600_000);
        File.WriteAllText(Path.Combine(dir, "export-log.txt"), output);
        if (code == 0)
            ConsoleUi.Ok($"Drivers exported: {dir}");
        else
            ConsoleUi.Warn($"Driver export finished with code {code}. See export-log.txt");
    }

    public static void ExportSystemInfo()
    {
        var dir = Path.Combine(BackupRoot, "SystemInfo", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        Directory.CreateDirectory(dir);
        var (_, info) = ProcessRunner.CaptureCmd("systeminfo");
        File.WriteAllText(Path.Combine(dir, "systeminfo.txt"), info);
        ConsoleUi.Ok($"System info saved: {dir}");
    }
}
