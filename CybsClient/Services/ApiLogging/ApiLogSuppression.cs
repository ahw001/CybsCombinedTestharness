using System.Text.Json;
using System.Text.Json.Serialization;

namespace CybsClient.Services.ApiLogging
{
    /// <summary>
    /// Which payload(s) an endpoint's log suppression covers.
    /// </summary>
    public enum SuppressScope { Both, Request, Response }

    /// <summary>
    /// One rule from apiLogSuppression.json.
    /// </summary>
    public class ApiLogSuppressionRule
    {
        [JsonPropertyName("path")] public string Path { get; set; } = string.Empty;
        [JsonPropertyName("method")] public string? Method { get; set; }
        [JsonPropertyName("match")] public string? Match { get; set; }
        [JsonPropertyName("suppress")] public string? Suppress { get; set; }
        [JsonPropertyName("note")] public string? Note { get; set; }

        public bool ExactMatch => string.Equals(Match, "exact", StringComparison.OrdinalIgnoreCase);

        public SuppressScope Scope => Suppress?.ToLowerInvariant() switch
        {
            "request" => SuppressScope.Request,
            "response" => SuppressScope.Response,
            _ => SuppressScope.Both
        };

        public string NoteText => string.IsNullOrWhiteSpace(Note) ? "success" : Note!;

        // Wire value for the X-Suppress-Payload-Log header the client sends to the server.
        public string HeaderValue => Scope switch
        {
            SuppressScope.Request => "request",
            SuppressScope.Response => "response",
            _ => "both"
        };
    }

    internal class ApiLogSuppressionFile
    {
        [JsonPropertyName("endpoints")]
        public List<ApiLogSuppressionRule> Endpoints { get; set; } = new();
    }

    /// <summary>
    /// Payload-logging suppression, driven by apiLogSuppression.json in the client application
    /// root. The call is always logged (method, URL, status, duration) — only the request and/or
    /// response BODY is replaced with a short note, for endpoints whose payloads are noise.
    ///
    /// This is the single source of truth for BOTH sides. The client applies it to the
    /// ApiLogSidebar and its own console logging; ApiLogDelegatingHandler also sends
    /// <see cref="HeaderName"/> on the outgoing request so CyberSourceServer suppresses its
    /// console payload logging for the same call. The server deliberately has no copy of this
    /// file — one file, one place to edit, and it works identically whether the two projects run
    /// as separate processes or merged into the single combined host.
    ///
    /// Loaded once at startup (see Program.cs). A missing or malformed file is not fatal: it
    /// simply means nothing is suppressed.
    /// </summary>
    public static class ApiLogSuppression
    {
        public const string HeaderName = "X-Suppress-Payload-Log";
        public const string FileName = "apiLogSuppression.json";

        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        private static List<ApiLogSuppressionRule> _rules = new();

        public static IReadOnlyList<ApiLogSuppressionRule> Rules => _rules;

        public static void Load(string contentRootPath)
        {
            var path = System.IO.Path.Combine(contentRootPath, FileName);
            try
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine($"[ApiLogSuppression] {FileName} not found at {path} — no payload logging is suppressed.");
                    _rules = new();
                    return;
                }

                var json = File.ReadAllText(path);
                var parsed = JsonSerializer.Deserialize<ApiLogSuppressionFile>(json, _options);
                _rules = parsed?.Endpoints?.Where(r => !string.IsNullOrWhiteSpace(r.Path)).ToList() ?? new();

                Console.WriteLine($"[ApiLogSuppression] Loaded {_rules.Count} payload-suppression rule(s) from {FileName}:");
                foreach (var r in _rules)
                {
                    Console.WriteLine($"[ApiLogSuppression]   {r.Method ?? "*"} {r.Path} ({(r.ExactMatch ? "exact" : "prefix")}, suppress={r.HeaderValue})");
                }
            }
            catch (Exception ex)
            {
                // Never fatal — a bad config file must not take the app down or silently change
                // application behavior. It only ever affects how much gets logged.
                Console.Error.WriteLine($"[ApiLogSuppression] Failed to read {FileName}: {ex.Message}. No payload logging is suppressed.");
                _rules = new();
            }
        }

        /// <summary>
        /// Returns the matching rule for this call, or null when its payloads should be logged
        /// in full. <paramref name="url"/> may be absolute or relative, with or without a query.
        /// </summary>
        public static ApiLogSuppressionRule? Match(string? method, string? url)
        {
            if (_rules.Count == 0 || string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            var candidate = NormalizePath(url);
            if (candidate.Length == 0)
            {
                return null;
            }

            foreach (var rule in _rules)
            {
                if (!MethodMatches(rule.Method, method))
                {
                    continue;
                }

                var rulePath = NormalizePath(rule.Path);
                if (rulePath.Length == 0)
                {
                    continue;
                }

                // Prefix matching is segment-aware: "/api/samplecards" covers itself and
                // "/api/samplecards/42", but NOT an unrelated "/api/samplecardsarchive".
                var matched = candidate.Equals(rulePath, StringComparison.Ordinal)
                              || (!rule.ExactMatch && candidate.StartsWith(rulePath + "/", StringComparison.Ordinal));

                if (matched)
                {
                    return rule;
                }
            }

            return null;
        }

        private static bool MethodMatches(string? ruleMethod, string? actualMethod)
        {
            if (string.IsNullOrWhiteSpace(ruleMethod) || ruleMethod == "*")
            {
                return true;
            }
            return string.Equals(ruleMethod, actualMethod, StringComparison.OrdinalIgnoreCase);
        }

        // Call sites disagree about leading slashes ("api/samplecards/" vs "/api/tokens/...") and
        // some carry a full absolute URI or a query string. Normalize all of that away so the
        // config can be written the obvious way.
        private static string NormalizePath(string url)
        {
            var path = url;

            if (Uri.TryCreate(path, UriKind.Absolute, out var absolute))
            {
                path = absolute.AbsolutePath;
            }
            else
            {
                var queryIndex = path.IndexOf('?');
                if (queryIndex >= 0)
                {
                    path = path.Substring(0, queryIndex);
                }
            }

            path = path.Trim();
            if (path.Length == 0)
            {
                return string.Empty;
            }

            if (!path.StartsWith('/'))
            {
                path = "/" + path;
            }

            path = path.TrimEnd('/');
            return path.ToLowerInvariant();
        }
    }
}
