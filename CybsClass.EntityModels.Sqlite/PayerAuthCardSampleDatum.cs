using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CybsClass.EntityModels;

public partial class PayerAuthCardSampleDatum
{
    [Key]
    public int SamplePayAuthPaymentCardId { get; set; }

    [StringLength(40)]
    public string CardBrand { get; set; } = null!;

    [StringLength(40)]
    public string AccountNumber { get; set; } = null!;

    [StringLength(2)]
    public string? ExpMonth { get; set; }

    [StringLength(4)]
    public string? ExpYear { get; set; }

    [StringLength(3)]
    public string? Cvv { get; set; }

    [StringLength(10)]
    public string? TestCaseId { get; set; }

    [StringLength(200)]
    public string? TestCaseName { get; set; }

    [StringLength(10)]
    public string? SpecVersion { get; set; }

    [StringLength(10)]
    public string? CardTypeCode { get; set; }

    [StringLength(5)]
    public string? VeresEnrolled { get; set; }

    public bool StepUpRequired { get; set; }

    [StringLength(5)]
    public string? ParesStatus { get; set; }

    [StringLength(5)]
    public string? EciRawValue { get; set; }

    [StringLength(40)]
    public string? EciStringValue { get; set; }

    [StringLength(10)]
    public string? RequiresCountryOverride { get; set; }

    [StringLength(500)]
    public string? OtherInputNotes { get; set; }

    [StringLength(1000)]
    public string? ExpectedOutcomeNotes { get; set; }

    [StringLength(500)]
    public string? SourceUrl { get; set; }
}
