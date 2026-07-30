using System;
using CybsClass.Cybersource.Models.BaseData;

namespace CybsClass.Cybersource.Models.DTOs;

// Outbound shape for GET /api/echeck/paymentinstruments/{b2ccustomerid} — the saved eCheck
// tokens offered by /echecktokencheckout's dropdown.
public class ECheckPaymentInstrumentDto
{
    public int ECheckPaymentInstrumentId { get; set; }
    public int B2cCustomerId { get; set; }
    public string? CustomerTokenId { get; set; }
    public string? PaymentInstrumentId { get; set; }
    public string? InstrumentIdentifierId { get; set; }
    public string? InstrumentIdentifierState { get; set; }
    public string? RoutingNumber { get; set; }
    public string? MaskedAccountNumber { get; set; }
    public string? AccountType { get; set; }
    public string? BankName { get; set; }
    public string? DisplayLabel { get; set; }
    public string? SourceTransactionId { get; set; }
    public DateTime CreatedAt { get; set; }

    public ErrorObject? Error { get; set; }
}
