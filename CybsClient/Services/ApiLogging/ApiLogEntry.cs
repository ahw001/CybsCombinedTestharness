namespace CybsClient.Services.ApiLogging
{
    public enum ApiLogKind { Request, Error }

    public class ApiLogEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Method { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int? StatusCode { get; set; }
        public string? RequestJson { get; set; }
        public string? ResponseJson { get; set; }
        // Raw CyberSource response body, extracted from ResponseJson's "cybersourceJson"
        // field (top-level, or nested under "error"). Present whenever the server captured
        // it via CallCyberSource's centralized raw-body attach.
        public string? CybersourceJson { get; set; }
        // Actual CyberSource endpoint URL(s) called on the server for this request, from the
        // X-Cybs-Target-Urls response header. Null/empty when the endpoint made no CyberSource call.
        public IReadOnlyList<string>? CybsTargetUrls { get; set; }
        public long DurationMs { get; set; }
        public ApiLogKind Kind { get; set; } = ApiLogKind.Request;
        public bool IsError { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
