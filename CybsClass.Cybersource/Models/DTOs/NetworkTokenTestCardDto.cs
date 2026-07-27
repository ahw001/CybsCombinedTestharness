using CybsClass.Cybersource.Models.BaseData;

namespace CybsClass.Cybersource.Models.DTOs;

public class NetworkTokenTestCardDto
{
    public int NetworkTokenTestCardId { get; set; }
    public string? CardBrand { get; set; }
    public string? AccountNumber { get; set; }
    public string? ExpMonth { get; set; }
    public string? ExpYear { get; set; }
    public string? Cvv { get; set; }
    public string? TestCategory { get; set; }
    public string? ScenarioOutcome { get; set; }
    public bool IsSuccess { get; set; }
    public string? DisplayLabel { get; set; }
    public ErrorObject? Error { get; set; }
}
