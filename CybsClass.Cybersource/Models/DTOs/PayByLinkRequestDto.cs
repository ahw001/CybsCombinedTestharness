using CybsClass.Cybersource.Models.BaseData;
using System.Text.Json.Serialization;

namespace CybsClass.Cybersource.Models.DTOs;

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

    [JsonPropertyName("error")]
    public ErrorObject? Error { get; set; }
}
