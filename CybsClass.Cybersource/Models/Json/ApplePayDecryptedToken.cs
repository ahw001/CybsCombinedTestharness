using System.Text.Json.Serialization;

namespace CybsClass.Cybersource.Models.Json
{
    // Shape of Apple's decrypted EC_v1 payment data dictionary — see Apple's
    // "Payment Token Format Reference". Produced by ApplePayDecryptor.Decrypt.
    public class ApplePayDecryptedToken
    {
        [JsonPropertyName("applicationPrimaryAccountNumber")]
        public string? ApplicationPrimaryAccountNumber { get; set; }

        // Format YYMMDD.
        [JsonPropertyName("applicationExpirationDate")]
        public string? ApplicationExpirationDate { get; set; }

        [JsonPropertyName("currencyCode")]
        public string? CurrencyCode { get; set; }

        [JsonPropertyName("transactionAmount")]
        public long? TransactionAmount { get; set; }

        [JsonPropertyName("cardholderName")]
        public string? CardholderName { get; set; }

        [JsonPropertyName("deviceManufacturerIdentifier")]
        public string? DeviceManufacturerIdentifier { get; set; }

        // "3DSecure" for card-network tokens (the standard web/in-app case).
        [JsonPropertyName("paymentDataType")]
        public string? PaymentDataType { get; set; }

        [JsonPropertyName("paymentData")]
        public ApplePayCryptogramData? PaymentData { get; set; }
    }

    public class ApplePayCryptogramData
    {
        [JsonPropertyName("onlinePaymentCryptogram")]
        public string? OnlinePaymentCryptogram { get; set; }

        [JsonPropertyName("eciIndicator")]
        public string? EciIndicator { get; set; }
    }
}
