namespace CybsClient.Services.Utilities
{
    public static class AppErrorConfig
    {
        public static string[] ErrorKeywords { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Statuses that should never be treated as errors even if they contain error keyword substrings.
        /// PARTIAL_AUTHORIZED is a valid CyberSource response status for partial authorizations.
        /// </summary>
        public static string[] ExcludedStatuses { get; set; } = new[] { "PARTIAL_AUTHORIZED" };

        public static bool IsExcludedStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return false;
            return ExcludedStatuses.Any(s => string.Equals(s, status, StringComparison.OrdinalIgnoreCase));
        }
    }
}
