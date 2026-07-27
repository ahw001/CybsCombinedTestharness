using System.Text.Json.Serialization;

namespace CybsClient.Model.Cybersource.Boarding;

// ── Portfolio ────────────────────────────────────────────────────────────────

public class BoardingPortfolioDto
{
    public int BoardingPortfolioId { get; set; }
    public string? PortfolioName { get; set; }
    public string? Description { get; set; }
    public string? ExpectedSignatureMerchantId { get; set; }
    public string? BoardingPackageId { get; set; }
    public string? CardProcessingTemplateId { get; set; }
    public string? VirtualTerminalTemplateId { get; set; }
    public string? TokenManagementTemplateId { get; set; }
    public string? PayerAuthenticationTemplateId { get; set; }

    [JsonPropertyName("error")]
    public BoardingErrorDto? Error { get; set; }
}

// ── Organization ─────────────────────────────────────────────────────────────

public class BoardingOrganizationDto
{
    public int BoardingOrganizationId { get; set; }
    public string? OrganizationId { get; set; }
    public string? ParentOrganizationId { get; set; }
    public string? Status { get; set; }
    public string? Type { get; set; }
    public bool? Configurable { get; set; }
    public string? BoardingFlow { get; set; }
    public string? Mode { get; set; }
    public string? BoardingPackageId { get; set; }
    public int? BoardingPortfolioId { get; set; }
    public string? BusinessName { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? BusinessPhoneNumber { get; set; }
    public string? TimeZone { get; set; }
    public string? MerchantCategoryCode { get; set; }
    public string? AddressCountry { get; set; }
    public string? Address1 { get; set; }
    public string? PostalCode { get; set; }
    public string? AdministrativeArea { get; set; }
    public string? Locality { get; set; }
    public string? BusinessContactFirstName { get; set; }
    public string? BusinessContactLastName { get; set; }
    public string? BusinessContactPhone { get; set; }
    public string? BusinessContactEmail { get; set; }
    public string? TechnicalContactFirstName { get; set; }
    public string? TechnicalContactLastName { get; set; }
    public string? TechnicalContactPhone { get; set; }
    public string? TechnicalContactEmail { get; set; }
    public string? EmergencyContactFirstName { get; set; }
    public string? EmergencyContactLastName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactEmail { get; set; }

    [JsonPropertyName("error")]
    public BoardingErrorDto? Error { get; set; }
}

// ── Transacting Merchant ─────────────────────────────────────────────────────

public class BoardingTransactingMerchantDto
{
    public int BoardingTransactingMerchantId { get; set; }
    public int BoardingOrganizationId { get; set; }
    public string? TransactingOrganizationId { get; set; }
    public string? Status { get; set; }
    public string? Type { get; set; }
    public bool? Configurable { get; set; }
    public string? BusinessName { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? BusinessPhoneNumber { get; set; }
    public string? TimeZone { get; set; }
    public string? MerchantCategoryCode { get; set; }
    public string? AddressCountry { get; set; }
    public string? Address1 { get; set; }
    public string? PostalCode { get; set; }
    public string? AdministrativeArea { get; set; }
    public string? Locality { get; set; }
    public string? BoardingProcessor { get; set; }
    public string? BoardingPackageId { get; set; }
    public string? ParentOrganizationId { get; set; }
    public string? CybersourceBoardingStatus { get; set; }

    [JsonPropertyName("error")]
    public BoardingErrorDto? Error { get; set; }
}

// ── Card Product Subscription ─────────────────────────────────────────────────

public class BoardingCardProductSubscriptionDto
{
    public int BoardingProductSubscriptionId { get; set; }
    // Nullable — subscriptions are saved independently; linked to merchants via junction table
    public int? BoardingTransactingMerchantId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCategory { get; set; }
    public bool SubscriptionEnabled { get; set; }
    public string? EnablementStatus { get; set; }
    public string? SelfServiceability { get; set; }
    public string? Distributability { get; set; }
    public bool? CardPresentEnabled { get; set; }
    public bool? CardNotPresentEnabled { get; set; }
    public string? TemplateId { get; set; }
    public BoardingCardProcessingConfigDto? CardProcessingConfig { get; set; }

    [JsonPropertyName("error")]
    public BoardingErrorDto? Error { get; set; }
}

// ── Transacting Merchant ↔ Subscription Junction ──────────────────────────────

public class BoardingTransactingMerchantSubscriptionDto
{
    public int BoardingTransactingMerchantSubscriptionId { get; set; }
    public int BoardingTransactingMerchantId { get; set; }
    public int BoardingProductSubscriptionId { get; set; }

    [JsonPropertyName("error")]
    public BoardingErrorDto? Error { get; set; }
}

public class BoardingCardProcessingConfigDto
{
    public int BoardingCardProcessingConfigId { get; set; }
    public string? DefaultAuthTypeCode { get; set; }
    public bool? EnablePartialAuth { get; set; }
    public string? MerchantCategoryCode { get; set; }
    public bool? EnableDuplicateRefNumBlocking { get; set; }
    public bool? AuthMerchantRetryDisabled { get; set; }
    public bool? IgnoreAddressVerificationSystem { get; set; }
    public bool? VisaStraightThroughProcessingOnly { get; set; }
    public string? CardPresentSolutionType { get; set; }
    public string? CardPresentProductSelected { get; set; }
    public bool? CpRelaxAddressVerificationSystem { get; set; }
    public bool? CpRelaxAvsAllowZipWithoutCountry { get; set; }
    public bool? CpRelaxAvsAllowExpiredCard { get; set; }
    public List<BoardingProcessorConfigDto> ProcessorConfigs { get; set; } = new();
}

// ── Processor Config ─────────────────────────────────────────────────────────

public class BoardingProcessorConfigDto
{
    public int BoardingProcessorConfigId { get; set; }
    public int BoardingCardProcessingConfigId { get; set; }
    public string? ProcessorName { get; set; }
    public string? BatchGroup { get; set; }
    public string? AcquirerCountryCode { get; set; }
    public string? AcquirerFileDestinationBin { get; set; }
    public string? AcquirerInterbankCardAssociationId { get; set; }
    public string? AcquirerInstitutionId { get; set; }
    public string? AcquirerDiscoverInstitutionId { get; set; }
    public string? AcquirerMerchantId { get; set; }
    public bool? EnableTransactionReferenceNumber { get; set; }
    public bool? EnablePosNetworkSwitching { get; set; }
    public string? MerchantTaxId { get; set; }
    public bool? AllowMultipleBills { get; set; }
    public string? BusinessApplicationId { get; set; }
    public bool? EnableAutoAuthReversalAfterVoid { get; set; }
    public bool? EnableExpresspayPanTranslation { get; set; }
    public bool? QuasiCash { get; set; }
    public bool? DisablePointOfSaleTerminalIdValidation { get; set; }
    public bool? EnablePinTranslation { get; set; }
    public string? DefaultPointOfSaleTerminalId { get; set; }
    public string? PointOfSaleTerminalIds { get; set; }
    public bool? EnableMultipleTerminalIDs { get; set; }
    public bool? PinDebitEnablePartialAuth { get; set; }
    public bool? RelaxAddressVerificationSystem { get; set; }
    public bool? RelaxAvsAllowZipWithoutCountry { get; set; }
    public bool? RelaxAvsAllowExpiredCard { get; set; }
    public bool? EnableEmsTransactionRiskScore { get; set; }
    public bool? CurrencyUsdEnabled { get; set; }
    public bool? CurrencyUsdEnabledCardPresent { get; set; }
    public bool? CurrencyUsdEnabledCardNotPresent { get; set; }
    public string? CurrencyUsdMerchantId { get; set; }
    public string? CurrencyUsdTerminalId { get; set; }
    public string? CurrencyUsdTerminalIds { get; set; }
    public List<BoardingPaymentTypeDto> PaymentTypes { get; set; } = new();

    [JsonPropertyName("error")]
    public BoardingErrorDto? Error { get; set; }
}

public class BoardingPaymentTypeDto
{
    public int BoardingProcessorPaymentTypeId { get; set; }
    public string? PaymentType { get; set; }
    public bool Enabled { get; set; }
    public string? MerchantId { get; set; }
    public string? TerminalId { get; set; }
    public bool? EnabledCardPresent { get; set; }
    public bool? EnabledCardNotPresent { get; set; }
}

// ── Error ────────────────────────────────────────────────────────────────────

public class BoardingErrorDto
{
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("action")]
    public string? Action { get; set; }
}
