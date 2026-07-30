namespace CybsClass.Cybersource.Transactions
{
    // One server->CyberSource HTTP exchange captured for ApiLogSidebar display.
    // RequestJson/ResponseJson always hold the human-readable form: for MLE/JOSE calls the
    // call site records the plaintext request (never the JWE) and overwrites ResponseJson
    // with the decrypted plaintext once available. Capture-context/Flex responses are JWTs
    // and are stored as-is.
    public class CybsExchange
    {
        // 256 KB per body — the store holds up to 200 requests' worth of exchanges and some
        // CyberSource payloads (capture-context JWTs, token BLOBs) run large.
        private const int MaxBodyLength = 256 * 1024;

        public string Url { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public string? RequestJson { get; set; }
        public string? ResponseJson { get; set; }
        public int? HttpStatusCode { get; set; }
        public bool IsError { get; set; }
        public string? FaultMessage { get; set; }

        public void Complete(int statusCode, string? responseBody)
        {
            HttpStatusCode = statusCode;
            ResponseJson = Truncate(responseBody);
            IsError = IsError || statusCode >= 400;
        }

        // Called from catch blocks when the send/read threw before a response body existed.
        public void MarkError(string message)
        {
            IsError = true;
            FaultMessage = message;
        }

        public static string? Truncate(string? body)
        {
            if (body is null || body.Length <= MaxBodyLength) return body;
            return body.Substring(0, MaxBodyLength) + $"\n... [truncated {body.Length - MaxBodyLength} chars]";
        }
    }

    // Per-request capture of the CyberSource exchange(s) actually performed, so the
    // Minimal API response can surface them (X-Cybs-Log-Id -> GET /api/cybslog/{id})
    // for display in the CybsClient ApiLogSidebar. AsyncLocal — not a plain static field
    // like CallCyberSource.LastRequestSent/LastRequestHeaders — because this must stay
    // correct under concurrent requests, not just single-developer manual testing.
    //
    // Reset() must create the list up front (never leave the AsyncLocal null) and
    // StartExchange must only mutate that same list in place, never rebind the AsyncLocal.
    // Response.OnStarting callbacks run against the ExecutionContext captured at the moment
    // OnStarting was called — a later AsyncLocal.Value reassignment (e.g. lazily creating the
    // list on first use) would be invisible to that frozen snapshot. Mutating a list the
    // snapshot already points to works because it's the same shared object, not a new binding.
    // The same rule covers Complete()/MarkError(): they mutate exchange objects the list
    // already references.
    public static class CybsCallContext
    {
        private static readonly AsyncLocal<List<CybsExchange>> _exchanges = new();

        // Adds a new in-flight exchange and returns it so the call site can Complete() /
        // MarkError() it. When no HTTP request context bound the AsyncLocal (harness code,
        // background work), the returned instance is simply detached — safe to mutate, never null.
        public static CybsExchange StartExchange(string httpMethod, string url, string? requestJson)
        {
            var exchange = new CybsExchange
            {
                HttpMethod = httpMethod,
                Url = url,
                RequestJson = CybsExchange.Truncate(requestJson)
            };
            _exchanges.Value?.Add(exchange);
            return exchange;
        }

        public static IReadOnlyList<CybsExchange> GetExchanges()
        {
            return (IReadOnlyList<CybsExchange>?)_exchanges.Value ?? Array.Empty<CybsExchange>();
        }

        // Kept for the X-Cybs-Target-Urls header (curl diagnostics + client fallback).
        public static IReadOnlyList<string> GetTargetUrls()
        {
            var exchanges = _exchanges.Value;
            if (exchanges is null || exchanges.Count == 0) return Array.Empty<string>();
            return exchanges.Select(e => e.Url).ToList();
        }

        public static void Reset()
        {
            _exchanges.Value = new List<CybsExchange>();
        }
    }
}
