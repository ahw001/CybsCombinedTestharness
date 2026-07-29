using CybsClass.Cybersource.Models.BaseData;
using System.Text.Json.Serialization;
namespace CybsClass.Cybersource.Models.DTOs;

public partial class IndividualTransactionDto
{
    public int TransactionId { get; set; }

    public string RequestId { get; set; } = null!;

    public string TransactionType { get; set; } = null!;

    public string ReferenceTransactionId { get; set; } = null!;

    public string? ResponseTransactionJson { get; set; }

    [JsonPropertyName("error")]
    public ErrorObject? Error { get; set; }
}
