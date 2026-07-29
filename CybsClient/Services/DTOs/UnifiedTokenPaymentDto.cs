using System.Text.Json.Serialization;
using CybsClient.Model.Cybersource.BaseData;

namespace CybsClient.Services.DTOs;

// Client mirror of the server's UnifiedTokenPaymentDto — POST /api/unified/v1tokenpayment,
// the manual Unified Checkout follow-on (/pts/v2/payments with tokenInformation.transientTokenJwt).
// BillTo/ShipTo stay null to omit them from the wire request entirely (the transient token's
// session data backs them) — never send empty objects.
public class UnifiedTokenPaymentDto
{
    [JsonPropertyName("b2cCustomerId")]
    public int B2cCustomerId { get; set; }

    [JsonPropertyName("orderId")]
    public int OrderId { get; set; }

    [JsonPropertyName("clientReferenceCode")]
    public string? ClientReferenceCode { get; set; }

    [JsonPropertyName("transientTokenJwt")]
    public string? TransientTokenJwt { get; set; }

    [JsonPropertyName("totalAmount")]
    public string? TotalAmount { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("billTo")]
    public BillTo? BillTo { get; set; }

    [JsonPropertyName("shipTo")]
    public BillTo? ShipTo { get; set; }

    // true ⇒ the server adds processingInformation.actionList: ["TOKEN_CREATE"]
    [JsonPropertyName("enableTokenCreate")]
    public bool EnableTokenCreate { get; set; }

    // e.g. ["customer","paymentInstrument","shippingAddress"] — individual array entries,
    // never a single comma-joined string.
    [JsonPropertyName("actionTokenTypes")]
    public string[]? ActionTokenTypes { get; set; }

    [JsonPropertyName("error")]
    public ErrorObject? Error { get; set; }
}
