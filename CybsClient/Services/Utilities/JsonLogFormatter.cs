using System.Text.Json;
using System.Text.Json.Nodes;

namespace CybsClient.Services.Utilities;

// Shared indented JsonSerializerOptions for console logging and debug UI rendering only —
// never use this for actual outgoing/incoming wire payloads.
public static class JsonLogFormatter
{
    // UnsafeRelaxedJsonEscaping: display-only options — without it every quote inside an
    // embedded-JSON string renders as " and non-ASCII as \uXXXX, which the sidebar/log
    // convention forbids. Safe here precisely because this never feeds wire payloads.
    public static readonly JsonSerializerOptions Indented = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

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

    // Deep pretty-print: like Pretty, but string values that themselves contain JSON
    // (e.g. cybersourceJson, a serialized request stored inside a response) are parsed and
    // replaced with their nested object/array form, recursively, so the output is indented
    // at every object level with no escaped single-line JSON strings left. Non-JSON strings
    // (JWTs, JWE compact tokens, plain text) are left untouched; non-JSON input falls back
    // to the raw string.
    public static string DeepPretty(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return raw ?? string.Empty;
        try
        {
            var node = JsonNode.Parse(raw);
            if (node is null) return raw;
            var expanded = ExpandEmbeddedJson(node, depth: 0);
            return expanded?.ToJsonString(Indented) ?? raw;
        }
        catch (JsonException)
        {
            return raw;
        }
    }

    // Depth cap guards against pathological nesting (JSON-in-JSON-in-JSON…); 10 levels is
    // far beyond anything CyberSource returns.
    private const int MaxExpandDepth = 10;

    private static JsonNode? ExpandEmbeddedJson(JsonNode? node, int depth)
    {
        if (node is null || depth > MaxExpandDepth) return node;

        switch (node)
        {
            case JsonObject obj:
            {
                // Materialize the property list first — we mutate while iterating otherwise.
                var properties = obj.ToList();
                foreach (var kvp in properties)
                {
                    obj[kvp.Key] = ExpandEmbeddedJson(DetachedCopy(kvp.Value), depth + 1);
                }
                return obj;
            }
            case JsonArray arr:
            {
                for (int i = 0; i < arr.Count; i++)
                {
                    arr[i] = ExpandEmbeddedJson(DetachedCopy(arr[i]), depth + 1);
                }
                return arr;
            }
            case JsonValue value when value.TryGetValue<string>(out var s):
            {
                var trimmed = s.TrimStart();
                // Only strings that plausibly hold a JSON object/array — never numbers/dates.
                if (trimmed.StartsWith('{') || trimmed.StartsWith('['))
                {
                    try
                    {
                        var inner = JsonNode.Parse(s);
                        if (inner is JsonObject or JsonArray)
                        {
                            return ExpandEmbeddedJson(inner, depth + 1);
                        }
                    }
                    catch (JsonException) { /* looked like JSON but wasn't — keep the string */ }
                }
                return node;
            }
            default:
                return node;
        }
    }

    // Re-assigning a JsonNode that already has a parent throws; clone-by-value first.
    private static JsonNode? DetachedCopy(JsonNode? node) => node?.DeepClone();
}
