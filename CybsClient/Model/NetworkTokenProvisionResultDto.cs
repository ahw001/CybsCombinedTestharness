using System.Text.Json.Serialization;
using CybsClient.Model.Cybersource.BaseData;

namespace CybsClient.Model;

public class NetworkTokenProvisionResultDto
{
    [JsonPropertyName("tokenizedCardId")]
    public string? TokenizedCardId { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("enrollmentId")]
    public string? EnrollmentId { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("tokenReferenceId")]
    public string? TokenReferenceId { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("number")]
    public string? Number { get; set; }

    [JsonPropertyName("expirationMonth")]
    public string? ExpirationMonth { get; set; }

    [JsonPropertyName("expirationYear")]
    public string? ExpirationYear { get; set; }

    [JsonPropertyName("requestorId")]
    public string? RequestorId { get; set; }

    [JsonPropertyName("error")]
    public ErrorObject? Error { get; set; }
}
