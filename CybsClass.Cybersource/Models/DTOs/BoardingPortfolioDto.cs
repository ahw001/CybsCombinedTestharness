using System.Text.Json.Serialization;
using CybsClass.Cybersource.Models.BaseData;

namespace CybsClass.Cybersource.Models.DTOs;

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
    public ErrorObject? Error { get; set; }
}
