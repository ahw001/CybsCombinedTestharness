using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CybsClass.EntityModels;

[Table("TokenizeTransaction")]
public partial class TokenizeTransaction
{
    [Key]
    public int TokenizeTransactionId { get; set; }

    public DateTime CreatedAt { get; set; }

    [StringLength(40)]
    public string EndpointMode { get; set; } = null!;

    [StringLength(100)]
    public string? TokenId { get; set; }

    [StringLength(40)]
    public string? TokenState { get; set; }

    [StringLength(40)]
    public string? NetworkTokenNumber { get; set; }

    [StringLength(40)]
    public string? CardType { get; set; }

    [StringLength(64)]
    public string? PaymentAccountReference { get; set; }

    [StringLength(8)]
    public string? RequestCardSuffix { get; set; }

    [StringLength(2)]
    public string? ExpMonth { get; set; }

    [StringLength(4)]
    public string? ExpYear { get; set; }

    public string? ResponseTransactionJson { get; set; }
}
