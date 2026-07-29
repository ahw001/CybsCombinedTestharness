using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using CybsClass.Cybersource.Models.BaseData;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Models.Json;
using CybsClass.EntityModels;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CybsClass.WebApi.Service.Services.DBOperations
{
    public static class PersistUnifiedAuth
    {
        public static async Task<Dictionary<string, string>> UnifiedAuthDBOps(CtxPaymentDto ctxPaymentDto, JsonObject jsonObject)
        {
            Dictionary<string, string> dbResults = new();

            try
            {
                Console.WriteLine("Inserting unified auth payment card data ...");

                FollowOnTransJson followOnTransJson = JsonSerializer.Deserialize<FollowOnTransJson>(jsonObject)!;

                using CybsDbContext db = new();

                // Payment card and auth response are one unit of work. The context enables
                // retry-on-failure, whose execution strategy refuses a user-initiated
                // transaction unless the whole unit runs through the strategy.
                var strategy = db.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                dbResults.Clear();
                await using var tx = await db.Database.BeginTransactionAsync();

                var paymentCardInfo = new PaymentCardInfo();

                paymentCardInfo.B2cCustomerId = Convert.ToInt32(ctxPaymentDto.B2cCustomerId);
                paymentCardInfo.InstrumentIdentifierId = followOnTransJson.TokenInformation?.InstrumentIdentifier?.Id!;
                paymentCardInfo.InstrumentIdentifierState = followOnTransJson.TokenInformation?.InstrumentIdentifier?.State!;

                EntityEntry<PaymentCardInfo> entity = db.PaymentCardInfos.Add(paymentCardInfo);
                Console.WriteLine($"PaymentCard State: {entity.State}, PaymentCardId: {paymentCardInfo.PaymentCardId}");

                int affected0 = await db.SaveChangesAsync();
                Console.WriteLine($"PaymentCard State: {entity.State}, PaymentCardId: {paymentCardInfo.PaymentCardId}");
                dbResults.Add("PaymentCardId", paymentCardInfo.PaymentCardId.ToString());

                int? payCardInfo = paymentCardInfo!.PaymentCardId;

                Console.WriteLine("Inserting auth transaction data ...");

                AuthTransResponse authDb = new AuthTransResponse();

                authDb.Status = followOnTransJson.Status;
                authDb.SubmitTimeUtc = followOnTransJson.SubmitTimeUtc!;
                authDb.ReconciliationId = followOnTransJson.ReconciliationId!;

                if (followOnTransJson.OrderInformation is not null && followOnTransJson.OrderInformation?.AmountDetails is not null)
                {
                    authDb.AuthorizedAmount = Convert.ToDecimal(followOnTransJson.OrderInformation?.AmountDetails?.AuthorizedAmount);
                    Console.WriteLine($"authTransResponseJson.OrderInformation.AuthAmount is NOT null: {authDb.AuthorizedAmount}");
                }
                else
                {
                    Console.WriteLine("authTransResponseJson.OrderInformation.AuthAmount is NULL");
                    authDb.AuthorizedAmount = 0.00M;
                }

                authDb.Links = followOnTransJson.Links?.ToString() ?? "0";

                authDb.Id = followOnTransJson.Id ?? "0";
                authDb.OrderId = Convert.ToInt32(ctxPaymentDto.OrderId);
                authDb.ProcInfoApprovalCode = followOnTransJson.ProcessorInformation?.ApprovalCode ?? "0";
                authDb.ProcInfoNetworkTransactionId = followOnTransJson.ProcessorInformation?.NetworkTransactionId ?? "0";
                authDb.ProcInfoResponseCode = followOnTransJson.ProcessorInformation?.ResponseCode ?? "0";
                authDb.ProcInfoTransactionId = followOnTransJson.ProcessorInformation?.TransactionId ?? "0";

                authDb.TokenInformationInstId = followOnTransJson.TokenInformation?.InstrumentIdentifier?.Id ?? "0";
                authDb.TokenInformationInstIdNew = Convert.ToString(followOnTransJson.TokenInformation?.InstrumentIdentifierNew) ?? "0";

                EntityEntry<AuthTransResponse> authEntity = db.AuthTransResponses.Add(authDb);
                Console.WriteLine($"Auth Trans State: {authEntity.State}, ID: {authDb.AuthTransResponsesId}");

                await db.SaveChangesAsync();
                Console.WriteLine($"Auth Trans State: {authEntity.State}, ID: {authDb.AuthTransResponsesId}");

                await tx.CommitAsync();
                });
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(UnifiedAuthDBOps), ex);
                return new Dictionary<string, string> { [DbErrorHandler.ErrorKey] = ex.GetBaseException().Message };
            }
            return dbResults;
        }
    }
}






