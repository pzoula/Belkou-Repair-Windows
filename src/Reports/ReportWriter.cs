using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Belkou.Core;

namespace Belkou.Reports;

internal static class ReportWriter
{
    public sealed record Paths(string Directory, string HtmlPath, string JsonPath, string TextPath);

    public static string ReportsRoot
    {
        get
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Belkou-Reports");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    public static Paths WriteAll(DiagnosticReport report)
    {
        var stamp = report.GeneratedAt.ToString("yyyy-MM-dd");
        var dir = ReportsRoot;
        var html = Path.Combine(dir, $"Belkou-Diagnostic-{stamp}.html");
        var json = Path.Combine(dir, $"Belkou-Diagnostic-{stamp}.json");
        var text = Path.Combine(dir, $"Belkou-Diagnostic-{stamp}.txt");

        File.WriteAllText(html, ToHtml(report), Encoding.UTF8);
        File.WriteAllText(json, ToJson(report), Encoding.UTF8);
        File.WriteAllText(text, ToText(report), Encoding.UTF8);
        return new Paths(dir, html, json, text);
    }

    public static string ToJson(DiagnosticReport report)
    {
        var payload = new
        {
            generatedAt = report.GeneratedAt,
            version = report.Version,
            risk = ConsoleUi.RiskLabel(report.OverallRisk),
            system = new
            {
                os = report.Meta.GetValueOrDefault("os"),
                architecture = Environment.Is64BitOperatingSystem ? "x64" : "x86"
            },
            memory = new
            {
                totalGB = report.Meta.GetValueOrDefault("memoryTotalGb"),
                availableGB = report.Meta.GetValueOrDefault("memoryAvailableGb")
            },
            storage = new
            {
                health = report.Meta.GetValueOrDefault("storageHealth"),
                freeGB = report.Meta.GetValueOrDefault("systemDriveFreeGb")
            },
            network = new
            {
                internet = report.Checks.FirstOrDefault(c => c.Id == "internet")?.Passed
            },
            sfc = new
            {
                status = report.Checks.FirstOrDefault(c => c.Id == "system-files")?.Summary
            },
            checks = report.Checks.Select(c => new
            {
                id = c.Id,
                title = c.Title,
                passed = c.Passed,
                severity = c.Severity.ToString(),
                summary = c.Summary,
                recommendation = c.Recommendation
            }),
            recommendations = report.Recommendations
        };

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }

    public static string ToText(DiagnosticReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("BELKOU SYSTEM REPORT");
        sb.AppendLine($"Generated: {report.GeneratedAt:u}");
        sb.AppendLine($"Version: {report.Version}");
        sb.AppendLine($"Risk: {ConsoleUi.RiskLabel(report.OverallRisk)}");
        sb.AppendLine();
        sb.AppendLine("CHECKS");
        sb.AppendLine(new string('-', 40));
        foreach (var c in report.Checks)
            sb.AppendLine($"{(c.Passed ? "[OK]" : "[!]")} {c.Title}: {c.Summary}");
        sb.AppendLine();
        sb.AppendLine("RECOMMENDATIONS");
        sb.AppendLine(new string('-', 40));
        if (report.Recommendations.Count == 0)
            sb.AppendLine("None");
        else
            foreach (var r in report.Recommendations)
                sb.AppendLine("- " + r);
        return sb.ToString();
    }

    public static string ToHtml(DiagnosticReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"/>");
        sb.AppendLine($"<title>Belkou System Report</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:2rem;background:#0f1419;color:#e7ecf1;}h1,h2{color:#5ad1e6;}table{border-collapse:collapse;width:100%;margin:1rem 0;}th,td{border:1px solid #2a3540;padding:.5rem;text-align:left;}th{background:#1a2330;}.ok{color:#3ddc97;}.bad{color:#ff6b6b;}.meta{color:#9aa7b5;}</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine("<h1>BELKOU SYSTEM REPORT</h1>");
        sb.AppendLine($"<p class=\"meta\">{report.GeneratedAt:u} · v{report.Version} · Risk: <strong>{ConsoleUi.RiskLabel(report.OverallRisk)}</strong></p>");

        sb.AppendLine("<h2>SYSTEM</h2><pre>" + Esc(report.Meta.GetValueOrDefault("os")?.ToString() ?? "") + "</pre>");
        sb.AppendLine("<h2>HARDWARE</h2><pre>");
        sb.AppendLine("CPU: " + Esc(report.Meta.GetValueOrDefault("cpu")?.ToString() ?? ""));
        sb.AppendLine("RAM: " + Esc($"{report.Meta.GetValueOrDefault("memoryTotalGb")} GB (available {report.Meta.GetValueOrDefault("memoryAvailableGb")} GB)"));
        sb.AppendLine("GPU: " + Esc(report.Meta.GetValueOrDefault("gpu")?.ToString() ?? ""));
        sb.AppendLine("</pre>");

        sb.AppendLine("<h2>STORAGE</h2><pre>");
        sb.AppendLine(Esc(report.Meta.GetValueOrDefault("storageHealth")?.ToString() ?? ""));
        sb.AppendLine($"Free: {report.Meta.GetValueOrDefault("systemDriveFreeGb")} GB / {report.Meta.GetValueOrDefault("systemDriveTotalGb")} GB");
        sb.AppendLine("</pre>");

        sb.AppendLine("<h2>CHECKS</h2><table><tr><th>Status</th><th>Check</th><th>Summary</th><th>Severity</th></tr>");
        foreach (var c in report.Checks)
        {
            var cls = c.Passed ? "ok" : "bad";
            sb.AppendLine($"<tr><td class=\"{cls}\">{(c.Passed ? "OK" : "ISSUE")}</td><td>{Esc(c.Title)}</td><td>{Esc(c.Summary)}</td><td>{c.Severity}</td></tr>");
        }
        sb.AppendLine("</table>");

        sb.AppendLine("<h2>RECOMMENDATIONS</h2><ul>");
        if (report.Recommendations.Count == 0)
            sb.AppendLine("<li>None</li>");
        else
            foreach (var r in report.Recommendations)
                sb.AppendLine("<li>" + Esc(r) + "</li>");
        sb.AppendLine("</ul></body></html>");
        return sb.ToString();
    }

    static string Esc(string s) =>
        s.Replace("&", "&amp;", StringComparison.Ordinal)
         .Replace("<", "&lt;", StringComparison.Ordinal)
         .Replace(">", "&gt;", StringComparison.Ordinal);
}
