using System.Text.Json.Serialization;
using CybsClass.Cybersource.Models.BaseData;

namespace CybsClass.Cybersource.Models.DTOs;

// One server->CyberSource exchange as returned by GET /api/cybslog/{id} for ApiLogSidebar
// display. Property names are deliberately absent from the client JsonErrorExtractor's
// known-error-name list (message, error, reason, action, description, detail, title) and
// none is "status", so a declined-transaction body carried inside requestJson/responseJson
// cannot be mistaken for an error on this DTO itself. The client keeps an attribute-for-
// attribute copy — casing must match exactly (System.Text.Json binds case-sensitively here).
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
