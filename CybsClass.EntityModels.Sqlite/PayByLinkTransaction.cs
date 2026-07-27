using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CybsClass.EntityModels;

[Table("PayByLinkTransaction")]
public partial class PayByLinkTransaction
{
    [Key]
    public int PayByLinkTransactionId { get; set; }

    public Guid PayByLinkUuid { get; set; }

    [StringLength(100)]
    public string? CybersourceLinkId { get; set; }

    [StringLength(500)]
    public string? PaymentLink { get; set; }

    [StringLength(100)]
    public string? PurchaseNumber { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? TotalAmount { get; set; }

    [StringLength(10)]
    public string? Currency { get; set; }

    [StringLength(200)]
    public string? CustomerName { get; set; }

    [StringLength(200)]
    public string? Email { get; set; }

    [StringLength(50)]
    public string? Phone { get; set; }

    [StringLength(20)]
    public string? DeliveryMethod { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "CREATED";

    public string? TransactionJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
