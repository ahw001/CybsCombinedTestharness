using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using CybsClient.Model.Cybersource.BaseData;

namespace CybsClient.Services.DTOs;

public class CaptureContextDto
{
    [JsonPropertyName("ctx")]
    public string? Ctx { get; set; }

    // These two must stay PascalCase to match what the server actually emits - its
    // CaptureContextDto declares [JsonPropertyName("B2cCustomerId")] and ("OrderId"),
    // unlike every other property here, which is camelCase. The pages deserialize this with
    // a bare JsonSerializer.Deserialize<CaptureContextDto>(json), and System.Text.Json
    // matches names case-SENSITIVELY by default, so camelCase names here bound to nothing:
    // both ids arrived null, PersistUnifiedAuth's Convert.ToInt32(null) produced 0, and every
    // legacy Unified Checkout payment died on FK_PaymentCardInfo_B2cCustomers after the card
    // had already been charged.
    [JsonPropertyName("B2cCustomerId")]
    public string? B2cCustomerId { get; set; }

    [JsonPropertyName("OrderId")]
    public string? OrderId { get; set; }

    [JsonPropertyName("clientReferenceInformation")]
    public ClientReferenceInformation? ClientReferenceInformation { get; set; } = new();

    [JsonPropertyName("orderInformation")]
    public OrderInformation? OrderInformation { get; set; } = new();

    [JsonPropertyName("billTo")]
    public BillTo? BillTo { get; set; } = new();

    [JsonPropertyName("tokenInformation")]
    public TokenInformation? TokenInformation { get; set; } = new();

    [JsonPropertyName("processingInformation")]
    public ProcessingInformation? ProcessingInformation { get; set; } = new();

}

