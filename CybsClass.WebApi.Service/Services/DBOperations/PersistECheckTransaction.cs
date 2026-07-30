using Microsoft.EntityFrameworkCore;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.EntityModels;
using System.Text.Json.Nodes;

namespace CybsClass.WebApi.Service.Services.DBOperations
{
    // Mirrors PersistApplePayTransaction: an eCheck debit is a first-time transaction just like
    // Checkout, so it creates its own B2cCustomer + Order (+ OrderDetails from the cart) chain,
    // then writes an ECheckTransaction row FK'd to that Order. When the request asked for a TMS
    // token and CyberSource returned one, an ECheckPaymentInstrument row is written too, so the
    // token-debit page has something to offer.
    //
    // Values are read straight off the response JsonNode rather than through a typed response
    // model: an eCheck response is a much smaller shape than an authorization (no processor
    // block, no AVS, no card summary) and the typed AuthTransResponseJson would leave most of
    // this null while adding a deserialization failure mode for no benefit.
    public static class PersistECheckTransaction
    {
        public static async Task<Dictionary<string, object>> InsertECheckTransaction(
            B2cCustomerDto b2cCustomerDto,
            JsonNode cybersourceResponseNode,
            string requestTransactionJson,
            string transactionType)
        {
            var dbResults = new Dictionary<string, object>();

            Console.WriteLine("Inserting eCheck transaction data ...");
            try
            {
                using CybsDbContext db = new();

                // Customer, order, order lines, the eCheck transaction row and any minted token
                // are one unit of work — a failure part way through must not leave an orphan
                // customer. The context enables retry-on-failure, whose execution strategy
                // refuses a user-initiated transaction unless the whole unit runs through it.
                var strategy = db.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    // The strategy may re-run this lambda, so every accumulator it writes to has
                    // to be cleared at the top rather than outside.
                    dbResults.Clear();
                    await using var tx = await db.Database.BeginTransactionAsync();

                    B2cCustomer c = new()
                    {
                        FirstName = b2cCustomerDto.FirstName!,
                        LastName = b2cCustomerDto.LastName!,
                        Phone = b2cCustomerDto.Phone!,
                        Email = b2cCustomerDto.Email!,
                        Address1 = b2cCustomerDto.Address1!,
                        City = b2cCustomerDto.City!,
                        Region = b2cCustomerDto.AdministrativeArea!,
                        PostalCode = b2cCustomerDto.PostalCode!,
                        Country = b2cCustomerDto.Country!
                    };

                    db.B2cCustomers.Add(c);
                    await db.SaveChangesAsync();
                    dbResults.Add("B2cCustomerId", c.B2cCustomerId);

                    Order o = new()
                    {
                        B2cCustomerId = c.B2cCustomerId,
                        OrderDate = DateTime.Now
                    };

                    db.Orders.Add(o);
                    await db.SaveChangesAsync();
                    dbResults.Add("OrderId", o.OrderId);

                    if (b2cCustomerDto.Cart is not null && b2cCustomerDto.Cart.Count > 0)
                    {
                        // OrderDetail's key is the composite (OrderId, ProductId), so the same
                        // product appearing twice in the cart must become one row with a quantity
                        // rather than two colliding rows.
                        var groupedCart = b2cCustomerDto.Cart
                            .GroupBy(p => p.ProductId)
                            .Select(g => new
                            {
                                ProductId = g.Key,
                                Quantity = (short)g.Count(),
                                UnitPrice = g.First().UnitPrice ?? 0m
                            });

                        foreach (var product in groupedCart)
                        {
                            db.OrderDetails.Add(new OrderDetail
                            {
                                OrderId = o.OrderId,
                                ProductId = product.ProductId,
                                Quantity = product.Quantity,
                                UnitPrice = product.UnitPrice
                            });
                        }

                        await db.SaveChangesAsync();
                    }

                    JsonNode? tokenInformation = cybersourceResponseNode["tokenInformation"];
                    string? customerTokenId = ReadId(tokenInformation, "customer");
                    string? paymentInstrumentId = ReadId(tokenInformation, "paymentInstrument");
                    string? instrumentIdentifierId = ReadId(tokenInformation, "instrumentIdentifier");
                    string? instrumentIdentifierState =
                        tokenInformation?["instrumentIdentifier"]?["state"]?.GetValue<string>();

                    string? maskedAccount = MaskAccount(b2cCustomerDto.BankAccountNumber);

                    var echeckDb = new ECheckTransaction
                    {
                        OrderId = o.OrderId,
                        Id = ReadString(cybersourceResponseNode, "id") ?? string.Empty,
                        ClientReferenceCode =
                            cybersourceResponseNode["clientReferenceInformation"]?["code"]?.GetValue<string>(),
                        Status = ReadString(cybersourceResponseNode, "status"),
                        ReconciliationId = ReadString(cybersourceResponseNode, "reconciliationId"),
                        SubmitTimeUtc = DateTime.TryParse(
                            ReadString(cybersourceResponseNode, "submitTimeUtc"),
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.AdjustToUniversal
                                | System.Globalization.DateTimeStyles.AssumeUniversal,
                            out var submitTime)
                            ? submitTime
                            : null,
                        TotalAmount = b2cCustomerDto.TotalAmount,
                        Currency = "USD",
                        RoutingNumber = b2cCustomerDto.RoutingNumber,
                        MaskedAccountNumber = maskedAccount,
                        AccountType = b2cCustomerDto.BankAccountType,
                        SecCode = b2cCustomerDto.SecCode,
                        CommerceIndicator = b2cCustomerDto.IsRecurring ? "recurring" : "internet",
                        IsRecurring = b2cCustomerDto.IsRecurring,
                        FirstRecurringPayment = b2cCustomerDto.IsRecurring
                            ? b2cCustomerDto.FirstRecurringPayment
                            : null,
                        TransactionType = transactionType,
                        CustomerTokenId = customerTokenId,
                        PaymentInstrumentId = paymentInstrumentId,
                        InstrumentIdentifierId = instrumentIdentifierId,
                        RequestTransactionJson = requestTransactionJson,
                        ResponseTransactionJson = cybersourceResponseNode.ToString(),
                        // Set explicitly — the column deliberately has no SQL DEFAULT so the
                        // SQL Server and SQLite builds cannot diverge. See the entity comment.
                        CreatedAt = DateTime.UtcNow
                    };

                    db.ECheckTransactions.Add(echeckDb);
                    await db.SaveChangesAsync();
                    dbResults.Add("ECheckTransactionId", echeckDb.ECheckTransactionId);

                    // Only worth a token row if CyberSource actually returned something the
                    // token-debit flow can send back as paymentInformation.customer.id.
                    if (!string.IsNullOrWhiteSpace(customerTokenId))
                    {
                        var instrument = new ECheckPaymentInstrument
                        {
                            B2cCustomerId = c.B2cCustomerId,
                            CustomerTokenId = customerTokenId,
                            PaymentInstrumentId = paymentInstrumentId,
                            InstrumentIdentifierId = instrumentIdentifierId,
                            InstrumentIdentifierState = instrumentIdentifierState,
                            RoutingNumber = b2cCustomerDto.RoutingNumber,
                            MaskedAccountNumber = maskedAccount,
                            AccountType = b2cCustomerDto.BankAccountType,
                            BankName = b2cCustomerDto.BankName,
                            DisplayLabel = BuildDisplayLabel(
                                b2cCustomerDto.BankAccountType, maskedAccount, b2cCustomerDto.RoutingNumber),
                            SourceTransactionId = echeckDb.Id,
                            ResponseTransactionJson = cybersourceResponseNode.ToString(),
                            CreatedAt = DateTime.UtcNow
                        };

                        db.ECheckPaymentInstruments.Add(instrument);
                        await db.SaveChangesAsync();
                        dbResults.Add("ECheckPaymentInstrumentId", instrument.ECheckPaymentInstrumentId);
                    }

                    await tx.CommitAsync();
                });

                return dbResults;
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(InsertECheckTransaction), ex);
                return new Dictionary<string, object> { [DbErrorHandler.ErrorKey] = ex.GetBaseException().Message };
            }
        }

        private static string? ReadString(JsonNode node, string property)
        {
            JsonNode? value = node[property];
            return value is null ? null : value.GetValue<string>();
        }

        private static string? ReadId(JsonNode? tokenInformation, string tokenType) =>
            tokenInformation?[tokenType]?["id"]?.GetValue<string>();

        /// <summary>
        /// Last four digits only — the full bank account number is never persisted.
        /// </summary>
        private static string? MaskAccount(string? accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber)) return null;

            return accountNumber.Length <= 4
                ? new string('*', accountNumber.Length)
                : new string('*', accountNumber.Length - 4) + accountNumber[^4..];
        }

        private static string BuildDisplayLabel(string? accountType, string? maskedAccount, string? routingNumber)
        {
            string type = accountType switch
            {
                "C" => "Checking",
                "S" => "Savings",
                "X" => "Corporate checking",
                _ => "Bank account"
            };

            return $"{type} {maskedAccount ?? "account"} - {routingNumber ?? "unknown routing"}";
        }
    }
}
