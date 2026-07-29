using Microsoft.EntityFrameworkCore;
using CybsClass.EntityModels;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Models.Mappers;

namespace CybsClass.WebApi.Service.Services.DBOperations
{
    public class DBPaymentCardServices
    {
        public static Task<DbResult<int>> GetPaymentCardCountAsync() =>
            DbErrorHandler.GuardAsync(nameof(GetPaymentCardCountAsync), async () =>
            {
                using CybsDbContext db = new();
                return await db.PaymentCardInfos.CountAsync();
            });

        public static Task<List<PaymentCardDto>> GetPaymentCardInfos() =>
            DbErrorHandler.GuardDtoAsync(
                nameof(GetPaymentCardInfos),
                async () =>
                {
                    Console.WriteLine("Geting full list of Payment Cards ...");
                    using CybsDbContext db = new();
                    var paymentCardInfos = await db.PaymentCardInfos.ToListAsync();
                    return PaymentCardMapper.Map(paymentCardInfos)!;
                },
                // PaymentCardDto.Error is a string on the wire and the client deserializes it
                // as one — carry the ErrorObject through as compact JSON rather than changing
                // the property's type. See the DTO error-shape note in DbErrorHandler.
                err => new List<PaymentCardDto>
                {
                    new PaymentCardDto { Error = DbErrorHandler.ToErrorString(err) }
                });

        public static async Task<PaymentCardDto?> GetPaymentCardInfoByUsingId(int paymentcardid)
        {
            try
            {
                Console.WriteLine($"Geting Payment Card for: {paymentcardid}");
                using CybsDbContext db = new();
                var paymentCardInfo = await db.PaymentCardInfos.AsNoTracking()
                            .FirstOrDefaultAsync(model => model.PaymentCardId == paymentcardid);

                if (paymentCardInfo == null)
                {
                    return null;
                }
                return PaymentCardMapper.Map(paymentCardInfo);
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(GetPaymentCardInfoByUsingId), ex);
                return new PaymentCardDto { Error = DbErrorHandler.BuildErrorString(ex, nameof(GetPaymentCardInfoByUsingId)) };
            }
        }

        public static Task<DbResult<PaymentCardInfo?>> GetPaymentCardInfoEntityByIdAsync(int paymentcardid) =>
            DbErrorHandler.GuardAsync<PaymentCardInfo?>(nameof(GetPaymentCardInfoEntityByIdAsync), async () =>
            {
                using CybsDbContext db = new();
                return await db.PaymentCardInfos.AsNoTracking()
                    .FirstOrDefaultAsync(model => model.PaymentCardId == paymentcardid);
            });

        public static async Task<Dictionary<string, string>> CreatePaymentCardInfo(PaymentCardDto paymentCardDto)
        {
            Dictionary<string, string> dbResults = new();

            try
            {
                PaymentCardInfo paymentCardInfo = PaymentCardMapper.Map(paymentCardDto)!;
                using CybsDbContext db = new();
                db.PaymentCardInfos.Add(paymentCardInfo);
                var affected = await db.SaveChangesAsync();
                dbResults.Add("Affected", affected.ToString());
                dbResults.Add("PaymentCardId", paymentCardInfo.PaymentCardId.ToString());
                return dbResults;
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(CreatePaymentCardInfo), ex);
                return new Dictionary<string, string> { [DbErrorHandler.ErrorKey] = ex.GetBaseException().Message };
            }
        }

        public static Task<DbResult<List<PaymentCardInfo>>> GetAllPaymentCardInfoEntities() =>
            DbErrorHandler.GuardAsync(nameof(GetAllPaymentCardInfoEntities), async () =>
            {
                Console.WriteLine("Getting full list of Payment Card entities ...");
                using CybsDbContext db = new();
                return await db.PaymentCardInfos.ToListAsync();
            });

        public static Task<DbResult<int>> DeletePaymentCardInfo(int id) =>
            DbErrorHandler.GuardAsync(nameof(DeletePaymentCardInfo), async () =>
            {
                Console.WriteLine($"Deleting Payment Card with ID {id} ...");
                using CybsDbContext db = new();
                return await db.PaymentCardInfos
                    .Where(model => model.PaymentCardId == id)
                    .ExecuteDeleteAsync();
            });

        public static Task<DbResult<int>> UpdatePaymentCardInfo(int id, PaymentCardDto paymentCardDto) =>
            DbErrorHandler.GuardAsync(nameof(UpdatePaymentCardInfo), async () =>
            {
                PaymentCardInfo paymentCardInfo = PaymentCardMapper.Map(paymentCardDto)!;
                using CybsDbContext db = new();
                return await db.PaymentCardInfos
                .Where(model => model.PaymentCardId == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.B2cCustomerId, paymentCardInfo.B2cCustomerId)
                    .SetProperty(m => m.AccountNumber, paymentCardInfo.AccountNumber)
                    .SetProperty(m => m.TokenValue, paymentCardInfo.TokenValue)
                    .SetProperty(m => m.ExpMonth, paymentCardInfo.ExpMonth)
                    .SetProperty(m => m.ExpYear, paymentCardInfo.ExpYear)
                    .SetProperty(m => m.Cvv, paymentCardInfo.Cvv)
                    .SetProperty(m => m.PaymentAccountReferenceNumber, paymentCardInfo.PaymentAccountReferenceNumber)
                    .SetProperty(m => m.TokenizedCardType, paymentCardInfo.TokenizedCardType)
                    .SetProperty(m => m.InstrumentidentifierNew, paymentCardInfo.InstrumentidentifierNew)
                    .SetProperty(m => m.InstrumentIdentifierId, paymentCardInfo.InstrumentIdentifierId)
                    .SetProperty(m => m.InstrumentIdentifierState, paymentCardInfo.InstrumentIdentifierState)
                    .SetProperty(m => m.PaymentInstrumentId, paymentCardInfo.PaymentInstrumentId)
                    .SetProperty(m => m.ResponseTransactionJson, paymentCardInfo.ResponseTransactionJson)
                    );
            });
    }
}
