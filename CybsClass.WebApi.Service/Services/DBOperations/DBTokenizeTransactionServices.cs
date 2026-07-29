using Microsoft.EntityFrameworkCore;
using CybsClass.EntityModels;
using CybsClass.Cybersource.Models.DTOs;
using ErrorObject = CybsClass.Cybersource.Models.BaseData.ErrorObject;

namespace CybsClass.WebApi.Service.Services.DBOperations;

public static class DBTokenizeTransactionServices
{
    public static async Task<PagedResultDto<TokenizeTransactionDto>> GetPagedAsync(
        int pageNumber, int pageSize, string? search)
    {
        try
        {
            Console.WriteLine($"[DBTokenizeTransactionServices] GetPagedAsync page={pageNumber} size={pageSize} search={search}");
            using CybsDbContext db = new();

            IQueryable<TokenizeTransaction> q = db.TokenizeTransactions;

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(t =>
                    (t.TokenId != null && t.TokenId.Contains(search)) ||
                    (t.NetworkTokenNumber != null && t.NetworkTokenNumber.Contains(search)));

            int totalCount = await q.CountAsync();

            var items = await q
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .Select(t => new TokenizeTransactionDto
                {
                    TokenizeTransactionId   = t.TokenizeTransactionId,
                    CreatedAt               = t.CreatedAt,
                    EndpointMode            = t.EndpointMode,
                    TokenId                 = t.TokenId,
                    TokenState              = t.TokenState,
                    NetworkTokenNumber      = t.NetworkTokenNumber,
                    CardType                = t.CardType,
                    PaymentAccountReference = t.PaymentAccountReference,
                    RequestCardSuffix       = t.RequestCardSuffix,
                    ExpMonth                = t.ExpMonth,
                    ExpYear                 = t.ExpYear,
                    ResponseTransactionJson = t.ResponseTransactionJson
                })
                .ToListAsync();

            Console.WriteLine($"[DBTokenizeTransactionServices] GetPagedAsync returning {items.Count} of {totalCount}");

            return new PagedResultDto<TokenizeTransactionDto>
            {
                Items      = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize   = pageSize
            };
        }
        catch (Exception ex)
        {
            DbErrorHandler.Log(nameof(GetPagedAsync), ex);
            return new PagedResultDto<TokenizeTransactionDto>
            {
                Error = new ErrorObject { Message = ex.Message }
            };
        }
    }

    public static async Task<TokenizeTransactionDto?> GetByIdAsync(int id)
    {
        try
        {
            Console.WriteLine($"[DBTokenizeTransactionServices] GetByIdAsync id={id}");
            using CybsDbContext db = new();

            var t = await db.TokenizeTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TokenizeTransactionId == id);

            if (t is null)
            {
                Console.WriteLine($"[DBTokenizeTransactionServices] GetByIdAsync: id={id} not found");
                return null;
            }

            return new TokenizeTransactionDto
            {
                TokenizeTransactionId   = t.TokenizeTransactionId,
                CreatedAt               = t.CreatedAt,
                EndpointMode            = t.EndpointMode,
                TokenId                 = t.TokenId,
                TokenState              = t.TokenState,
                NetworkTokenNumber      = t.NetworkTokenNumber,
                CardType                = t.CardType,
                PaymentAccountReference = t.PaymentAccountReference,
                RequestCardSuffix       = t.RequestCardSuffix,
                ExpMonth                = t.ExpMonth,
                ExpYear                 = t.ExpYear,
                ResponseTransactionJson = t.ResponseTransactionJson
            };
        }
        catch (Exception ex)
        {
            DbErrorHandler.Log(nameof(GetByIdAsync), ex);
            return new TokenizeTransactionDto
            {
                Error = new ErrorObject { Message = ex.Message }
            };
        }
    }
}
