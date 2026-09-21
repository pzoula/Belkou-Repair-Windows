namespace Belkou.CLI;

internal sealed class ParsedArgs
{
    public string Command { get; init; } = "menu";
    public string? SubCommand { get; init; }
    public bool Json { get; init; }
    public bool Help { get; init; }
    public bool Version { get; init; }
}

internal static class ArgumentParser
{
    public static ParsedArgs Parse(string[] args)
    {
        if (args.Length == 0)
            return new ParsedArgs();

        var json = args.Any(a => a.Equals("--json", StringComparison.OrdinalIgnoreCase));
        var help = args.Any(a => a.Equals("--help", StringComparison.OrdinalIgnoreCase) || a.Equals("-h", StringComparison.OrdinalIgnoreCase));
        var version = args.Any(a => a.Equals("--version", StringComparison.OrdinalIgnoreCase) || a.Equals("-v", StringComparison.OrdinalIgnoreCase));

        var positional = args.Where(a => !a.StartsWith('-')).ToArray();
        if (help && positional.Length == 0)
            return new ParsedArgs { Help = true, Json = json };
        if (version && positional.Length == 0)
            return new ParsedArgs { Version = true };

        var command = positional.ElementAtOrDefault(0)?.ToLowerInvariant() ?? "menu";
        var sub = positional.ElementAtOrDefault(1)?.ToLowerInvariant();

        return new ParsedArgs
        {
            Command = command,
            SubCommand = sub,
            Json = json,
            Help = help,
            Version = version
        };
    }
}
