using System.Text.Json.Serialization;
using CybsClass.Cybersource.Models.BaseData;

#nullable enable

namespace CybsClass.Cybersource.Models.BaseData.Boarding;

// ── PECS request envelope ────────────────────────────────────────────────────

public sealed class NtPecsBoardingRequestDto
{
    [JsonPropertyName("organizationId")]
    public string? OrganizationId { get; set; }

    [JsonPropertyName("commerceSolutions")]
    public NtPecsCommerceSolutionsDto? CommerceSolutions { get; set; }
}

public sealed class NtPecsCommerceSolutionsDto
{
    [JsonPropertyName("tokenManagement")]
    public NtPecsProductDto? TokenManagement { get; set; }
}

public sealed class NtPecsProductDto
{
    [JsonPropertyName("subscriptionInformation")]
    public NtTmsSubscriptionInformationDto? SubscriptionInformation { get; set; }

    [JsonPropertyName("configurationInformation")]
    public NtPecsConfigurationInformationDto? ConfigurationInformation { get; set; }
}

public sealed class NtPecsConfigurationInformationDto
{
    [JsonPropertyName("configurations")]
    public NtTmsConfigurationsDto? Configurations { get; set; }
}

// ── PECS response envelope ───────────────────────────────────────────────────

public sealed class NtPecsBoardingResponseDto
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("submitTimeUtc")]
    public string? SubmitTimeUtc { get; set; }

    [JsonPropertyName("setups")]
    public NtPecsSetups? Setups { get; set; }

    [JsonPropertyName("error")]
    public ErrorObject? Error { get; set; }
}

public sealed class NtPecsSetups
{
    [JsonPropertyName("commerceSolutions")]
    public NtPecsSetupCommerceSolutions? CommerceSolutions { get; set; }
}

public sealed class NtPecsSetupCommerceSolutions
{
    [JsonPropertyName("tokenManagement")]
    public NtPecsSetupTokenManagement? TokenManagement { get; set; }
}

public sealed class NtPecsSetupTokenManagement
{
    [JsonPropertyName("configurationStatus")]
    public NtTmsStatusResult? ConfigurationStatus { get; set; }

    [JsonPropertyName("subscriptionStatus")]
    public NtTmsStatusResult? SubscriptionStatus { get; set; }
}

public sealed class NtTmsStatusResult
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

// ── Shared configurations block ───────────────────────────────────────────────
// Token Management is submitted only via the dedicated PECS endpoint
// (CallCybsNetworkTokenBoarding) — it is no longer part of the BRS envelope.

public sealed class NtTmsSubscriptionInformationDto
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
}

public sealed class NtTmsConfigurationsDto
{
    [JsonPropertyName("vault")]
    public NtTmsVaultDto? Vault { get; set; }

    [JsonPropertyName("networkTokenEnrollment")]
    public NtTmsNetworkTokenEnrollmentDto? NetworkTokenEnrollment { get; set; }

    [JsonPropertyName("networkTokenServices")]
    public NtTmsConfigurationsNetworkTokenServicesDto? NetworkTokenServices { get; set; }
}

public sealed class NtTmsVaultDto
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("defaultTokenType")]
    public string? DefaultTokenType { get; set; }

    [JsonPropertyName("tokenFormats")]
    public NtTmsTokenFormatsDto? TokenFormats { get; set; }

    [JsonPropertyName("sensitivePrivileges")]
    public NtTmsSensitivePrivilegesDto? SensitivePrivileges { get; set; }
}

public sealed class NtTmsTokenFormatsDto
{
    [JsonPropertyName("customer")]
    public string? Customer { get; set; }

    [JsonPropertyName("paymentInstrument")]
    public string? PaymentInstrument { get; set; }

    [JsonPropertyName("instrumentIdentifierCard")]
    public string? InstrumentIdentifierCard { get; set; }

    [JsonPropertyName("instrumentIdentifierBankAccount")]
    public string? InstrumentIdentifierBankAccount { get; set; }
}

public sealed class NtTmsSensitivePrivilegesDto
{
    [JsonPropertyName("cardNumberMaskingFormat")]
    public string? CardNumberMaskingFormat { get; set; }
}

public sealed class NtTmsNetworkTokenEnrollmentDto
{
    [JsonPropertyName("businessInformation")]
    public NtTmsEnrollmentBusinessInfoDto? BusinessInformation { get; set; }

    [JsonPropertyName("networkTokenServices")]
    public NtTmsEnrollmentNetworkTokenServicesDto? NetworkTokenServices { get; set; }
}

public sealed class NtTmsEnrollmentBusinessInfoDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("doingBusinessAs")]
    public string? DoingBusinessAs { get; set; }

    [JsonPropertyName("address")]
    public NtTmsEnrollmentAddressDto? Address { get; set; }

    [JsonPropertyName("websiteUrl")]
    public string? WebsiteUrl { get; set; }

    [JsonPropertyName("acquirer")]
    public NtTmsEnrollmentAcquirerDto? Acquirer { get; set; }
}

public sealed class NtTmsEnrollmentAddressDto
{
    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("locality")]
    public string? Locality { get; set; }
}

public sealed class NtTmsEnrollmentAcquirerDto
{
    [JsonPropertyName("acquirerId")]
    public string? AcquirerId { get; set; }

    [JsonPropertyName("acquirerMerchantId")]
    public string? AcquirerMerchantId { get; set; }
}

public sealed class NtTmsEnrollmentNetworkTokenServicesDto
{
    [JsonPropertyName("visaTokenService")]
    public NtTmsSchemeEnrollmentDto? VisaTokenService { get; set; }

    [JsonPropertyName("mastercardDigitalEnablementService")]
    public NtTmsSchemeEnrollmentDto? MastercardDigitalEnablementService { get; set; }
}

public sealed class NtTmsSchemeEnrollmentDto
{
    [JsonPropertyName("enrollment")]
    public bool Enrollment { get; set; }
}

public sealed class NtTmsConfigurationsNetworkTokenServicesDto
{
    [JsonPropertyName("notifications")]
    public NtTmsEnabledDto? Notifications { get; set; }

    [JsonPropertyName("paymentCredentials")]
    public NtTmsEnabledDto? PaymentCredentials { get; set; }

    [JsonPropertyName("visaTokenService")]
    public NtTmsSchemeServiceDto? VisaTokenService { get; set; }

    [JsonPropertyName("mastercardDigitalEnablementService")]
    public NtTmsSchemeServiceDto? MastercardDigitalEnablementService { get; set; }

    [JsonPropertyName("synchronousProvisioning")]
    public NtTmsEnabledDto? SynchronousProvisioning { get; set; }
}

public sealed class NtTmsSchemeServiceDto
{
    [JsonPropertyName("enableService")]
    public bool EnableService { get; set; }

    [JsonPropertyName("enableTransactionalTokens")]
    public bool EnableTransactionalTokens { get; set; }
}

public sealed class NtTmsEnabledDto
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
}
