namespace CybsClass.Cybersource.Transactions
{
    // Per-request capture of the CyberSource endpoint URL(s) actually called, so the
    // Minimal API response can surface them (via X-Cybs-Target-Urls) for display in the
    // CybsClient ApiLogSidebar. AsyncLocal — not a plain static field like
    // CallCyberSource.LastRequestSent/LastRequestHeaders — because this must stay correct
    // under concurrent requests, not just single-developer manual testing.
    //
    // Reset() must create the list up front (never leave the AsyncLocal null) and
    // RecordTargetUrl must only mutate that same list in place, never rebind the AsyncLocal.
    // Response.OnStarting callbacks run against the ExecutionContext captured at the moment
    // OnStarting was called — a later AsyncLocal.Value reassignment (e.g. lazily creating the
    // list on first use) would be invisible to that frozen snapshot. Mutating a list the
    // snapshot already points to works because it's the same shared object, not a new binding.
    public static class CybsCallContext
    {
        private static readonly AsyncLocal<List<string>> _targetUrls = new();

        public static void RecordTargetUrl(string url)
        {
            _targetUrls.Value?.Add(url);
        }

        public static IReadOnlyList<string> GetTargetUrls()
        {
            return (IReadOnlyList<string>?)_targetUrls.Value ?? Array.Empty<string>();
        }

        public static void Reset()
        {
            _targetUrls.Value = new List<string>();
        }
    }
}
