using System.ComponentModel.DataAnnotations;

namespace CybsClass.EntityModels;

public partial class NetworkTokenTestCard
{
    [Key]
    public int NetworkTokenTestCardId { get; set; }

    [StringLength(40)]
    public string CardBrand { get; set; } = null!;

    [StringLength(40)]
    public string AccountNumber { get; set; } = null!;

    [StringLength(2)]
    public string? ExpMonth { get; set; }

    [StringLength(4)]
    public string? ExpYear { get; set; }

    [StringLength(4)]
    public string? Cvv { get; set; }

    [StringLength(10)]
    public string TestCategory { get; set; } = null!;

    [StringLength(50)]
    public string ScenarioOutcome { get; set; } = null!;

    public bool IsSuccess { get; set; }

    [StringLength(160)]
    public string? DisplayLabel { get; set; }
}
