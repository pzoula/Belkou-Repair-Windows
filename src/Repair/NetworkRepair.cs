using Belkou.Core;
using Belkou.Diagnostics;

namespace Belkou.Repair;

internal static class NetworkRepair
{
    public static void FlushDns() => NetworkDiagnostic.FlushDns();
    public static void ResetWinsock() => NetworkDiagnostic.ResetWinsock();
    public static void ResetTcpIp() => NetworkDiagnostic.ResetTcpIp();

    public static void FullReset()
    {
        ConsoleUi.Warn("Network reset may briefly interrupt connectivity and can require a reboot.");
        if (!ConsoleUi.Confirm("Continue with DNS flush + Winsock + TCP/IP reset?", false))
            return;
        FlushDns();
        ResetWinsock();
        ResetTcpIp();
        ConsoleUi.Info("Reboot recommended.");
    }
}
