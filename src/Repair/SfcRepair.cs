using Belkou.Core;

namespace Belkou.Repair;

internal static class SfcRepair
{
    public static int ScanNow() => ProcessRunner.RunInteractive("sfc", "/scannow");
    public static int VerifyOnly() => ProcessRunner.RunInteractive("sfc", "/verifyonly");

    public static void AnalyzeAndOfferRepair()
    {
        Console.WriteLine("Running SFC /verifyonly...");
        var (code, output) = ProcessRunner.CaptureCmd("sfc /verifyonly", timeoutMs: 300_000);
        Console.WriteLine(output);
        Console.WriteLine($"Exit code: {code}");

        if (output.Contains("corrupt", StringComparison.OrdinalIgnoreCase) ||
            output.Contains("integrity violations", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine();
            ConsoleUi.Fail("SYSTEM FILE CORRUPTION DETECTED");
            Console.WriteLine("Severity: HIGH");
            Console.WriteLine();
            Console.WriteLine("Recommended:");
            Console.WriteLine("Run DISM RestoreHealth before running SFC again.");
            Console.WriteLine();
            if (ConsoleUi.Confirm("Would you like Belkou to repair this automatically?", true))
            {
                DismRepair.RestoreHealth();
                ScanNow();
            }
        }
        else if (output.Contains("did not find any integrity violations", StringComparison.OrdinalIgnoreCase))
        {
            ConsoleUi.Ok("No integrity violations found.");
        }
        else
        {
            ConsoleUi.Info("SFC result inconclusive — review output above.");
        }
    }
}
