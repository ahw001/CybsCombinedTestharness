using Microsoft.EntityFrameworkCore;
using CybsClass.EntityModels;

namespace CybsClass.WebApi.Service.Services.DBOperations
{
    public class DBMerchantSampleDatumServices
    {
        public static Task<DbResult<MerchantSampleDatum?>> GetRandomMerchantSampleDatum() =>
            DbErrorHandler.GuardAsync<MerchantSampleDatum?>(nameof(GetRandomMerchantSampleDatum), async () =>
            {
                Console.WriteLine("[DBMerchantSampleDatumServices] Fetching random merchant sample datum.");
                using CybsDbContext db = new();
                int count = await db.MerchantSampleData.CountAsync();
                if (count == 0) return null;
                int skip = new Random().Next(count);
                return await db.MerchantSampleData.Skip(skip).FirstOrDefaultAsync();
            });

        public static Task<DbResult<MerchantSampleDatum?>> GetMerchantSampleDatumById(int id) =>
            DbErrorHandler.GuardAsync<MerchantSampleDatum?>(nameof(GetMerchantSampleDatumById), async () =>
            {
                Console.WriteLine($"[DBMerchantSampleDatumServices] Fetching merchant sample datum with ID {id}.");
                using CybsDbContext db = new();
                return await db.MerchantSampleData.AsNoTracking()
                    .FirstOrDefaultAsync(model => model.SampleMerchantId == id);
            });

        public static Task<DbResult<int>> UpdateMerchantSampleDatum(int id, MerchantSampleDatum merchantSampleDatum) =>
            DbErrorHandler.GuardAsync(nameof(UpdateMerchantSampleDatum), async () =>
            {
                Console.WriteLine($"[DBMerchantSampleDatumServices] Updating merchant sample datum with ID {id}.");
                using CybsDbContext db = new();
                return await db.MerchantSampleData
                    .Where(model => model.SampleMerchantId == id)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(m => m.OrganizationId, merchantSampleDatum.OrganizationId)
                        .SetProperty(m => m.Status, merchantSampleDatum.Status)
                        .SetProperty(m => m.Type, merchantSampleDatum.Type)
                        .SetProperty(m => m.Configurable, merchantSampleDatum.Configurable)
                        .SetProperty(m => m.Country, merchantSampleDatum.Country)
                        .SetProperty(m => m.Address1, merchantSampleDatum.Address1)
                        .SetProperty(m => m.PostalCode, merchantSampleDatum.PostalCode)
                        .SetProperty(m => m.AdministrativeArea, merchantSampleDatum.AdministrativeArea)
                        .SetProperty(m => m.Locality, merchantSampleDatum.Locality)
                        .SetProperty(m => m.BusinessContactFirstName, merchantSampleDatum.BusinessContactFirstName)
                        .SetProperty(m => m.BusinessContactLastName, merchantSampleDatum.BusinessContactLastName)
                        .SetProperty(m => m.BusinessContactPhoneNumber, merchantSampleDatum.BusinessContactPhoneNumber)
                        .SetProperty(m => m.BusinessContactEmail, merchantSampleDatum.BusinessContactEmail)
                        .SetProperty(m => m.TechnicalContactFirstName, merchantSampleDatum.TechnicalContactFirstName)
                        .SetProperty(m => m.TechnicalContactLastName, merchantSampleDatum.TechnicalContactLastName)
                        .SetProperty(m => m.TechnicalContactphoneNumber, merchantSampleDatum.TechnicalContactphoneNumber)
                        .SetProperty(m => m.TechnicalContactEmail, merchantSampleDatum.TechnicalContactEmail)
                        .SetProperty(m => m.EmergencyContactFirstName, merchantSampleDatum.EmergencyContactFirstName)
                        .SetProperty(m => m.EmergencyContactLastName, merchantSampleDatum.EmergencyContactLastName)
                        .SetProperty(m => m.EmergencyContactPhoneNumber, merchantSampleDatum.EmergencyContactPhoneNumber)
                        .SetProperty(m => m.EmergencyContactEmail, merchantSampleDatum.EmergencyContactEmail)
                        .SetProperty(m => m.Name, merchantSampleDatum.Name)
                        .SetProperty(m => m.WebsiteUrl, merchantSampleDatum.WebsiteUrl)
                        .SetProperty(m => m.BusinessInformationPhoneNumber, merchantSampleDatum.BusinessInformationPhoneNumber)
                        .SetProperty(m => m.BusinessInformationTimeZone, merchantSampleDatum.BusinessInformationTimeZone)
                        .SetProperty(m => m.MerchantCategoryCode, merchantSampleDatum.MerchantCategoryCode));
            });

        public static Task<DbResult<MerchantSampleDatum?>> CreateMerchantSampleDatum(MerchantSampleDatum merchantSampleDatum) =>
            DbErrorHandler.GuardAsync<MerchantSampleDatum?>(nameof(CreateMerchantSampleDatum), async () =>
            {
                Console.WriteLine("[DBMerchantSampleDatumServices] Inserting new merchant sample datum.");
                using CybsDbContext db = new();
                db.MerchantSampleData.Add(merchantSampleDatum);
                await db.SaveChangesAsync();
                Console.WriteLine($"[DBMerchantSampleDatumServices] Merchant sample datum created with ID {merchantSampleDatum.SampleMerchantId}.");
                return merchantSampleDatum;
            });

        public static Task<DbResult<int>> DeleteMerchantSampleDatum(int id) =>
            DbErrorHandler.GuardAsync(nameof(DeleteMerchantSampleDatum), async () =>
            {
                Console.WriteLine($"[DBMerchantSampleDatumServices] Deleting merchant sample datum with ID {id}.");
                using CybsDbContext db = new();
                return await db.MerchantSampleData
                    .Where(model => model.SampleMerchantId == id)
                    .ExecuteDeleteAsync();
            });
    }
}
