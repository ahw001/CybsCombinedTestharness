using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CybsClass.EntityModels;

// TMS token store for bank accounts. Card tokens live on PaymentCardInfo; before this table
// there was nowhere for an eCheck token to go.
//
// This is what makes the fourth documented flow usable: without a persisted token store, the
// "submit a debit using a stored token" page has nothing to offer in its dropdown.
[Table("ECheckPaymentInstrument")]
public partial class ECheckPaymentInstrument
{
    [Key]
    public int ECheckPaymentInstrumentId { get; set; }

    public int B2cCustomerId { get; set; }

    // tokenInformation.customer.id — the value sent back as paymentInformation.customer.id
    [StringLength(40)]
    public string? CustomerTokenId { get; set; }

    [StringLength(40)]
    public string? PaymentInstrumentId { get; set; }

    [StringLength(40)]
    public string? InstrumentIdentifierId { get; set; }

    [StringLength(40)]
    public string? InstrumentIdentifierState { get; set; }

    [StringLength(9)]
    public string? RoutingNumber { get; set; }

    [StringLength(40)]
    public string? MaskedAccountNumber { get; set; }

    [StringLength(1)]
    public string? AccountType { get; set; }

    [StringLength(80)]
    public string? BankName { get; set; }

    // Pre-rendered dropdown text, e.g. "Checking ****7890 - 071923284"
    [StringLength(160)]
    public string? DisplayLabel { get; set; }

    // The eCheck transaction that minted this token
    [StringLength(255)]
    public string? SourceTransactionId { get; set; }

    public string? ResponseTransactionJson { get; set; }

    // See the note on ECheckTransaction.CreatedAt — no SQL DEFAULT, set explicitly in code.
    public DateTime CreatedAt { get; set; }

    [ForeignKey("B2cCustomerId")]
    [InverseProperty("ECheckPaymentInstruments")]
    public virtual B2cCustomer B2cCustomer { get; set; } = null!;
}
