using CybsClass.Cybersource.Models.BaseData;
using System.Text.Json.Serialization;

namespace CybsClass.Cybersource.Models.DTOs;

public class TokenizeTransactionDto
{
    [JsonPropertyName("tokenizeTransactionId")]
    public int TokenizeTransactionId { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("endpointMode")]
    public string? EndpointMode { get; set; }

    [JsonPropertyName("tokenId")]
    public string? TokenId { get; set; }

    [JsonPropertyName("tokenState")]
    public string? TokenState { get; set; }

    [JsonPropertyName("networkTokenNumber")]
    public string? NetworkTokenNumber { get; set; }

    [JsonPropertyName("cardType")]
    public string? CardType { get; set; }

    [JsonPropertyName("paymentAccountReference")]
    public string? PaymentAccountReference { get; set; }

    [JsonPropertyName("requestCardSuffix")]
    public string? RequestCardSuffix { get; set; }

    [JsonPropertyName("expMonth")]
    public string? ExpMonth { get; set; }

    [JsonPropertyName("expYear")]
    public string? ExpYear { get; set; }

    [JsonPropertyName("responseTransactionJson")]
    public string? ResponseTransactionJson { get; set; }

    [JsonPropertyName("error")]
    public ErrorObject? Error { get; set; }
}
