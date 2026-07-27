using CybsClass.Cybersource.Models.BaseData;
using System.Text.Json.Serialization;

namespace CybsClass.Cybersource.Models.DTOs;

public class NetworkTokenProvisionResultDto
{
    // The tokenizedCard resource's own id, from POST /tms/v2/tokenize's "responses" array
    // (the entry where resource == "tokenizedCard"). This is also the id
    // GET /tms/v2/tokenized-cards/{id} is keyed by.
    [JsonPropertyName("tokenizedCardId")]
    public string? TokenizedCardId { get; set; }

    // The instrumentIdentifier resource's own id, from the same "responses" array
    // (the entry where resource == "instrumentIdentifier"). Returned alongside
    // tokenizedCardId as of the instrumentIdentifier/"enrollable card" tokenize shape.
    [JsonPropertyName("instrumentIdentifierId")]
    public string? InstrumentIdentifierId { get; set; }

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
