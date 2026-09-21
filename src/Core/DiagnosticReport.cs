namespace Belkou.Core;

internal sealed class DiagnosticReport
{
    public DateTimeOffset GeneratedAt { get; init; } = DateTimeOffset.Now;
    public string Version { get; init; } = AppInfo.Version;
    public List<CheckResult> Checks { get; init; } = [];
    public List<string> Recommendations { get; init; } = [];
    public Dictionary<string, object?> Meta { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public Severity OverallRisk
    {
        get
        {
            if (Checks.Count == 0) return Severity.Inconclusive;
            return Checks
                .Where(c => c.Severity is not Severity.Inconclusive and not Severity.Info)
                .Select(c => c.Severity)
                .DefaultIfEmpty(Severity.Ok)
                .Max();
        }
    }

    public IEnumerable<CheckResult> Issues =>
        Checks.Where(c => !c.Passed && c.Severity is Severity.Low or Severity.Medium or Severity.High or Severity.Critical);
}
