using System.Text.Json.Serialization;

namespace CybsClient.Model;

public class NetworkTokenTestCardDto
{
    [JsonPropertyName("networkTokenTestCardId")]
    public int NetworkTokenTestCardId { get; set; }

    [JsonPropertyName("cardBrand")]
    public string? CardBrand { get; set; }

    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; set; }

    [JsonPropertyName("expMonth")]
    public string? ExpMonth { get; set; }

    [JsonPropertyName("expYear")]
    public string? ExpYear { get; set; }

    [JsonPropertyName("cvv")]
    public string? Cvv { get; set; }

    [JsonPropertyName("testCategory")]
    public string? TestCategory { get; set; }

    [JsonPropertyName("scenarioOutcome")]
    public string? ScenarioOutcome { get; set; }

    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; set; }

    [JsonPropertyName("displayLabel")]
    public string? DisplayLabel { get; set; }
}
