using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CybsClient.Model.Cybersource.Boarding;

// Product type discriminator mirror.
public static class BoardingProductTypes
{
    public const string DigitalPayments    = "digitalPayments";
    public const string CustomerInvoicing  = "customerInvoicing";
    public const string PayByLink          = "payByLink";
    public const string TokenManagement    = "tokenManagement";
    public const string UnifiedCheckout    = "unifiedCheckout";
    public const string ValueAddedServices = "valueAddedServices";
    public const string VirtualTerminal    = "virtualTerminal";
    public const string PayerAuthentication = "payerAuthentication";
}

public class BoardingTransactingMerchantProductSubscriptionDto
{
    public int BoardingTransactingMerchantProductSubscriptionId { get; set; }
    public int BoardingTransactingMerchantId { get; set; }
    public string? ProductType { get; set; }
    public int ProductSubscriptionId { get; set; }

    [JsonPropertyName("error")]
    public BoardingErrorDto? Error { get; set; }
}

public abstract class BoardingBasicSubscriptionDto
{
    public bool? Enabled { get; set; }
    public string? EnablementStatus { get; set; }
    public string? SelfServiceability { get; set; }
    public string? Distributability { get; set; }

    [JsonPropertyName("error")]
    public BoardingErrorDto? Error { get; set; }
}

public class BoardingDigitalPaymentsSubscriptionDto : BoardingBasicSubscriptionDto
{
    public int BoardingDigitalPaymentsSubscriptionId { get; set; }
    public bool? SamsungPayEnabled { get; set; }
    public bool? ApplePayEnabled { get; set; }
}

public class BoardingInvoicingSubscriptionDto : BoardingBasicSubscriptionDto
{
    public int BoardingInvoicingSubscriptionId { get; set; }
}

public class BoardingPayByLinkSubscriptionDto : BoardingBasicSubscriptionDto
{
    public int BoardingPayByLinkSubscriptionId { get; set; }
}

// Everything CyberSource's PECS NT-enablement JSON requires beyond these fields
// (vault settings, token formats, card masking, address, acquirerId, and all
// networkTokenServices enable flags) is a hardcoded constant on the server —
// not persisted or edited here.
public class BoardingTokenManagementSubscriptionDto
{
    public int BoardingTokenManagementSubscriptionId { get; set; }

    public bool? Enabled { get; set; }
    public string? OrganizationId { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? BusinessName { get; set; }
    public string? DoingBusinessAs { get; set; }
    public string? AcquirerMerchantId { get; set; }

    public string? CybersourceBoardingStatus { get; set; }
    public DateTime? SubmittedAt { get; set; }

    [JsonPropertyName("error")]
    public BoardingErrorDto? Error { get; set; }
}

public class BoardingUnifiedCheckoutSubscriptionDto : BoardingBasicSubscriptionDto
{
    public int BoardingUnifiedCheckoutSubscriptionId { get; set; }
    public string? ConfigurationStatus { get; set; }
    public string? ConfigurationMessage { get; set; }
    public bool? ApplePayEnabled { get; set; }
    public bool? ClickToPayEnabled { get; set; }
    public bool? ECheckEnabled { get; set; }
    public bool? GooglePayEnabled { get; set; }
    public bool? DecisionManagerEnabled { get; set; }
    public bool? PayerAuthenticationEnabled { get; set; }
    public bool? TokenManagementEnabled { get; set; }
    public List<string> AllowedCardNetworks { get; set; } = new();
}

public class BoardingValueAddedServicesSubscriptionDto
{
    public int BoardingValueAddedServicesSubscriptionId { get; set; }

    public bool? TransactionSearchEnabled { get; set; }
    public string? TransactionSearchEnablementStatus { get; set; }
    public string? TransactionSearchSelfServiceability { get; set; }
    public string? TransactionSearchDistributability { get; set; }

    public bool? ReportingEnabled { get; set; }
    public string? ReportingEnablementStatus { get; set; }
    public string? ReportingSelfServiceability { get; set; }
    public string? ReportingDistributability { get; set; }

    public bool? DisputeManagementEnabled { get; set; }

    [JsonPropertyName("error")]
    public BoardingErrorDto? Error { get; set; }
}

public class BoardingVirtualTerminalSubscriptionDto : BoardingBasicSubscriptionDto
{
    public int BoardingVirtualTerminalSubscriptionId { get; set; }
    public string? ConfigurationStatus { get; set; }

    public bool? AllowECheckFields { get; set; }
    public bool? AllowLevel3Fields { get; set; }
    public bool? AllowServiceFeeFields { get; set; }
    public bool? ProductProfileEnabled { get; set; }
    public string? MerchantCountry { get; set; }
    public bool? AccountLevelEnabled { get; set; }
    public string? TokenProvider { get; set; }
    public bool? SecureStorageEnabled { get; set; }
    public string? OtsTokenClass { get; set; }
    public string? OtsProfileId { get; set; }
    public string? CardProcessingType { get; set; }
    public string? DefaultTransactionMethod { get; set; }

    public string? GlobalPaymentInfoJson { get; set; }
    public string? ReceiptInfoJson { get; set; }
    public string? ReaderInfoJson { get; set; }
    public List<BoardingVirtualTerminalAcceptedCardTypeDto> AcceptedCardTypes { get; set; } = new();
    public List<BoardingVirtualTerminalMerchantDefinedFieldDto> MerchantDefinedFields { get; set; } = new();
}

public class BoardingVirtualTerminalAcceptedCardTypeDto
{
    public int BoardingVirtualTerminalAcceptedCardTypeId { get; set; }
    public string? ListType { get; set; }
    public string? CardType { get; set; }
}

public class BoardingVirtualTerminalMerchantDefinedFieldDto
{
    public int BoardingVirtualTerminalMerchantDefinedFieldId { get; set; }
    public byte FieldIndex { get; set; }
    public bool? DisplayField { get; set; }
    public bool? RequiredField { get; set; }
    public bool? ShowReceipt { get; set; }
    public bool? ReceiptDisplayEnabled { get; set; }
}

// ── Payer Authentication ────────────────────────────────────────────────────
public class BoardingPayerAuthenticationSubscriptionDto : BoardingBasicSubscriptionDto
{
    public int BoardingPayerAuthenticationSubscriptionId { get; set; }
    public string? TemplateId { get; set; }
    public List<BoardingPayerAuthenticationCardTypeDto> CardTypeConfigs { get; set; } = new();
}

public class BoardingPayerAuthenticationCardTypeDto
{
    public int BoardingPayerAuthenticationCardTypeConfigId { get; set; }
    public string? CardTypeName { get; set; }
    public List<BoardingPayerAuthenticationCurrencyDto> Currencies { get; set; } = new();
}

public class BoardingPayerAuthenticationCurrencyDto
{
    public int BoardingPayerAuthenticationCurrencyId { get; set; }
    public string? CurrencyCodes { get; set; }
    public string? AcquirerId { get; set; }
    public string? ProcessorMerchantId { get; set; }
}
