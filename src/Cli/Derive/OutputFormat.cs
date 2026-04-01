namespace KeyUtils.Cli.Derive;

public enum OutputFormat
{
    Hex,
    Base58,
    Json
}

internal static class OutputFormatExtensions
{
    extension(OutputFormat format)
    {
        public string ToCliValue()
            => format switch
            {
                OutputFormat.Hex => "hex",
                OutputFormat.Base58 => "base58",
                OutputFormat.Json => "json",
                _ => throw new NotImplementedException()
            };
    }

    public static bool TryParse(string? value, out OutputFormat format)
    {
        switch(value?.Trim().ToLowerInvariant())
        {
            case "hex":
                format = OutputFormat.Hex;
                return true;
            case "base58":
                format = OutputFormat.Base58;
                return true;
            case "json":
                format = OutputFormat.Json;
                return true;
            default:
                format = default;
                return false;
        }
    }

    public static string Describe(IEnumerable<OutputFormat> formats)
        => String.Join(", ", formats.Select(format => format.ToCliValue()));
}
