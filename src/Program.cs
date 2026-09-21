using System.Diagnostics;
using System.Security.Principal;
using Belkou.CLI;
using Belkou.Core;

namespace Belkou;

internal static class Program
{
    static int Main(string[] args)
    {
        var parsed = ArgumentParser.Parse(args);

        // Version / help never require elevation.
        if (parsed.Version)
        {
            Console.WriteLine($"{AppInfo.Product} v{AppInfo.Version}");
            return 0;
        }

        if (parsed.Help || args.Length == 1 && args[0].Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            Commands.PrintHelp();
            return 0;
        }

        if (!EnsureAdministrator(args))
            return 0;

        return Commands.Execute(parsed);
    }

    static bool EnsureAdministrator(string[] args)
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            return true;

        var psi = new ProcessStartInfo
        {
            FileName = Environment.ProcessPath!,
            UseShellExecute = true,
            Verb = "runas",
            Arguments = string.Join(' ', args.Select(QuoteArg))
        };

        try
        {
            Process.Start(psi);
            return false;
        }
        catch
        {
            Console.WriteLine("Administrator privileges are required.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
            Environment.Exit(1);
            return false;
        }
    }

    static string QuoteArg(string arg) =>
        arg.Contains(' ', StringComparison.Ordinal) ? $"\"{arg}\"" : arg;
}
