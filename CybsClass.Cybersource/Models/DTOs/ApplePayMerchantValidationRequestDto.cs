using CybsClass.Cybersource.Models.BaseData;
using System.Text.Json.Serialization;

namespace CybsClass.Cybersource.Models.DTOs;

public class ApplePayMerchantValidationRequestDto
{
    [JsonPropertyName("validationUrl")]
    public string? ValidationUrl { get; set; }

    [JsonPropertyName("error")]
    public ErrorObject? Error { get; set; }
}
