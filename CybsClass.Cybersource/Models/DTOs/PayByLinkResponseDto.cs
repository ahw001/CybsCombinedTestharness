using CybsClass.Cybersource.Models.BaseData;
using System;
using System.Text.Json.Serialization;

namespace CybsClass.Cybersource.Models.DTOs;

public class PayByLinkResponseDto
{
    [JsonPropertyName("payByLinkTransactionId")]
    public int PayByLinkTransactionId { get; set; }

    [JsonPropertyName("payByLinkUuid")]
    public Guid PayByLinkUuid { get; set; }

    [JsonPropertyName("cybersourceLinkId")]
    public string? CybersourceLinkId { get; set; }

    [JsonPropertyName("paymentLink")]
    public string? PaymentLink { get; set; }

    [JsonPropertyName("purchaseNumber")]
    public string? PurchaseNumber { get; set; }

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
    public string? Currency { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("transactionJson")]
    public string? TransactionJson { get; set; }

    [JsonPropertyName("error")]
    public ErrorObject? Error { get; set; }
}
