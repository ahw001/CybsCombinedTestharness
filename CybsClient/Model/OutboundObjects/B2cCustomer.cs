using CybsClient.Model.DBQueries;
using CybsClient.Model.Cybersource.BaseData;
using System.ComponentModel.DataAnnotations;


namespace CybsClient.Model.OutboundObjects;

public class B2cCustomer : IValidatableObject
{
    public int B2cCustomerId { get; set; }
    
    public string? PaymentCardId { get; set; }
    public string? OrderId { get; set; }

    [Required]
    public string FirstName { get; set; } = null!;
    [Required]
    public string LastName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? MiddleName  { get; set; }
    public string? Title { get; set; }
    public string? District { get; set; }

    [Required]
    public string? Email { get; set; }
    [Required]
    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? Address3 { get; set; }

    public string? BuildingNumber { get; set; }

    public string? PhoneType { get; set; }

    [Required]
    public string? City { get; set; }

    public string? AdministrativeArea { get; set; }

    public string? Region { get; set; }

    [Required]
    public string? PostalCode { get; set; }
    [Required]
    public string? Country { get; set; }
    [Required]
    public string? Phone { get; set; }

    public string? BearerToken { get; set; }

    public string? CloudPosType { get; set; }

    public string? MerchantCustomerID { get; set; } = string.Empty;

    public string? MerchantReferenceCode { get; set; }

    public string? CustomerInstrumentId { get; set; }

    public string? InstrumentIdentifier { get; set; }

    public string? PaymentMethod { get; set; } = "Credit/Debit";

    public string? PreAuthOrOnDeviceTip { get; set; } = "Standard";

    public string? InvoiceType { get; set; } = "Draft";

    public bool ShippingSameAsBilling { get; set; }

    public bool SaveFormData { get; set; }

    public bool PerformZeroAuth { get; set; }

    public string? AccountNumber { get; set; } = null!;

    public string? ExpMonth { get; set; }
    public string? ExpYear { get; set; }
    public string? Cvv { get; set; }
    public string? CardType { get; set; } = null!;

    // Set by CustomerMasterComponent.EnsureCustomerInitialized() based on whether the
    // hosting page's FormElements includes "ShowAccountDetails". Pages like FlexCheckout
    // never render AccountDetailsSection (card capture happens later via Flex Microform),
    // so FullName/AccountNumber/ExpMonth/ExpYear/Cvv must not be required there.
    public bool RequiresCardDetails { get; set; } = true;

    // Flags the Flex capture-context request (server: CallForFlexCaptureContext) to send a
    // narrower allowedCardNetworks list and omit allowedPaymentTypes entirely, matching a
    // known-working external application's capture context — set only by the Network Token
    // Test flow's minimal-customer builders (FlexCheckout.razor / FlexConsolidatedCheckout.razor).
    public bool IsNetworkTokenTest { get; set; }

    // Selects a saved UnifiedCheckoutConfiguration (Phase 0, UnifiedCheckoutPlanning.md) so the
    // server's CallForCaptureContext builds the capture context from it instead of hardcoded
    // defaults. Set by StoreCheckout.razor from IUnifiedCheckoutSettingsService; left null by
    // every other page (legacy /unifiedcheckout included), preserving existing behavior there.
    public int? UnifiedCheckoutConfigurationId { get; set; }

    public string? TransientToken { get; set; }

    public string? TransientTokenJwt { get; set; }

    // Raw JSON of Apple Pay's PKPaymentToken.paymentData (the "EC_v1" encrypted envelope) from
    // the Apple Pay JS payment sheet's onpaymentauthorized event. Decrypted server-side
    // (merchant decryption) — see ApplePayCheckout.razor / ApplePayDecryptor.
    public string? ApplePayPaymentData { get; set; }

    // event.payment.token.paymentMethod.network from Apple Pay JS (e.g. "visa", "masterCard").
    public string? ApplePayCardNetwork { get; set; }

    public bool MarkedForCapture { get; set; }

    public string[]? ActionTokenTypes { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? CompanyName { get; set; }
    public string? CompanyAddress1 { get; set; }
    public string? CompanyAdministrativeArea { get; set; }
    public string? CompanyBuildingNumber { get; set; }
    public string? CompanyCountry { get; set; }
    public string? CompanyDistrict { get; set; }
    public string? CompanyLocality { get; set; }
    public string? CompanyPostalCode { get; set; }

    public string ShippingFirstName { get; set; } = null!;

    public string ShippingLastName { get; set; } = null!;

    public string ShippingFullName { get; set; } = null!;

    public string? ShippingEmail { get; set; }

    public string? ShippingAddress1 { get; set; }

    public string? ShippingAddress2 { get; set; }

    public string? ShippingAddress3 { get; set; }

    public string? ShippingBuildingNumber { get; set; }

    public string? ShippingPhoneType { get; set; }

    public string? ShippingCity { get; set; }

    public string? ShippingAdministrativeArea { get; set; }

    public string? ShippingPostalCode { get; set; }

    public string? ShippingCountry { get; set; }

    public string? ShippingPhone { get; set; }

    public string? FreightAmount { get; set; }
    public string? TaxableFreightAmount { get; set; }

    public string? TaxDetailsType { get; set; }

    public string? TaxDetailsAmount { get; set; }
    public string? TaxDetailsRate { get; set; }

    public string? Currency { get; set; } = "USD";

    public string? PosTransId { get; set; }

    public string? IdType { get; set; }

    public string? CloudStatusType { get; set; }
    public string? Reason { get; set; }

    public bool OnDeviceTip { get; set; } = false;

    public bool PreAuthOnly { get; set; } = false;

    public bool PreAuthTip { get; set; }

    public bool IncrementalAuth { get; set; }

    public bool AllowPartialAuth { get; set; }

    public string? CloudPaymentMode { get; set; }

    public string? PosActivationCode { get; set; }

    public string? PosSetupCode { get; set; }

    public string? Error { get; set; }

    public Freight? Freight { get; set; } = new();

    public AmountDetails? AmountDetails { get; set; } = new();

    public LineItems? LineItem { get; set; } = new();

    public InvoiceInformation? InvoiceInformation { get; set; } = new();

    public AdditionalInformation? AdditionalInformation { get; set; } = new();

    public List<DBProduct>? Cart { get; set; } = new();

    public List<LineItems> LineItems { get; set; } = new();

    public ErrorObject? ErrorObject { get; set; } = new();

    public ErrorObject DbErrorObject { get; set; } = new();

        public string? TargetOrigin { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!RequiresCardDetails) yield break;

        if (string.IsNullOrWhiteSpace(FullName))
            yield return new ValidationResult("The FullName field is required.", new[] { nameof(FullName) });
        if (string.IsNullOrWhiteSpace(AccountNumber))
            yield return new ValidationResult("The AccountNumber field is required.", new[] { nameof(AccountNumber) });
        if (string.IsNullOrWhiteSpace(ExpMonth))
            yield return new ValidationResult("The ExpMonth field is required.", new[] { nameof(ExpMonth) });
        if (string.IsNullOrWhiteSpace(ExpYear))
            yield return new ValidationResult("The ExpYear field is required.", new[] { nameof(ExpYear) });
        if (string.IsNullOrWhiteSpace(Cvv))
            yield return new ValidationResult("The Cvv field is required.", new[] { nameof(Cvv) });
    }
}
