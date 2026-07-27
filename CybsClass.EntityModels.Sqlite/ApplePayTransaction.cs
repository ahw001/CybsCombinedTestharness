using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CybsClass.EntityModels;

// EF Core's default table-naming convention maps to the DbSet property name
// (CybsDbContext.ApplePayTransactions, plural), not this class name. The actual SQL table
// (per the DDL in ApplePayWebPlanningOutput.md, already created on both LAN and Winhost) is named
// singular — ApplePayTransaction — so this attribute is required or every query 500s with
// "Invalid object name 'ApplePayTransactions'".
[Table("ApplePayTransaction")]
public partial class ApplePayTransaction
{
    [Key]
    public int ApplePayTransactionsId { get; set; }

    [StringLength(255)]
    public string Id { get; set; } = null!;

    public int OrderId { get; set; }

    [StringLength(255)]
    public string? ClientReferenceCode { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    public DateTime? SubmitTimeUtc { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? AuthorizedAmount { get; set; }

    [StringLength(10)]
    public string? Currency { get; set; }

    [StringLength(50)]
    public string? CardNetwork { get; set; }

    [StringLength(50)]
    public string? MaskedPan { get; set; }

    [StringLength(10)]
    public string? PaymentSolution { get; set; }

    [StringLength(10)]
    public string? TransactionType { get; set; }

    public string? DecryptedTokenJson { get; set; }

    public string? RequestTransactionJson { get; set; }

    public string? ResponseTransactionJson { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("ApplePayTransactions")]
    public virtual Order Order { get; set; } = null!;
}
