using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CybsClass.EntityModels;

[Table("BoardingTokenManagementSubscription")]
public class BoardingTokenManagementSubscription
{
    [Key]
    public int BoardingTokenManagementSubscriptionId { get; set; }

    public int? BoardingTransactingMerchantId { get; set; }

    public bool? Enabled { get; set; }

    // ── Configurable fields (persisted; everything else CyberSource requires
    //    for NT boarding is a hardcoded constant in CallCybsNetworkTokenBoarding) ──
    [StringLength(50)]  public string? OrganizationId { get; set; }
    [StringLength(500)] public string? WebsiteUrl { get; set; }
    [StringLength(200)] public string? BusinessName { get; set; }
    [StringLength(200)] public string? DoingBusinessAs { get; set; }
    [StringLength(100)] public string? AcquirerMerchantId { get; set; }

    [StringLength(50)] public string? CybersourceBoardingStatus { get; set; }
    public DateTime?   SubmittedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("BoardingTransactingMerchantId")]
    public virtual BoardingTransactingMerchant? BoardingTransactingMerchant { get; set; }
}
