using System.Text.Json.Serialization;

namespace CybsClient.Model;

public class PaymentCardSampleDatumDto
{
    [JsonPropertyName("samplePaymentCardId")]
    public int SamplePaymentCardId { get; set; }

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

    [JsonPropertyName("ntScenario")]
    public string? NtScenario { get; set; }
}
