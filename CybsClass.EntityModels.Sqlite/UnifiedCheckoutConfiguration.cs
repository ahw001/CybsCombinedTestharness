using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CybsClass.EntityModels;

// Named, saveable Unified Checkout capture-context behavior settings — see
// UnifiedCheckoutPlanning.md Phase 0. DDL already executed by the user (local + Winhost
// DB_166624_cybssampledb) before this entity was written; column names/types below must match
// that DDL exactly (CybsDbContext's DbSet property name must also match the table name — see the
// NetworkTokenTestCard naming-bug precedent).
[Table("UnifiedCheckoutConfiguration")]
public class UnifiedCheckoutConfiguration
{
    [Key]
    public int UnifiedCheckoutConfigurationId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    // Comma-delimited: PANENTRY,CHECK,APPLEPAY,...
    [StringLength(200)]
    public string? AllowedPaymentTypes { get; set; }

    // Comma-delimited: VISA,MASTERCARD,AMEX,...
    [StringLength(200)]
    public string? AllowedCardNetworks { get; set; }

    [StringLength(2)]
    public string? Country { get; set; }

    [StringLength(10)]
    public string? Locale { get; set; }

    [StringLength(40)]
    public string? ButtonType { get; set; }

    // FULL | PARTIAL | NONE
    [StringLength(20)]
    public string? BillingType { get; set; }

    public bool RequestShipping { get; set; }
    public bool RequestEmail { get; set; }
    public bool RequestPhone { get; set; }
    public bool RequestSaveCredentials { get; set; }
    public bool ShowConfirmationStep { get; set; }
    public bool ShowAcceptedNetworkIcons { get; set; }

    // AUTH | CAPTURE | PREFER_AUTH — null = no completeMandate (Goal 1 scope)
    [StringLength(20)]
    public string? CompleteMandateType { get; set; }

    public bool EnableTms { get; set; }

    // Comma-delimited: customer,instrumentIdentifier,paymentInstrument
    [StringLength(200)]
    public string? TmsTokenTypes { get; set; }

    public bool Enable3ds { get; set; }
    public bool EnableDecisionManager { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
