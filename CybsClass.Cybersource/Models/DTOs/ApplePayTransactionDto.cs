using CybsClass.Cybersource.Models.BaseData;
using System.Text.Json.Serialization;

namespace CybsClass.Cybersource.Models.DTOs;

public class ApplePayTransactionDto
{
    [JsonPropertyName("applePayTransactionsId")]
    public int ApplePayTransactionsId { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("orderId")]
    public int OrderId { get; set; }

    [JsonPropertyName("clientReferenceCode")]
    public string? ClientReferenceCode { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("submitTimeUtc")]
    public DateTime? SubmitTimeUtc { get; set; }

    [JsonPropertyName("authorizedAmount")]
    public decimal? AuthorizedAmount { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("cardNetwork")]
    public string? CardNetwork { get; set; }

    [JsonPropertyName("maskedPan")]
    public string? MaskedPan { get; set; }

    [JsonPropertyName("paymentSolution")]
    public string? PaymentSolution { get; set; }

    [JsonPropertyName("transactionType")]
    public string? TransactionType { get; set; }

    [JsonPropertyName("decryptedTokenJson")]
    public string? DecryptedTokenJson { get; set; }

    [JsonPropertyName("requestTransactionJson")]
    public string? RequestTransactionJson { get; set; }

    [JsonPropertyName("responseTransactionJson")]
    public string? ResponseTransactionJson { get; set; }

    [JsonPropertyName("error")]
    public ErrorObject? Error { get; set; }
}
