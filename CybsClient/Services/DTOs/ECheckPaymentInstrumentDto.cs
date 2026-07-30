using System;
using System.Text.Json.Serialization;

namespace CybsClient.Services.DTOs;

// Client-side copy of the server's ECheckPaymentInstrumentDto — a saved eCheck TMS token,
// offered by /echecktokencheckout's dropdown.
public partial class ECheckPaymentInstrumentDto
{
    [JsonPropertyName("eCheckPaymentInstrumentId")]
    public int ECheckPaymentInstrumentId { get; set; }

    [JsonPropertyName("b2cCustomerId")]
    public int B2cCustomerId { get; set; }

    // Sent back as paymentInformation.customer.id on a token debit.
    [JsonPropertyName("customerTokenId")]
    public string? CustomerTokenId { get; set; }

    [JsonPropertyName("paymentInstrumentId")]
    public string? PaymentInstrumentId { get; set; }

    [JsonPropertyName("instrumentIdentifierId")]
    public string? InstrumentIdentifierId { get; set; }

    [JsonPropertyName("instrumentIdentifierState")]
    public string? InstrumentIdentifierState { get; set; }

    [JsonPropertyName("routingNumber")]
    public string? RoutingNumber { get; set; }

    [JsonPropertyName("maskedAccountNumber")]
    public string? MaskedAccountNumber { get; set; }

    [JsonPropertyName("accountType")]
    public string? AccountType { get; set; }

    [JsonPropertyName("bankName")]
    public string? BankName { get; set; }

    [JsonPropertyName("displayLabel")]
    public string? DisplayLabel { get; set; }

    [JsonPropertyName("sourceTransactionId")]
    public string? SourceTransactionId { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}
