using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CybsClass.Cybersource.Models.OutboundTransObjects
{
    // Request models for CyberSource's real Unified Checkout v1 API — POST /uc/v1/sessions
    // (the VAS.UnifiedCheckout() client). Confirmed against the vendored/public .NET SDK
    // (Api/UnifiedCheckoutV1CaptureContextApi.cs, Model/Ucv1sessions*.cs) — deliberately
    // separate from CaptureContext (v0, /up/v1/capture-contexts) which backs the legacy
    // /unifiedcheckout page. See UnifiedCheckoutPlanning.md Execution Goal 1.
    //
    // Key shape difference from v0: there is no top-level orderInformation — billing/shipping/
    // amount now live under data.orderInformation. This schema is STRICTLY validated
    // (additionalProperties: false) — live-verified 2026-07-21 that reusing v0's OrderInformation
    // class fails with "$.data.orderInformation.freight: ... schema does not allow additional
    // properties" (v0's OrderInformation defaults Freight to a non-null empty object). Dedicated,
    // minimal classes below intentionally expose ONLY fields confirmed present in the v1 schema.
    public class UnifiedCheckoutV1SessionRequest
    {
        [JsonPropertyName("targetOrigins")]
        public List<string>? TargetOrigins { get; set; }

        [JsonPropertyName("allowedCardNetworks")]
        public string[]? AllowedCardNetworks { get; set; }

        [JsonPropertyName("allowedPaymentTypes")]
        public string[]? AllowedPaymentTypes { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("locale")]
        public string? Locale { get; set; }

        [JsonPropertyName("buttonType")]
        public string? ButtonType { get; set; }

        [JsonPropertyName("captureMandate")]
        public UnifiedCheckoutV1CaptureMandate? CaptureMandate { get; set; }

        [JsonPropertyName("completeMandate")]
        public UnifiedCheckoutV1CompleteMandate? CompleteMandate { get; set; }

        [JsonPropertyName("data")]
        public UnifiedCheckoutV1Data? Data { get; set; }
    }

    // Explicit completeMandate for the v1 session request (SDK model: Ucv1sessionsCompleteMandate).
    // type: AUTH | CAPTURE | PREFER_AUTH. There is NO value that disables auto-processing —
    // whether the widget processes the payment itself vs. returns the transient token is
    // governed by the merchant's Business Center Unified Checkout configuration when this
    // object is absent. Live-verified 2026-07-28: sending {"type":"CAPTURE"} passes the strict
    // /uc/v1/sessions schema and the returned ctx carries it merged with the account defaults
    // (decisionManager, tms.tokenCreate).
    public class UnifiedCheckoutV1CompleteMandate
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("decisionManager")]
        public bool? DecisionManager { get; set; }

        [JsonPropertyName("consumerAuthentication")]
        public string? ConsumerAuthentication { get; set; }
    }

    public class UnifiedCheckoutV1CaptureMandate
    {
        [JsonPropertyName("billingType")]
        public string? BillingType { get; set; }

        [JsonPropertyName("requestEmail")]
        public bool RequestEmail { get; set; }

        [JsonPropertyName("requestPhone")]
        public bool RequestPhone { get; set; }

        [JsonPropertyName("requestShipping")]
        public bool RequestShipping { get; set; }

        [JsonPropertyName("shipToCountries")]
        public string[]? ShipToCountries { get; set; }

        [JsonPropertyName("showAcceptedNetworkIcons")]
        public bool? ShowAcceptedNetworkIcons { get; set; }

        [JsonPropertyName("showConfirmationStep")]
        public bool? ShowConfirmationStep { get; set; }

        [JsonPropertyName("requestSaveCredentials")]
        public bool? RequestSaveCredentials { get; set; }
    }

    public class UnifiedCheckoutV1Data
    {
        [JsonPropertyName("orderInformation")]
        public UnifiedCheckoutV1OrderInformation? OrderInformation { get; set; }
    }

    public class UnifiedCheckoutV1OrderInformation
    {
        [JsonPropertyName("amountDetails")]
        public UnifiedCheckoutV1AmountDetails? AmountDetails { get; set; }

        [JsonPropertyName("billTo")]
        public UnifiedCheckoutV1BillTo? BillTo { get; set; }

        [JsonPropertyName("shipTo")]
        public UnifiedCheckoutV1ShipTo? ShipTo { get; set; }
    }

    public class UnifiedCheckoutV1AmountDetails
    {
        [JsonPropertyName("totalAmount")]
        public string? TotalAmount { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }
    }

    public class UnifiedCheckoutV1BillTo
    {
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("middleName")]
        public string? MiddleName { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("address1")]
        public string? Address1 { get; set; }

        [JsonPropertyName("address2")]
        public string? Address2 { get; set; }

        [JsonPropertyName("address3")]
        public string? Address3 { get; set; }

        [JsonPropertyName("buildingNumber")]
        public string? BuildingNumber { get; set; }

        [JsonPropertyName("locality")]
        public string? Locality { get; set; }

        [JsonPropertyName("administrativeArea")]
        public string? AdministrativeArea { get; set; }

        [JsonPropertyName("postalCode")]
        public string? PostalCode { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("phoneType")]
        public string? PhoneType { get; set; }
    }

    public class UnifiedCheckoutV1ShipTo
    {
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("address1")]
        public string? Address1 { get; set; }

        [JsonPropertyName("address2")]
        public string? Address2 { get; set; }

        [JsonPropertyName("address3")]
        public string? Address3 { get; set; }

        [JsonPropertyName("buildingNumber")]
        public string? BuildingNumber { get; set; }

        [JsonPropertyName("locality")]
        public string? Locality { get; set; }

        [JsonPropertyName("administrativeArea")]
        public string? AdministrativeArea { get; set; }

        [JsonPropertyName("postalCode")]
        public string? PostalCode { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }
    }
}
