using Belkou.Core;

namespace Belkou.Diagnostics;

internal static class NetworkDiagnostic
{
    public static void FlushDns() => ProcessRunner.RunInteractive("ipconfig", "/flushdns");
    public static void ResetWinsock() => ProcessRunner.RunInteractive("netsh", "winsock reset");
    public static void ResetTcpIp() => ProcessRunner.RunInteractive("netsh", "int ip reset");

    public static void PrintConnectivity()
    {
        Console.WriteLine("DNS resolution (microsoft.com):");
        ProcessRunner.RunInteractive("nslookup", "microsoft.com");
        Console.WriteLine();
        Console.WriteLine("Ping gateway / internet:");
        var (_, gw) = ProcessRunner.CapturePowerShell(
            "(Get-NetIPConfiguration | Where-Object {$_.IPv4DefaultGateway -ne $null -and $_.NetAdapter.Status -eq 'Up'} | Select-Object -First 1).IPv4DefaultGateway.NextHop");
        if (!string.IsNullOrWhiteSpace(gw))
            ProcessRunner.RunInteractive("ping", $"-n 2 {gw.Trim()}");
        ProcessRunner.RunInteractive("ping", "-n 2 8.8.8.8");
        ProcessRunner.RunInteractive("ping", "-n 2 www.microsoft.com");
    }

    public static DiagnosticReport RunEngine(bool print = true)
    {
        var report = new DiagnosticReport();
        if (print)
        {
            Console.WriteLine("NETWORK DIAGNOSTICS");
            Console.WriteLine(new string('─', 40));
            Console.WriteLine();
        }

        var (_, adapter) = ProcessRunner.CapturePowerShell(
            "(Get-NetAdapter | Where-Object Status -eq 'Up' | Select-Object -First 1 Name,LinkSpeed | ForEach-Object { \"$($_.Name) ($($_.LinkSpeed))\" })");
        var adapterOk = !string.IsNullOrWhiteSpace(adapter);
        report.Checks.Add(Result("adapter", "Adapter detected", adapterOk, adapterOk ? adapter.Trim() : "No active adapter"));

        var (_, ip) = ProcessRunner.CapturePowerShell(
            "(Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.IPAddress -notlike '127.*' -and $_.PrefixOrigin -ne 'WellKnown' } | Select-Object -First 1 -ExpandProperty IPAddress)");
        report.Checks.Add(Result("ip", "IP address", !string.IsNullOrWhiteSpace(ip), string.IsNullOrWhiteSpace(ip) ? "None" : ip.Trim()));

        var (_, gw) = ProcessRunner.CapturePowerShell(
            "(Get-NetIPConfiguration | Where-Object {$_.IPv4DefaultGateway -ne $null -and $_.NetAdapter.Status -eq 'Up'} | Select-Object -First 1).IPv4DefaultGateway.NextHop");
        report.Checks.Add(Result("gateway-cfg", "Default gateway", !string.IsNullOrWhiteSpace(gw), string.IsNullOrWhiteSpace(gw) ? "None" : gw.Trim()));

        var (_, dns) = ProcessRunner.CapturePowerShell(
            "(Get-DnsClientServerAddress -AddressFamily IPv4 | Where-Object { $_.ServerAddresses } | Select-Object -First 1 -ExpandProperty ServerAddresses) -join ', '");
        report.Checks.Add(Result("dns-cfg", "DNS configuration", !string.IsNullOrWhiteSpace(dns), string.IsNullOrWhiteSpace(dns) ? "None" : dns.Trim()));

        var gwReach = false;
        var gwMs = "";
        if (!string.IsNullOrWhiteSpace(gw))
        {
            var (code, pingOut) = ProcessRunner.CaptureCmd($"ping -n 2 {gw.Trim()}");
            gwReach = code == 0 && pingOut.Contains("TTL=", StringComparison.OrdinalIgnoreCase);
            gwMs = ExtractAvgMs(pingOut);
        }
        report.Checks.Add(Result("gateway", "Gateway reachable", gwReach, gwReach ? $"{gwMs} ms" : "Unreachable",
            Severity.Medium, "Reset TCP/IP / check cable or Wi-Fi."));

        var (inetCode, inetOut) = ProcessRunner.CaptureCmd("ping -n 2 8.8.8.8");
        var inetOk = inetCode == 0 && inetOut.Contains("TTL=", StringComparison.OrdinalIgnoreCase);
        var inetMs = ExtractAvgMs(inetOut);
        report.Checks.Add(Result("internet", "Internet reachable", inetOk, inetOk ? $"{inetMs} ms" : "Unreachable",
            Severity.High, "Check ISP, firewall, or proxy settings."));

        var (dnsCode, _) = ProcessRunner.CaptureCmd("nslookup microsoft.com");
        report.Checks.Add(Result("dns", "DNS resolution", dnsCode == 0, dnsCode == 0 ? "OK" : "Failed",
            Severity.Medium, "Flush DNS or reset Winsock."));

        if (print)
        {
            foreach (var c in report.Checks)
                ConsoleUi.WriteCheck(c);

            Console.WriteLine();
            Console.WriteLine("Latency:");
            Console.WriteLine($"Gateway      {(string.IsNullOrEmpty(gwMs) ? "n/a" : gwMs + " ms")}");
            Console.WriteLine($"Internet     {(string.IsNullOrEmpty(inetMs) ? "n/a" : inetMs + " ms")}");
            Console.WriteLine($"DNS: {(dnsCode == 0 ? "OK" : "FAIL")}");
            Console.WriteLine($"Internet: {(inetOk ? "OK" : "FAIL")}");
        }

        return report;
    }

    static CheckResult Result(string id, string title, bool ok, string summary, Severity failSeverity = Severity.Medium, string? rec = null) =>
        new()
        {
            Id = id,
            Title = title,
            Passed = ok,
            Severity = ok ? Severity.Ok : failSeverity,
            Summary = summary,
            Recommendation = ok ? null : rec,
            RepairAction = ok ? null : "network"
        };

    static string ExtractAvgMs(string pingOutput)
    {
        var marker = "Average = ";
        var idx = pingOutput.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return "";
        var slice = pingOutput[(idx + marker.Length)..];
        var digits = new string(slice.TakeWhile(char.IsDigit).ToArray());
        return digits;
    }
}
