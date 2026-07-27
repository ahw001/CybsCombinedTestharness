using System.Text.Json.Serialization;
using CybsClass.Cybersource.Models.BaseData;

namespace CybsClass.Cybersource.Models.DTOs;

public class UnifiedCheckoutConfigurationDto
{
    public int UnifiedCheckoutConfigurationId { get; set; }

    public string? Name { get; set; }
    public string? Description { get; set; }

    public string? AllowedPaymentTypes { get; set; }
    public string? AllowedCardNetworks { get; set; }
    public string? Country { get; set; }
    public string? Locale { get; set; }
    public string? ButtonType { get; set; }
    public string? BillingType { get; set; }

    public bool RequestShipping { get; set; }
    public bool RequestEmail { get; set; }
    public bool RequestPhone { get; set; }
    public bool RequestSaveCredentials { get; set; }
    public bool ShowConfirmationStep { get; set; }
    public bool ShowAcceptedNetworkIcons { get; set; }

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
