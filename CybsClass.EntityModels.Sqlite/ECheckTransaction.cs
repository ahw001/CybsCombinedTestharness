using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CybsClass.EntityModels;

// eCheck response persistence. Modelled on ApplePayTransaction.
//
// Deliberately NOT a reuse of AuthTransResponse: roughly half of that table's columns are
// card-only (CardType, AvsCode, AvsCodeRaw, ProcInfoApprovalCode, ProcInfoNetworkTransactionId,
// ConsumerAuthenticationToken, ...) and would sit permanently null, while eCheck's own fields
// (routing number, masked account, account type, SEC code, recurring flags) have nowhere to go.
// Apple Pay hit exactly this and got its own table; this follows that precedent.
[Table("ECheckTransaction")]
public partial class ECheckTransaction
{
    [Key]
    public int ECheckTransactionId { get; set; }

    // CyberSource transaction id (response "id")
    [StringLength(255)]
    public string Id { get; set; } = null!;

    public int OrderId { get; set; }

    [StringLength(255)]
    public string? ClientReferenceCode { get; set; }

    // eCheck settles asynchronously — PENDING is the success case, not AUTHORIZED.
    [StringLength(50)]
    public string? Status { get; set; }

    [StringLength(255)]
    public string? ReconciliationId { get; set; }

    public DateTime? SubmitTimeUtc { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TotalAmount { get; set; }

    [StringLength(10)]
    public string? Currency { get; set; }

    [StringLength(9)]
    public string? RoutingNumber { get; set; }

    // Masked only — the full bank account number is never persisted.
    [StringLength(40)]
    public string? MaskedAccountNumber { get; set; }

    [StringLength(1)]
    public string? AccountType { get; set; }

    [StringLength(3)]
    public string? SecCode { get; set; }

    [StringLength(20)]
    public string? CommerceIndicator { get; set; }

    public bool IsRecurring { get; set; }

    public bool? FirstRecurringPayment { get; set; }

    // DEBIT | RECURRING | TOKEN_CREATE | TOKEN_DEBIT
    [StringLength(20)]
    public string? TransactionType { get; set; }

    // tokenInformation.* from a TOKEN_CREATE response
    [StringLength(40)]
    public string? CustomerTokenId { get; set; }

    [StringLength(40)]
    public string? PaymentInstrumentId { get; set; }

    [StringLength(40)]
    public string? InstrumentIdentifierId { get; set; }

    public string? RequestTransactionJson { get; set; }

    public string? ResponseTransactionJson { get; set; }

    // No SQL DEFAULT on this column, deliberately. Column defaults are dropped or mistranslated
    // on the way into SQLite and EF Core omits defaulted columns on insert — the combination
    // that broke SessionTransactionsStore with a NOT NULL constraint failure seen only on
    // SQLite. Requiring the caller to set it keeps the two databases from diverging.
    public DateTime CreatedAt { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("ECheckTransactions")]
    public virtual Order Order { get; set; } = null!;
}
