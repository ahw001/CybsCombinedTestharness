using System.Text.Json.Serialization;

namespace CybsClient.Services.DTOs;

public class PayByLinkRequestDto
{
    [JsonPropertyName("customerName")]
    public string? CustomerName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("deliveryMethod")]
    public string? DeliveryMethod { get; set; }

    [JsonPropertyName("totalAmount")]
    public decimal? TotalAmount { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; } = "USD";

    [JsonPropertyName("productName")]
    public string? ProductName { get; set; }
}
