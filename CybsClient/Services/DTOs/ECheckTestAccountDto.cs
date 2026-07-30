using System.Text.Json.Serialization;

namespace CybsClient.Services.DTOs;

// Client-side copy of the server's ECheckTestAccountDto. These two files are hand-maintained
// in separate repos — check both before debugging a property that arrives null.
public partial class ECheckTestAccountDto
{
    [JsonPropertyName("eCheckTestAccountId")]
    public int ECheckTestAccountId { get; set; }

    [JsonPropertyName("routingNumber")]
    public string? RoutingNumber { get; set; }

    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; set; }

    // C = checking, S = savings, X = corporate checking
    [JsonPropertyName("accountType")]
    public string? AccountType { get; set; }

    [JsonPropertyName("secCode")]
    public string? SecCode { get; set; }

    [JsonPropertyName("bankName")]
    public string? BankName { get; set; }

    [JsonPropertyName("testCategory")]
    public string? TestCategory { get; set; }

    [JsonPropertyName("scenarioOutcome")]
    public string? ScenarioOutcome { get; set; }

    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; set; }

    [JsonPropertyName("displayLabel")]
    public string? DisplayLabel { get; set; }

    [JsonPropertyName("sourceReference")]
    public string? SourceReference { get; set; }
}
