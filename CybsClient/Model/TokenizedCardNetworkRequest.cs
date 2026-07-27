using System.Text.Json.Serialization;

namespace CybsClient.Model;

public class TokenizedCardNetworkRequest
{
    [JsonPropertyName("source")]
    public string Source { get; set; } = "ONFILE";

    [JsonPropertyName("cardNumber")]
    public string? CardNumber { get; set; }

    [JsonPropertyName("expMonth")]
    public string? ExpMonth { get; set; }

    [JsonPropertyName("expYear")]
    public string? ExpYear { get; set; }

    [JsonPropertyName("securityCode")]
    public string? SecurityCode { get; set; }
}
