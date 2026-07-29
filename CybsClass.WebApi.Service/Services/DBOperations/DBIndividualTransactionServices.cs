using Microsoft.EntityFrameworkCore;
using CybsClass.EntityModels;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Models.Mappers;

namespace CybsClass.WebApi.Service.Services.DBOperations
{
    public class DBIndividualTransactionServices
    {
        public static Task<DbResult<int>> GetIndividualTransactionCountAsync() =>
            DbErrorHandler.GuardAsync(nameof(GetIndividualTransactionCountAsync), async () =>
            {
                using CybsDbContext db = new();
                return await db.IndividualTransactions.CountAsync();
            });
        public static async Task<List<IndividualTransactionDto>> GetIndividualTransactions()
        {
            try
            {
                Console.WriteLine("Geting full list of Individual Transactions ...");
                using CybsDbContext db = new();
                var individualTransactions = await db.IndividualTransactions.ToListAsync();
                List<IndividualTransactionDto> paymentCardDtos = IndividualTransactionMapper.Map(individualTransactions)!;
                return paymentCardDtos;
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(GetIndividualTransactions), ex);
                var individualTransactions = new List<IndividualTransactionDto>();
                IndividualTransactionDto paymentCardDto = new IndividualTransactionDto();
                paymentCardDto.Error = DbErrorHandler.BuildError(ex, nameof(GetIndividualTransactions));
                individualTransactions.Add(paymentCardDto);
                return individualTransactions;
            }

        }

        public static async Task<IndividualTransactionDto?> GetIndividualTransactionByUsingId(int transactionid)
        {
            try
            {
                Console.WriteLine($"Geting Individual Transaction for: {transactionid}");
                using CybsDbContext db = new();
                Task<IndividualTransaction?> task = db.IndividualTransactions.AsNoTracking()
                            .FirstOrDefaultAsync(model => model.TransactionId == transactionid);

                var individualTransaction = await task;
                if (individualTransaction == null)
                {
                    return null;
                }
                return IndividualTransactionMapper.Map(individualTransaction);
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(GetIndividualTransactionByUsingId), ex);
                var paymentCardDto = new IndividualTransactionDto();
                paymentCardDto.Error = DbErrorHandler.BuildError(ex, nameof(GetIndividualTransactionByUsingId));
                return paymentCardDto;
            }
        }
        public static Task<DbResult<int>> CreateIndividualTransaction(IndividualTransactionDto paymentCardDto) =>
            DbErrorHandler.GuardAsync(nameof(CreateIndividualTransaction), async () =>
            {
                IndividualTransaction individualTransaction = IndividualTransactionMapper.Map(paymentCardDto)!;
                using CybsDbContext db = new();
                db.IndividualTransactions.Add(individualTransaction);
                return await db.SaveChangesAsync();
            });
    }
}
