using CybsClient.Model.Cybersource.BaseData;
using System;
using System.Text.Json.Serialization;

namespace CybsClient.Services.DTOs;

public class PayByLinkTransactionDto
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
