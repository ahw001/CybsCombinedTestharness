using System.Text.Json.Serialization;

namespace CybsClient.Services.DTOs;

/// <summary>
/// A single CyberSource 3-D Secure 2.x sandbox test case + card-brand + spec-version combination,
/// sourced from the PayerAuthCardSampleData table (full outcome matrix, distinct from the
/// NT/T4T-focused PaymentCardSampleData table served by <see cref="PayerAuthCardSampleDto"/>/ICardService).
/// </summary>
public partial class PayerAuthTestCardDto
{
    [JsonPropertyName("samplePayAuthPaymentCardId")]
    public int SamplePayAuthPaymentCardId { get; set; }

    [JsonPropertyName("cardBrand")]
    public string CardBrand { get; set; } = null!;

    [JsonPropertyName("accountNumber")]
    public string AccountNumber { get; set; } = null!;

    [JsonPropertyName("expMonth")]
    public string? ExpMonth { get; set; }

    [JsonPropertyName("expYear")]
    public string? ExpYear { get; set; }

    [JsonPropertyName("cvv")]
    public string? Cvv { get; set; }

    [JsonPropertyName("testCaseId")]
    public string? TestCaseId { get; set; }

    [JsonPropertyName("testCaseName")]
    public string? TestCaseName { get; set; }

    [JsonPropertyName("specVersion")]
    public string? SpecVersion { get; set; }

    [JsonPropertyName("cardTypeCode")]
    public string? CardTypeCode { get; set; }

    [JsonPropertyName("veresEnrolled")]
    public string? VeresEnrolled { get; set; }

    [JsonPropertyName("stepUpRequired")]
    public bool StepUpRequired { get; set; }

    [JsonPropertyName("paresStatus")]
    public string? ParesStatus { get; set; }

    [JsonPropertyName("eciRawValue")]
    public string? EciRawValue { get; set; }

    [JsonPropertyName("eciStringValue")]
    public string? EciStringValue { get; set; }

    [JsonPropertyName("requiresCountryOverride")]
    public string? RequiresCountryOverride { get; set; }

    [JsonPropertyName("otherInputNotes")]
    public string? OtherInputNotes { get; set; }

    [JsonPropertyName("expectedOutcomeNotes")]
    public string? ExpectedOutcomeNotes { get; set; }

    [JsonPropertyName("sourceUrl")]
    public string? SourceUrl { get; set; }

    /// <summary>Display label for a grouped &lt;optgroup&gt; header, e.g. "2.9: Step-Up Authentication Is Successful".</summary>
    public string TestCaseGroupLabel => string.IsNullOrWhiteSpace(TestCaseId) ? (TestCaseName ?? "")
        : $"{TestCaseId}: {TestCaseName}";

    /// <summary>Display label for an individual &lt;option&gt;, e.g. "Visa (2.2.0)".</summary>
    public string OptionLabel => string.IsNullOrWhiteSpace(SpecVersion) ? CardBrand : $"{CardBrand} ({SpecVersion})";
}
