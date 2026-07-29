using Microsoft.EntityFrameworkCore;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.EntityModels;

namespace CybsClass.WebApi.Service.Services.DBOperations
{
    public static class DBCybsTokenServices
    {
        public static async Task<Dictionary<string, string>> UpdateCustomerInstIdAsync(int customerId, B2cCustomerDto b2cCustomerDto)
        {
            Dictionary<string, string> dbResult = new();

            try
            {
                using CybsDbContext db = new();
                var affected = await db.PaymentCardInfos
                    .Where(model => model.B2cCustomerId == customerId).ExecuteUpdateAsync(setters => setters
                        .SetProperty(m => m.CustomerInstrumentId, b2cCustomerDto.CustomerInstrumentId)
                        .SetProperty(m => m.MerchantCustomerId, b2cCustomerDto.MerchantCustomerID)
                    );
                dbResult.Add("Affected", affected.ToString());
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(UpdateCustomerInstIdAsync), ex);
                return new Dictionary<string, string> { [DbErrorHandler.ErrorKey] = ex.GetBaseException().Message };
            }

            return dbResult;
        }

        public static async Task<Dictionary<string, string>> UpdatePaymentCardInstId(int customerId, B2cCustomerDto b2cCustomerDto)
        {
            Dictionary<string, string> dbResult = new();

            try
            {
                using CybsDbContext db = new();
                var affected = await db.PaymentCardInfos
                    .Where(model => model.B2cCustomerId == customerId).ExecuteUpdateAsync(setters => setters
                        .SetProperty(m => m.InstrumentIdentifierId, b2cCustomerDto.InstrumentIdentifier)
                    );
                dbResult.Add("Affected", affected.ToString());
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(UpdatePaymentCardInstId), ex);
                return new Dictionary<string, string> { [DbErrorHandler.ErrorKey] = ex.GetBaseException().Message };
            }

            return dbResult;
        }

        public static async Task<Dictionary<string, string>> GetNetworkTokenCountById(int paymentCardId)
        {
            Dictionary<string, string> dbResults = new();

            try
            {
                using CybsDbContext db = new();
                var affected = await db.NetworkTokenInfos
                       .Where(nt => nt.PaymentCardId == paymentCardId)
                       .CountAsync();
                dbResults.Add("Network Token Count:", affected.ToString());
                return dbResults;
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(GetNetworkTokenCountById), ex);
                return new Dictionary<string, string> { [DbErrorHandler.ErrorKey] = ex.GetBaseException().Message };
            }
        }
    }
}
