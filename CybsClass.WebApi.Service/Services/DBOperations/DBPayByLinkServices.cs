using CybsClass.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace CybsClass.WebApi.Service.Services.DBOperations;

public static class DBPayByLinkServices
{
    public static Task<DbResult<List<PayByLinkTransaction>>> GetAllPayByLinkTransactionsAsync() =>
        DbErrorHandler.GuardAsync(nameof(GetAllPayByLinkTransactionsAsync), async () =>
        {
            using CybsDbContext db = new();
            return await db.PayByLinkTransactions
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        });

    public static Task<DbResult<PayByLinkTransaction?>> GetByIdAsync(int payByLinkTransactionId) =>
        DbErrorHandler.GuardAsync<PayByLinkTransaction?>(nameof(GetByIdAsync), async () =>
        {
            using CybsDbContext db = new();
            return await db.PayByLinkTransactions.FindAsync(payByLinkTransactionId);
        });
}
