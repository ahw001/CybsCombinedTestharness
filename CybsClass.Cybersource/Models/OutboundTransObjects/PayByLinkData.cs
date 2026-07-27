using CybsClass.Cybersource.Models.BaseData;
using System.Text.Json.Serialization;

namespace CybsClass.Cybersource.Models.OutboundTransObjects;

/// <summary>
/// CyberSource Pay by Link create-link request body (POST /ipl/v2/payment-links/).
/// </summary>
public class PayByLinkData
{
    [JsonPropertyName("processingInformation")]
    public PayByLinkProcessingInformation? ProcessingInformation { get; set; }

    [JsonPropertyName("purchaseInformation")]
    public PurchaseInformation? PurchaseInformation { get; set; }

    [JsonPropertyName("orderInformation")]
    public OrderInformation? OrderInformation { get; set; }
}

public class PayByLinkProcessingInformation
{
    [JsonPropertyName("linkType")]
    public string? LinkType { get; set; }
}

public class PurchaseInformation
{
    [JsonPropertyName("purchaseNumber")]
    public string? PurchaseNumber { get; set; }
}
