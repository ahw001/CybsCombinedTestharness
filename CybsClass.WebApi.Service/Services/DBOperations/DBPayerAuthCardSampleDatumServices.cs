using Microsoft.EntityFrameworkCore;
using CybsClass.EntityModels;

namespace CybsClass.WebApi.Service.Services.DBOperations
{
    public class DBPayerAuthCardSampleDatumServices
    {
        public static Task<DbResult<List<PayerAuthCardSampleDatum>>> GetAllPayerAuthCardSampleData() =>
            DbErrorHandler.GuardAsync(nameof(GetAllPayerAuthCardSampleData), async () =>
            {
                Console.WriteLine("[DBPayerAuthCardSampleDatumServices] Fetching all payer auth card sample data.");
                using CybsDbContext db = new();
                return await db.PayerAuthCardSampleData.ToListAsync();
            });

        public static Task<DbResult<PayerAuthCardSampleDatum?>> GetPayerAuthCardSampleDatumById(int id) =>
            DbErrorHandler.GuardAsync<PayerAuthCardSampleDatum?>(nameof(GetPayerAuthCardSampleDatumById), async () =>
            {
                Console.WriteLine($"[DBPayerAuthCardSampleDatumServices] Fetching payer auth card sample datum with ID {id}.");
                using CybsDbContext db = new();
                return await db.PayerAuthCardSampleData.AsNoTracking()
                    .FirstOrDefaultAsync(model => model.SamplePayAuthPaymentCardId == id);
            });

        public static Task<DbResult<int>> UpdatePayerAuthCardSampleDatum(int id, PayerAuthCardSampleDatum payerAuthCardSampleDatum) =>
            DbErrorHandler.GuardAsync(nameof(UpdatePayerAuthCardSampleDatum), async () =>
            {
                Console.WriteLine($"[DBPayerAuthCardSampleDatumServices] Updating payer auth card sample datum with ID {id}.");
                using CybsDbContext db = new();
                return await db.PayerAuthCardSampleData
                    .Where(model => model.SamplePayAuthPaymentCardId == id)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(m => m.CardBrand, payerAuthCardSampleDatum.CardBrand)
                        .SetProperty(m => m.AccountNumber, payerAuthCardSampleDatum.AccountNumber)
                        .SetProperty(m => m.ExpMonth, payerAuthCardSampleDatum.ExpMonth)
                        .SetProperty(m => m.ExpYear, payerAuthCardSampleDatum.ExpYear)
                        .SetProperty(m => m.Cvv, payerAuthCardSampleDatum.Cvv));
            });

        public static Task<DbResult<PayerAuthCardSampleDatum?>> CreatePayerAuthCardSampleDatum(PayerAuthCardSampleDatum payerAuthCardSampleDatum) =>
            DbErrorHandler.GuardAsync<PayerAuthCardSampleDatum?>(nameof(CreatePayerAuthCardSampleDatum), async () =>
            {
                Console.WriteLine("[DBPayerAuthCardSampleDatumServices] Inserting new payer auth card sample datum.");
                using CybsDbContext db = new();
                db.PayerAuthCardSampleData.Add(payerAuthCardSampleDatum);
                await db.SaveChangesAsync();
                Console.WriteLine($"[DBPayerAuthCardSampleDatumServices] Payer auth card sample datum created with ID {payerAuthCardSampleDatum.SamplePayAuthPaymentCardId}.");
                return payerAuthCardSampleDatum;
            });

        public static Task<DbResult<int>> DeletePayerAuthCardSampleDatum(int id) =>
            DbErrorHandler.GuardAsync(nameof(DeletePayerAuthCardSampleDatum), async () =>
            {
                Console.WriteLine($"[DBPayerAuthCardSampleDatumServices] Deleting payer auth card sample datum with ID {id}.");
                using CybsDbContext db = new();
                return await db.PayerAuthCardSampleData
                    .Where(model => model.SamplePayAuthPaymentCardId == id)
                    .ExecuteDeleteAsync();
            });
    }
}
