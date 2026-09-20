using System.Diagnostics;
using System.Security.Principal;

namespace Belkou;

internal class Program
{
    const string Version = "1.0.0";

    static void Main(string[] args)
    {
        if (args.Any(a => a.Equals("--version", StringComparison.OrdinalIgnoreCase) ||
                          a.Equals("-v", StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine($"Belkou Recovery Repair Toolkit v{Version}");
            return;
        }

        if (args.Any(a => a.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
                          a.Equals("-h", StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("Belkou Recovery Repair Toolkit");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  belkou             Open the recovery toolkit menu");
            Console.WriteLine("  belkou --version   Show version");
            Console.WriteLine("  belkou --help      Show help");
            return;
        }

        EnsureAdministrator();
        Menu();
    }

    static void EnsureAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);

        if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            return;

        var psi = new ProcessStartInfo
        {
            FileName = Environment.ProcessPath!,
            UseShellExecute = true,
            Verb = "runas"
        };

        try
        {
            Process.Start(psi);
            Environment.Exit(0);
        }
        catch
        {
            Console.WriteLine("Administrator privileges are required.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(1);
        }
    }

    static void Header()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;

        // BELKOU ASCII/outline terminal logo.
        Console.WriteLine(@"  ____  _____ _      _  __  ____  _   _");
        Console.WriteLine(@" | __ )| ____| |    | |/ / |  _ \| | | |");
        Console.WriteLine(@" |  _ \|  _| | |    | ' /  | |_) | | | |");
        Console.WriteLine(@" | |_) | |___| |___ | . \  |  __/| |_| |");
        Console.WriteLine(@" |____/|_____|_____|_| \_\ |_|    \___/");

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("   Welcome to BELKOU Recovery Repair Toolkit!");
        Console.WriteLine($"   Version {Version} | Windows System Repair");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine(new string('=', 64));
        Console.WriteLine();
    }

    static void Menu()
    {
        while (true)
        {
            Header();

            Console.WriteLine(" [1]  System Info             [10] Reset TCP/IP");
            Console.WriteLine(" [2]  SFC Scan                [11] Battery Report");
            Console.WriteLine(" [3]  SFC Verify              [12] Performance Report");
            Console.WriteLine(" [4]  DISM Scan               [13] WinRE Info");
            Console.WriteLine(" [5]  DISM Repair             [14] System Restore");
            Console.WriteLine(" [6]  Component Cleanup       [15] Memory Diagnostic");
            Console.WriteLine(" [7]  Drive Health            [16] Advanced Startup");
            Console.WriteLine(" [8]  Flush DNS               [17] Windows Update");
            Console.WriteLine(" [9]  Reset Winsock            [18] Full Report");
            Console.WriteLine(" [Q]  Exit");
            Console.WriteLine();
            Console.Write(" Select [1-18/Q]: ");

            var choice = Console.ReadLine()?.Trim().ToUpperInvariant();

            if (choice == "Q")
                return;

            RunChoice(choice);
            Console.WriteLine();
            Console.Write("Press any key to return to the menu...");
            Console.ReadKey(true);
        }
    }

    static void RunChoice(string? choice)
    {
        switch (choice)
        {
            case "1": Run("systeminfo"); break;
            case "2": Run("sfc", "/scannow"); break;
            case "3": Run("sfc", "/verifyonly"); break;
            case "4": Run("DISM", "/Online /Cleanup-Image /ScanHealth"); break;
            case "5": Run("DISM", "/Online /Cleanup-Image /RestoreHealth"); break;
            case "6": Run("DISM", "/Online /Cleanup-Image /StartComponentCleanup"); break;
            case "7":
                RunPowerShell("Get-PhysicalDisk | Format-Table -AutoSize; Get-Volume | Format-Table -AutoSize");
                break;
            case "8": Run("ipconfig", "/flushdns"); break;
            case "9": Run("netsh", "winsock reset"); break;
            case "10": Run("netsh", "int ip reset"); break;
            case "11":
                Run("powercfg", $"/batteryreport /output \"{Path.Combine(Path.GetTempPath(), "Belkou-BatteryReport.html")}\"");
                Console.WriteLine($"Battery report: {Path.Combine(Path.GetTempPath(), "Belkou-BatteryReport.html")}");
                break;
            case "12": Run("perfmon", "/report"); break;
            case "13": Run("reagentc", "/info"); break;
            case "14": Run("rstrui.exe"); break;
            case "15": Run("mdsched.exe"); break;
            case "16": Run("shutdown", "/r /o /f /t 0"); break;
            case "17": Process.Start(new ProcessStartInfo("ms-settings:windowsupdate") { UseShellExecute = true }); break;
            case "18": FullReport(); break;
            default: Console.WriteLine("Invalid option."); break;
        }
    }

    static void Run(string file, string args = "")
    {
        var psi = new ProcessStartInfo(file, args)
        {
            UseShellExecute = true
        };
        using var p = Process.Start(psi);
        p?.WaitForExit();
    }

    static void RunPowerShell(string command)
    {
        Run("powershell.exe", $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"");
    }

    static void FullReport()
    {
        var dir = Path.Combine(Path.GetTempPath(), "Belkou-RecoveryRepairToolkit");
        Directory.CreateDirectory(dir);

        File.WriteAllText(Path.Combine(dir, "systeminfo.txt"), Capture("systeminfo"));
        File.WriteAllText(Path.Combine(dir, "sfc.txt"), Capture("sfc /verifyonly"));
        File.WriteAllText(Path.Combine(dir, "dism.txt"), Capture("DISM /Online /Cleanup-Image /ScanHealth"));
        File.WriteAllText(Path.Combine(dir, "winre.txt"), Capture("reagentc /info"));
        File.WriteAllText(Path.Combine(dir, "network.txt"), Capture("ipconfig /all"));

        Console.WriteLine($"Full report saved to:");
        Console.WriteLine(dir);
        Process.Start(new ProcessStartInfo("explorer.exe", dir) { UseShellExecute = true });
    }

    static string Capture(string command)
    {
        var psi = new ProcessStartInfo("cmd.exe", $"/c {command}")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var p = Process.Start(psi)!;
        var output = p.StandardOutput.ReadToEnd();
        var error = p.StandardError.ReadToEnd();
        p.WaitForExit();
        return output + Environment.NewLine + error;
    }
}
