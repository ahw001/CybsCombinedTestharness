using CybsClass.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace CybsClass.WebApi.Service.Services.DBOperations;

public static class DBPayByLinkServices
{
    public static async Task<List<PayByLinkTransaction>> GetAllPayByLinkTransactionsAsync()
    {
        using CybsDbContext db = new();
        return await db.PayByLinkTransactions
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public static async Task<PayByLinkTransaction?> GetByIdAsync(int payByLinkTransactionId)
    {
        using CybsDbContext db = new();
        return await db.PayByLinkTransactions.FindAsync(payByLinkTransactionId);
    }
}
