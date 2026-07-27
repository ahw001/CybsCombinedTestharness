using System.Text.Json;
using System.Text.Json.Nodes;

namespace CybsClient.Services.Utilities;

// Shared indented JsonSerializerOptions for console logging and debug UI rendering only —
// never use this for actual outgoing/incoming wire payloads.
public static class JsonLogFormatter
{
    public static readonly JsonSerializerOptions Indented = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static string Pretty(JsonNode? node) => node?.ToJsonString(Indented) ?? string.Empty;

    // Best-effort pretty-print for a raw string: returns indented JSON if it parses,
    // otherwise the original string unchanged (it may be non-JSON debug text).
    public static string Pretty(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return raw ?? string.Empty;
        try
        {
            var node = JsonNode.Parse(raw);
            return node?.ToJsonString(Indented) ?? raw;
        }
        catch (JsonException)
        {
            return raw;
        }
    }
}
