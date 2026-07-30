using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CybsClass.EntityModels;

// Seeded eCheck test routing/account values, drawn at random by "Use Defaults" on
// /echeckcheckout — the eCheck counterpart to how /checkout draws a random card.
// Modelled on NetworkTokenTestCard.
//
// EF Core's default table-naming convention maps to the DbSet property name, not the class
// name, so the explicit [Table] attribute is what guarantees the singular SQL table name is
// used regardless of how the DbSet is named — see the NetworkTokenTestCards-vs-
// NetworkTokenTestCard bug that made every query 500 with "Invalid object name".
[Table("ECheckTestAccount")]
public partial class ECheckTestAccount
{
    [Key]
    public int ECheckTestAccountId { get; set; }

    // paymentInformation.bank.routingNumber
    [StringLength(9)]
    public string RoutingNumber { get; set; } = null!;

    // paymentInformation.bank.account.number
    [StringLength(17)]
    public string AccountNumber { get; set; } = null!;

    // paymentInformation.bank.account.type — C = checking, S = savings, X = corporate checking
    [StringLength(1)]
    public string AccountType { get; set; } = null!;

    // processingInformation.bankTransferOptions.secCode — ccd | ppd | tel | web.
    // Null means the node is omitted entirely, which is a valid (and documented) request shape.
    [StringLength(3)]
    public string? SecCode { get; set; }

    [StringLength(80)]
    public string? BankName { get; set; }

    // DEBIT | RECURRING | TOKEN — which of the four documented flows this row suits
    [StringLength(20)]
    public string TestCategory { get; set; } = null!;

    // Expected CyberSource status. For eCheck the success value is PENDING, not AUTHORIZED.
    [StringLength(50)]
    public string ScenarioOutcome { get; set; } = null!;

    public bool IsSuccess { get; set; }

    [StringLength(160)]
    public string? DisplayLabel { get; set; }

    // Which CyberSource document the value came from, so an unverified row can never be
    // mistaken for a documented one.
    [StringLength(255)]
    public string? SourceReference { get; set; }
}
