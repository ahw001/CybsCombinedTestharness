using System.Text.Json.Serialization;
using CybsClass.Cybersource.Models.BaseData;

namespace CybsClass.Cybersource.Models.OutboundTransObjects
{
    // Dedicated /pts/v2/payments request body for Apple Pay (merchant decryption). Deliberately
    // NOT AuthorizeData — that class's PaymentInformation is a Dictionary<string, FullCard> and
    // cannot carry a tokenizedCard node. Keeping this separate means the existing AUTH/SALE
    // request builder (CallForCybsAuthTokenCreate) is never touched by Apple Pay work.
    public class ApplePayAuthorizeData
    {
        [JsonPropertyName("clientReferenceInformation")]
        public ClientReferenceInformation? ClientReferenceInformation { get; set; }

        [JsonPropertyName("paymentInformation")]
        public ApplePayPaymentInformation? PaymentInformation { get; set; }

        [JsonPropertyName("orderInformation")]
        public OrderInformation? OrderInformation { get; set; }

        [JsonPropertyName("processingInformation")]
        public ProcessingInformation? ProcessingInformation { get; set; }
    }
}
