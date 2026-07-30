using Microsoft.EntityFrameworkCore;
using CybsClass.EntityModels;
using CybsClass.Cybersource.Models.DTOs;

namespace CybsClass.WebApi.Service.Services.DBOperations
{
    // Seeded eCheck test routing/account values. Read-only — the table is populated by the DDL,
    // not by the application.
    public static class DBECheckTestAccountServices
    {
        public static Task<DbResult<List<ECheckTestAccountDto>>> GetAllAsync(CybsDbContext context) =>
            DbErrorHandler.GuardAsync(nameof(GetAllAsync), async () =>
            {
                Console.WriteLine("[DBECheckTestAccountServices] Fetching eCheck test accounts ...");

                var entities = await context.ECheckTestAccounts
                    .OrderBy(e => e.ECheckTestAccountId)
                    .ToListAsync();

                return entities.Select(ToDto).ToList();
            });

        /// <summary>
        /// Single row by id — used when the client submits a chosen test account rather than a
        /// hand-edited one, so the server never has to trust client-supplied bank values it
        /// could have looked up itself.
        /// </summary>
        public static Task<DbResult<ECheckTestAccountDto?>> GetByIdAsync(CybsDbContext context, int echecktestaccountid) =>
            DbErrorHandler.GuardAsync<ECheckTestAccountDto?>(nameof(GetByIdAsync), async () =>
            {
                var entity = await context.ECheckTestAccounts
                    .FirstOrDefaultAsync(e => e.ECheckTestAccountId == echecktestaccountid);

                return entity is null ? null : ToDto(entity);
            });

        private static ECheckTestAccountDto ToDto(ECheckTestAccount e) => new()
        {
            ECheckTestAccountId = e.ECheckTestAccountId,
            RoutingNumber = e.RoutingNumber,
            AccountNumber = e.AccountNumber,
            AccountType = e.AccountType,
            SecCode = e.SecCode,
            BankName = e.BankName,
            TestCategory = e.TestCategory,
            ScenarioOutcome = e.ScenarioOutcome,
            IsSuccess = e.IsSuccess,
            DisplayLabel = e.DisplayLabel,
            SourceReference = e.SourceReference
        };
    }
}
