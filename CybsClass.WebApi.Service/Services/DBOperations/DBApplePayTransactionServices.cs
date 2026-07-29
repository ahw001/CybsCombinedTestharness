using Microsoft.EntityFrameworkCore;
using CybsClass.EntityModels;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Models.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace CybsClass.WebApi.Service.Services.DBOperations
{
    public class DBApplePayTransactionServices
    {
        public static Task<DbResult<int>> GetApplePayTransactionCountAsync() =>
            DbErrorHandler.GuardAsync(nameof(GetApplePayTransactionCountAsync), async () =>
            {
                using CybsDbContext db = new();
                return await db.ApplePayTransactions.CountAsync();
            });

        public static async Task<List<ApplePayTransactionDto>> GetApplePayTransactions()
        {
            try
            {
                using CybsDbContext db = new();
                var applePayTransactions = await db.ApplePayTransactions.ToListAsync();
                List<ApplePayTransactionDto> applePayTransactionDtos = ApplePayTransactionMapper.Map(applePayTransactions)!;
                return applePayTransactionDtos;
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(GetApplePayTransactions), ex);
                var applePayTransactionDtos = new List<ApplePayTransactionDto>();
                ApplePayTransactionDto applePayTransactionDto = new ApplePayTransactionDto();
                applePayTransactionDto.Error = new CybsClass.Cybersource.Models.BaseData.ErrorObject { Error = ex.ToString() };
                applePayTransactionDtos.Add(applePayTransactionDto);
                return applePayTransactionDtos;
            }
        }

        public static async Task<ApplePayTransactionDto?> GetApplePayTransactionByUsingId([FromRoute] int id)
        {
            try
            {
                using CybsDbContext db = new();
                var applePayTransaction = await db.ApplePayTransactions.AsNoTracking()
                    .FirstOrDefaultAsync(model => model.ApplePayTransactionsId == id);

                if (applePayTransaction == null)
                {
                    return null;
                }
                return ApplePayTransactionMapper.Map(applePayTransaction);
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(GetApplePayTransactionByUsingId), ex);
                var applePayTransactionDto = new ApplePayTransactionDto();
                applePayTransactionDto.Error = new CybsClass.Cybersource.Models.BaseData.ErrorObject { Error = ex.ToString() };
                return applePayTransactionDto;
            }
        }
    }
}
