using Microsoft.EntityFrameworkCore;
using CybsClass.EntityModels;

namespace CybsClass.WebApi.Service.Services.DBOperations
{
    public class DBCategoryServices
    {
        public static Task<DbResult<List<Category>>> GetAllCategories() =>
            DbErrorHandler.GuardAsync(nameof(GetAllCategories), async () =>
            {
                Console.WriteLine("[DBCategoryServices] Fetching all categories.");
                using CybsDbContext db = new();
                return await db.Categories.ToListAsync();
            });

        public static Task<DbResult<Category?>> GetCategoryById(int id) =>
            DbErrorHandler.GuardAsync<Category?>(nameof(GetCategoryById), async () =>
            {
                Console.WriteLine($"[DBCategoryServices] Fetching category with ID {id}.");
                using CybsDbContext db = new();
                return await db.Categories.AsNoTracking()
                    .FirstOrDefaultAsync(model => model.CategoryId == id);
            });

        public static Task<DbResult<int>> UpdateCategory(int id, Category category) =>
            DbErrorHandler.GuardAsync(nameof(UpdateCategory), async () =>
            {
                Console.WriteLine($"[DBCategoryServices] Updating category with ID {id}.");
                using CybsDbContext db = new();
                return await db.Categories
                    .Where(model => model.CategoryId == id)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(m => m.CategoryName, category.CategoryName)
                        .SetProperty(m => m.Description, category.Description)
                        .SetProperty(m => m.Picture, category.Picture));
            });

        public static Task<DbResult<Category?>> CreateCategory(Category category) =>
            DbErrorHandler.GuardAsync<Category?>(nameof(CreateCategory), async () =>
            {
                Console.WriteLine("[DBCategoryServices] Inserting new category.");
                using CybsDbContext db = new();
                db.Categories.Add(category);
                await db.SaveChangesAsync();
                Console.WriteLine($"[DBCategoryServices] Category created with ID {category.CategoryId}.");
                return category;
            });

        public static Task<DbResult<int>> DeleteCategory(int id) =>
            DbErrorHandler.GuardAsync(nameof(DeleteCategory), async () =>
            {
                Console.WriteLine($"[DBCategoryServices] Deleting category with ID {id}.");
                using CybsDbContext db = new();
                return await db.Categories
                    .Where(model => model.CategoryId == id)
                    .ExecuteDeleteAsync();
            });
    }
}
