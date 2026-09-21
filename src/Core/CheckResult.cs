namespace Belkou.Core;

internal sealed class CheckResult
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public Severity Severity { get; init; }
    public bool Passed { get; init; }
    public string Summary { get; init; } = "";
    public string Details { get; init; } = "";
    public string? Recommendation { get; init; }
    public string? RepairAction { get; init; }
}
