using System.Text.Json.Serialization;
using CybsClient.Model.Cybersource.BaseData;

namespace CybsClient.Services.DTOs;

// Client mirror of CyberSourceServer's UnifiedCheckoutConfigurationDto — a named, saveable
// Unified Checkout capture-context configuration (Phase 0, UnifiedCheckoutPlanning.md).
public class UnifiedCheckoutConfigurationDto
{
    public int UnifiedCheckoutConfigurationId { get; set; }

    public string? Name { get; set; }
    public string? Description { get; set; }

    public string? AllowedPaymentTypes { get; set; }
    public string? AllowedCardNetworks { get; set; }
    public string? Country { get; set; } = "US";
    public string? Locale { get; set; } = "en_US";
    public string? ButtonType { get; set; }
    public string? BillingType { get; set; } = "FULL";

    public bool RequestShipping { get; set; } = true;
    public bool RequestEmail { get; set; } = true;
    public bool RequestPhone { get; set; } = true;
    public bool RequestSaveCredentials { get; set; }
    public bool ShowConfirmationStep { get; set; } = true;
    public bool ShowAcceptedNetworkIcons { get; set; } = true;

    public string? CompleteMandateType { get; set; }
    public bool EnableTms { get; set; }
    public string? TmsTokenTypes { get; set; }
    public bool Enable3ds { get; set; }
    public bool EnableDecisionManager { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("error")]
    public ErrorObject? Error { get; set; }
}
