using System.Text.Json.Serialization;
using CybsClass.Cybersource.Models.BaseData;

namespace CybsClass.Cybersource.Models.OutboundTransObjects
{
    // Dedicated /pts/v2/payments request body for eCheck (ACH). Deliberately NOT AuthorizeData —
    // that class's PaymentInformation is a Dictionary<string, FullCard> and structurally cannot
    // carry a `bank` node. Apple Pay hit the identical wall and answered it with a dedicated
    // ApplePayAuthorizeData rather than reshaping the shared card path; this follows that.
    //
    // One shape serves all four documented eCheck flows — debit, recurring debit, create a TMS
    // token with the transaction, and submit a debit using a stored token. The difference between
    // them is entirely which nodes are populated:
    //
    //   debit          bank + paymentType
    //   recurring      bank + paymentType + commerceIndicator "recurring" + recurringOptions
    //   token create   bank + paymentType + actionList/actionTokenTypes
    //   token debit    customer + paymentType   (no bank node at all)
    public class ECheckAuthorizeData
    {
        [JsonPropertyName("clientReferenceInformation")]
        public ClientReferenceInformation? ClientReferenceInformation { get; set; }

        [JsonPropertyName("paymentInformation")]
        public EcheckPaymentInformation? PaymentInformation { get; set; }

        [JsonPropertyName("orderInformation")]
        public OrderInformation? OrderInformation { get; set; }

        [JsonPropertyName("processingInformation")]
        public ProcessingInformation? ProcessingInformation { get; set; }
    }
}
