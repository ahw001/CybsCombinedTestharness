using Microsoft.EntityFrameworkCore;
using CybsClass.EntityModels;

namespace CybsClass.WebApi.Service.Services.DBOperations
{
    public class DBElectronicProductServices
    {
        public static Task<DbResult<List<ElectronicProduct>>> GetAllElectronicProducts() =>
            DbErrorHandler.GuardAsync(nameof(GetAllElectronicProducts), async () =>
            {
                Console.WriteLine("[DBElectronicProductServices] Fetching all electronic products.");
                using CybsDbContext db = new();
                return await db.ElectronicProducts.ToListAsync();
            });

        public static Task<DbResult<ElectronicProduct?>> GetElectronicProductById(int id) =>
            DbErrorHandler.GuardAsync<ElectronicProduct?>(nameof(GetElectronicProductById), async () =>
            {
                Console.WriteLine($"[DBElectronicProductServices] Fetching electronic product with ID {id}.");
                using CybsDbContext db = new();
                return await db.ElectronicProducts.AsNoTracking()
                    .FirstOrDefaultAsync(model => model.ElectronicProductId == id);
            });

        public static Task<DbResult<int>> UpdateElectronicProduct(int id, ElectronicProduct electronicProduct) =>
            DbErrorHandler.GuardAsync(nameof(UpdateElectronicProduct), async () =>
            {
                Console.WriteLine($"[DBElectronicProductServices] Updating electronic product with ID {id}.");
                using CybsDbContext db = new();
                return await db.ElectronicProducts
                    .Where(model => model.ElectronicProductId == id)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(m => m.ProductName, electronicProduct.ProductName)
                        .SetProperty(m => m.ProductLabel, electronicProduct.ProductLabel)
                        .SetProperty(m => m.UnitPrice, electronicProduct.UnitPrice)
                        .SetProperty(m => m.ProductSku, electronicProduct.ProductSku)
                        .SetProperty(m => m.Picture, electronicProduct.Picture)
                        .SetProperty(m => m.Brand, electronicProduct.Brand));
            });

        public static Task<DbResult<ElectronicProduct?>> CreateElectronicProduct(ElectronicProduct electronicProduct) =>
            DbErrorHandler.GuardAsync<ElectronicProduct?>(nameof(CreateElectronicProduct), async () =>
            {
                Console.WriteLine($"[DBElectronicProductServices] Inserting new electronic product with SKU {electronicProduct.ProductSku}.");
                using CybsDbContext db = new();
                db.ElectronicProducts.Add(electronicProduct);
                await db.SaveChangesAsync();
                Console.WriteLine($"[DBElectronicProductServices] Electronic product created with ID {electronicProduct.ElectronicProductId}.");
                return electronicProduct;
            });

        public static Task<DbResult<int>> DeleteElectronicProduct(int id) =>
            DbErrorHandler.GuardAsync(nameof(DeleteElectronicProduct), async () =>
            {
                Console.WriteLine($"[DBElectronicProductServices] Deleting electronic product with ID {id}.");
                using CybsDbContext db = new();
                return await db.ElectronicProducts
                    .Where(model => model.ElectronicProductId == id)
                    .ExecuteDeleteAsync();
            });
    }
}
