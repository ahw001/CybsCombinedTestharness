using CybsClass.Cybersource.Models.BaseData;

namespace CybsClass.Cybersource.Models.DTOs;

// Outbound shape for GET /api/echeck/test-accounts. Mirrors NetworkTokenTestCardDto.
public class ECheckTestAccountDto
{
    public int ECheckTestAccountId { get; set; }
    public string? RoutingNumber { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountType { get; set; }
    public string? SecCode { get; set; }
    public string? BankName { get; set; }
    public string? TestCategory { get; set; }
    public string? ScenarioOutcome { get; set; }
    public bool IsSuccess { get; set; }
    public string? DisplayLabel { get; set; }
    public string? SourceReference { get; set; }

    public ErrorObject? Error { get; set; }
}
