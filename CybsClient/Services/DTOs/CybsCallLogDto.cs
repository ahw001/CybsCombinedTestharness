using CybsClient.Model.Cybersource.BaseData;
using System.Text.Json.Serialization;

namespace CybsClient.Services.DTOs
{
    // Client copy of the server's CybsCallLogDto/CybsExchangeDto
    // (CyberSourceServer\CybsClass.Cybersource\Models\DTOs\CybsCallLogDto.cs) — attribute-for-
    // attribute identical; a [JsonPropertyName] casing mismatch here fails silently (property
    // stays null). One server->CyberSource HTTP exchange, fetched from GET /api/cybslog/{id}
    // by ApiLogDelegatingHandler for ApiLogSidebar display.
    public class CybsExchangeDto
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("httpMethod")]
        public string HttpMethod { get; set; } = string.Empty;

        [JsonPropertyName("requestJson")]
        public string? RequestJson { get; set; }

        [JsonPropertyName("responseJson")]
        public string? ResponseJson { get; set; }

        [JsonPropertyName("httpStatusCode")]
        public int? HttpStatusCode { get; set; }

        [JsonPropertyName("isError")]
        public bool IsError { get; set; }

        [JsonPropertyName("faultMessage")]
        public string? FaultMessage { get; set; }
    }

    public class CybsCallLogDto
    {
        [JsonPropertyName("exchanges")]
        public List<CybsExchangeDto>? Exchanges { get; set; }

        [JsonPropertyName("error")]
        public ErrorObject? Error { get; set; }
    }
}
