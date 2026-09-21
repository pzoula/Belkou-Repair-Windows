using Belkou.Core;

namespace Belkou.Security;

internal static class DefenderStatus
{
    public static void Print()
    {
        var (_, output) = ProcessRunner.CapturePowerShell(
            "try { Get-MpComputerStatus | Select-Object AMServiceEnabled,AntispywareEnabled,AntivirusEnabled,RealTimeProtectionEnabled,NISEnabled,IoavProtectionEnabled,BehaviorMonitorEnabled,AntivirusSignatureLastUpdated | Format-List | Out-String } catch { $_.Exception.Message }");
        Console.WriteLine(string.IsNullOrWhiteSpace(output) ? "Windows Defender status unavailable." : output.Trim());
    }

    public static CheckResult Check()
    {
        var (_, output) = ProcessRunner.CapturePowerShell(
            "try { $s=Get-MpComputerStatus; \"$($s.AMServiceEnabled)|$($s.RealTimeProtectionEnabled)\" } catch { 'error' }");
        if (output.Contains("error", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(output))
        {
            return new CheckResult
            {
                Id = "defender",
                Title = "Security status",
                Passed = true,
                Severity = Severity.Inconclusive,
                Summary = "Could not query Defender"
            };
        }

        var parts = output.Split('|');
        var ok = parts.Length >= 2 &&
                 parts[0].Equals("True", StringComparison.OrdinalIgnoreCase) &&
                 parts[1].Equals("True", StringComparison.OrdinalIgnoreCase);

        return new CheckResult
        {
            Id = "defender",
            Title = "Security status",
            Passed = ok,
            Severity = ok ? Severity.Ok : Severity.Medium,
            Summary = ok ? "Defender real-time protection on" : "Defender protection may be disabled",
            Recommendation = ok ? null : "Enable Microsoft Defender real-time protection.",
            Details = output
        };
    }
}
