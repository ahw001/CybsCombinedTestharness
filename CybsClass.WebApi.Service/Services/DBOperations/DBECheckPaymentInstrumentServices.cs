using Microsoft.EntityFrameworkCore;
using CybsClass.EntityModels;
using CybsClass.Cybersource.Models.DTOs;

namespace CybsClass.WebApi.Service.Services.DBOperations
{
    // Saved eCheck TMS tokens. Rows are written by PersistECheckTransaction when a TOKEN_CREATE
    // flow returns token ids; this service only reads them back for the token-debit dropdown.
    public static class DBECheckPaymentInstrumentServices
    {
        /// <summary>
        /// Every saved eCheck token, newest first. The token-debit page offers these regardless of
        /// which customer minted them — the sandbox has no real account boundary and restricting
        /// the list to one B2cCustomerId would leave the dropdown empty on a fresh session.
        /// </summary>
        public static Task<DbResult<List<ECheckPaymentInstrumentDto>>> GetAllAsync(CybsDbContext context) =>
            DbErrorHandler.GuardAsync(nameof(GetAllAsync), async () =>
            {
                Console.WriteLine("[DBECheckPaymentInstrumentServices] Fetching saved eCheck tokens ...");

                var entities = await context.ECheckPaymentInstruments
                    .Where(e => e.CustomerTokenId != null)
                    .OrderByDescending(e => e.ECheckPaymentInstrumentId)
                    .ToListAsync();

                return entities.Select(ToDto).ToList();
            });

        public static Task<DbResult<List<ECheckPaymentInstrumentDto>>> GetByCustomerAsync(
            CybsDbContext context, int b2ccustomerid) =>
            DbErrorHandler.GuardAsync(nameof(GetByCustomerAsync), async () =>
            {
                var entities = await context.ECheckPaymentInstruments
                    .Where(e => e.B2cCustomerId == b2ccustomerid)
                    .OrderByDescending(e => e.ECheckPaymentInstrumentId)
                    .ToListAsync();

                return entities.Select(ToDto).ToList();
            });

        private static ECheckPaymentInstrumentDto ToDto(ECheckPaymentInstrument e) => new()
        {
            ECheckPaymentInstrumentId = e.ECheckPaymentInstrumentId,
            B2cCustomerId = e.B2cCustomerId,
            CustomerTokenId = e.CustomerTokenId,
            PaymentInstrumentId = e.PaymentInstrumentId,
            InstrumentIdentifierId = e.InstrumentIdentifierId,
            InstrumentIdentifierState = e.InstrumentIdentifierState,
            RoutingNumber = e.RoutingNumber,
            MaskedAccountNumber = e.MaskedAccountNumber,
            AccountType = e.AccountType,
            BankName = e.BankName,
            DisplayLabel = e.DisplayLabel,
            SourceTransactionId = e.SourceTransactionId,
            CreatedAt = e.CreatedAt
        };
    }
}
