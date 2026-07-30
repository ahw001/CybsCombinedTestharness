using CybsClient.Services.DTOs;

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
        // The server->CyberSource exchange(s) behind this request, fetched from
        // GET /api/cybslog/{id} when the response carried X-Cybs-Log-Id. When present and no
        // error occurred, the sidebar shows these INSTEAD of the client<->server bodies.
        public IReadOnlyList<CybsExchangeDto>? CybsExchanges { get; set; }
        // True when the 2XX response body carried a non-null root-level "error" property —
        // the server's ErrorObject convention for application-level failures.
        public bool HasEmbeddedError { get; set; }

        // One error test for the whole entry: HTTP/transport error, embedded 2XX ErrorObject,
        // or any CyberSource exchange fault / non-2xx CyberSource status.
        public bool HasAnyError =>
            IsError
            || HasEmbeddedError
            || (CybsExchanges?.Any(x => x.IsError || x.FaultMessage is not null) ?? false);
    }
}
