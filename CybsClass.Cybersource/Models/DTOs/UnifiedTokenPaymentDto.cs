using System.Text.Json.Serialization;
using CybsClass.Cybersource.Models.BaseData;

namespace CybsClass.Cybersource.Models.DTOs
{
    // Inbound DTO for POST /api/unified/v1tokenpayment — the manual Unified Checkout pattern:
    // the client obtained a transientTokenJwt from the widget (v0 show() or v1 mount() with
    // autoProcessing off) and asks the server to run the follow-on /pts/v2/payments call.
    // BillTo/ShipTo are optional by design: when null they are OMITTED from the wire request
    // entirely (never sent as empty objects) — the transient token's session data backs them.
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

        // true ⇒ processingInformation.actionList: ["TOKEN_CREATE"] goes out with the payment
        [JsonPropertyName("enableTokenCreate")]
        public bool EnableTokenCreate { get; set; }

        // e.g. ["customer","paymentInstrument","shippingAddress"] — sent only when
        // EnableTokenCreate is true and the array is non-empty; each entry is its own array
        // element (never a single comma-joined string).
        [JsonPropertyName("actionTokenTypes")]
        public string[]? ActionTokenTypes { get; set; }

        [JsonPropertyName("error")]
        public ErrorObject? Error { get; set; }
    }
}
