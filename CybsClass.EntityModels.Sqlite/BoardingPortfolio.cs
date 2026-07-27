using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CybsClass.EntityModels;

[Table("BoardingPortfolio")]
public class BoardingPortfolio
{
    [Key]
    public int BoardingPortfolioId { get; set; }

    [Required]
    [StringLength(100)]
    public string PortfolioName { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? ExpectedSignatureMerchantId { get; set; }

    [StringLength(100)]
    public string? BoardingPackageId { get; set; }

    [StringLength(100)]
    public string? CardProcessingTemplateId { get; set; }

    [StringLength(100)]
    public string? VirtualTerminalTemplateId { get; set; }

    [StringLength(100)]
    public string? TokenManagementTemplateId { get; set; }

    [StringLength(100)]
    public string? PayerAuthenticationTemplateId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    [InverseProperty("BoardingPortfolio")]
    public virtual ICollection<BoardingOrganization> Organizations { get; set; } = new List<BoardingOrganization>();
}
