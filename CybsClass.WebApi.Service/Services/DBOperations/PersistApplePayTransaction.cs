using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Models.Json;
using CybsClass.EntityModels;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CybsClass.WebApi.Service.Services.DBOperations
{
    // Mirrors PersistCustomerData.InsertCustomers — an Apple Pay authorization is a first-time
    // AUTH just like Checkout, so it creates its own B2cCustomer + Order (+ OrderDetails from the
    // cart) chain, then writes an ApplePayTransaction row FK'd to that Order (instead of an
    // AuthTransResponse row). Deliberately NOT the JsonNode/"FollowOnTransaction" style persist,
    // which assumes an Order already exists.
    public static class PersistApplePayTransaction
    {
        public static async Task<Dictionary<string, object>> InsertApplePayTransaction(
            B2cCustomerDto b2cCustomerDto,
            JsonNode cybersourceResponseNode,
            string decryptedTokenJson,
            string requestTransactionJson)
        {
            var dbResults = new Dictionary<string, object>();

            Console.WriteLine("Inserting Apple Pay transaction data ...");
            try
            {
                AuthTransResponseJson responseJson = JsonSerializer.Deserialize<AuthTransResponseJson>(cybersourceResponseNode.ToString())!;

                using CybsDbContext db = new();

                // Customer, order, order lines and the Apple Pay transaction row are one unit
                // of work — a failure part way through must not leave an orphan customer. The
                // context enables retry-on-failure, whose execution strategy refuses a
                // user-initiated transaction unless the whole unit runs through the strategy.
                var strategy = db.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                dbResults.Clear();
                await using var tx = await db.Database.BeginTransactionAsync();

                B2cCustomer c = new();
                c.FirstName = b2cCustomerDto.FirstName!;
                c.LastName = b2cCustomerDto.LastName!;
                c.Phone = b2cCustomerDto.Phone!;
                c.Email = b2cCustomerDto.Email!;
                c.Address1 = b2cCustomerDto.Address1!;
                c.City = b2cCustomerDto.City!;
                c.Region = b2cCustomerDto.AdministrativeArea!;
                c.PostalCode = b2cCustomerDto.PostalCode!;
                c.Country = b2cCustomerDto.Country!;

                db.B2cCustomers.Add(c);
                await db.SaveChangesAsync();
                dbResults.Add("B2cCustomerId", c.B2cCustomerId);

                Order o = new();
                o.B2cCustomerId = c.B2cCustomerId;
                o.OrderDate = DateTime.Now;

                db.Orders.Add(o);
                await db.SaveChangesAsync();
                dbResults.Add("OrderId", o.OrderId);

                if (b2cCustomerDto.Cart is not null)
                {
                    foreach (var product in b2cCustomerDto.Cart)
                    {
                        var orderDetails = new OrderDetail
                        {
                            OrderId = o.OrderId,
                            ProductId = product.ProductId,
                            Quantity = 1,
                            UnitPrice = product.UnitPrice ?? 0m
                        };
                        db.OrderDetails.Add(orderDetails);
                    }

                    await db.SaveChangesAsync();
                }

                var applePayDb = new ApplePayTransaction
                {
                    OrderId = o.OrderId,
                    Id = responseJson.Id ?? string.Empty,
                    ClientReferenceCode = responseJson.ClientReferenceInformation?.Code,
                    Status = responseJson.Status,
                    SubmitTimeUtc = responseJson.SubmitTimeUtc,
                    AuthorizedAmount = decimal.TryParse(responseJson.OrderInformation?.AmountDetails?.AuthorizedAmount, out var authAmt)
                        ? authAmt
                        : 0.00m,
                    Currency = responseJson.OrderInformation?.AmountDetails?.Currency ?? b2cCustomerDto.Currency,
                    CardNetwork = responseJson.PaymentInformation?.TokenizedCard?.BrandName ?? b2cCustomerDto.ApplePayCardNetwork,
                    MaskedPan = responseJson.PaymentInformation?.TokenizedCard?.MaskedPan,
                    PaymentSolution = "001",
                    TransactionType = "1",
                    DecryptedTokenJson = decryptedTokenJson,
                    RequestTransactionJson = requestTransactionJson,
                    ResponseTransactionJson = cybersourceResponseNode.ToString()
                };

                db.ApplePayTransactions.Add(applePayDb);
                await db.SaveChangesAsync();
                dbResults.Add("ApplePayTransactionsId", applePayDb.ApplePayTransactionsId);

                await tx.CommitAsync();
                });

                return dbResults;
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(InsertApplePayTransaction), ex);
                return new Dictionary<string, object> { [DbErrorHandler.ErrorKey] = ex.GetBaseException().Message };
            }
        }
    }
}
