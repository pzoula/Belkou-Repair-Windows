using Belkou.Core;

namespace Belkou.Security;

internal static class FirewallStatus
{
    public static void Print()
    {
        var (_, output) = ProcessRunner.CapturePowerShell(
            "Get-NetFirewallProfile | Select-Object Name,Enabled | Format-Table -AutoSize | Out-String");
        Console.WriteLine(string.IsNullOrWhiteSpace(output) ? "Firewall status unavailable." : output.Trim());
    }

    public static CheckResult Check()
    {
        var (_, output) = ProcessRunner.CapturePowerShell(
            "(Get-NetFirewallProfile | ForEach-Object { \"$($_.Name)=$($_.Enabled)\" }) -join '; '");
        var disabled = output.Contains("=False", StringComparison.OrdinalIgnoreCase);
        return new CheckResult
        {
            Id = "firewall",
            Title = "Firewall",
            Passed = !disabled,
            Severity = disabled ? Severity.Medium : Severity.Ok,
            Summary = disabled ? "One or more profiles disabled" : "Profiles enabled",
            Details = output,
            Recommendation = disabled ? "Enable Windows Firewall for Domain/Private/Public profiles." : null
        };
    }
}
