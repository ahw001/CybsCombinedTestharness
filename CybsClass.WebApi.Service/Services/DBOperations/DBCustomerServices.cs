using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.EntityModels;

namespace CybsClass.WebApi.Service.Services.DBOperations;

public static class DBCustomerServices
{
    public static async Task<Dictionary<string, string>> InsertB2CCustomerAsync(B2cCustomerDto b2cCustomerDto)
    {
        Dictionary<string, string> dbResults = new();

        B2cCustomer? b2cCustomer = new()
        {
            FirstName = b2cCustomerDto.FirstName ?? "null",
            LastName = b2cCustomerDto.LastName ?? "null",
            Email = b2cCustomerDto.Email ?? "null",
            Address1 = b2cCustomerDto.Address1 ?? "null",
            Address2 = b2cCustomerDto.Address2 ?? "null",
            City = b2cCustomerDto.City ?? "null",
            Region = b2cCustomerDto.AdministrativeArea ?? "null",
            PostalCode = b2cCustomerDto.PostalCode ?? "null",
            Country = b2cCustomerDto.Country ?? "null",
            Phone = b2cCustomerDto.Phone ?? "null"
        };

        try
        {
            using CybsDbContext db = new();

            // The customer, its order and every order line are one unit of work — a failure
            // part way through must not leave an orphan customer or an order with no lines.
            // The context enables retry-on-failure, whose execution strategy refuses a
            // user-initiated transaction unless the whole unit is run through the strategy,
            // so that the entire block (not just one statement) is what gets retried.
            var strategy = db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                dbResults.Clear();
                await using var tx = await db.Database.BeginTransactionAsync();

                EntityEntry<B2cCustomer> entity = db.B2cCustomers.Add(b2cCustomer);
                Console.WriteLine($"B2cCustomer State: {entity.State}, B2cCustomerId: {b2cCustomer.B2cCustomerId}");

                await db.SaveChangesAsync();
                Console.WriteLine($"B2cCustomer State: {entity.State}, B2cCustomerId: {b2cCustomer.B2cCustomerId}");
                dbResults.Add("B2cCustomerId", b2cCustomer.B2cCustomerId.ToString());

                Order o = new();

                o.B2cCustomerId = b2cCustomer.B2cCustomerId;
                o.OrderDate = DateTime.Now;

                db.Orders.Add(o);
                await db.SaveChangesAsync();

                dbResults.Add("OrderId", o.OrderId.ToString());

                if (b2cCustomerDto is not null && b2cCustomerDto.Cart is not null)
                {
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
                        var orderDetails = new OrderDetail();
                        orderDetails.OrderId = o.OrderId;
                        orderDetails.ProductId = product.ProductId;
                        orderDetails.Quantity = product.Quantity;
                        orderDetails.UnitPrice = product.UnitPrice;

                        db.OrderDetails.Add(orderDetails);
                    }

                    await db.SaveChangesAsync();
                }
                else
                {
                    Console.WriteLine("Nothing found in cart");
                }

                await tx.CommitAsync();
            });

            return dbResults;
        }
        catch (Exception ex)
        {
            DbErrorHandler.Log(nameof(InsertB2CCustomerAsync), ex);
            return new Dictionary<string, string> { [DbErrorHandler.ErrorKey] = ex.GetBaseException().Message };
        }
    }

    public static Task<DbResult<int>> GetCustomerCountAsync() =>
        DbErrorHandler.GuardAsync(nameof(GetCustomerCountAsync), async () =>
        {
            using CybsDbContext db = new();
            return await db.B2cCustomers.CountAsync();
        });

    public static Task<DbResult<List<B2cCustomer>>> GetB2CCustomers() =>
        DbErrorHandler.GuardAsync(nameof(GetB2CCustomers), async () =>
        {
            using CybsDbContext db = new();
            return await db.B2cCustomers.ToListAsync();
        });

    public static Task<DbResult<List<PaymentCardInfo>>> GetB2CCustomerPaymentCards(int b2ccustomerid) =>
        DbErrorHandler.GuardAsync(nameof(GetB2CCustomerPaymentCards), async () =>
        {
            using CybsDbContext db = new();
            return await db.PaymentCardInfos.Where(p => p.B2cCustomerId == b2ccustomerid).ToListAsync();
        });

    public static Task<DbResult<List<B2cCustomer>>> GetB2cCustomerPagedAsync(int pageIndex, int pageSize) =>
        DbErrorHandler.GuardAsync(nameof(GetB2cCustomerPagedAsync), async () =>
        {
            using CybsDbContext db = new();
            return await db.B2cCustomers
                .Distinct()
                .OrderBy(c => c.B2cCustomerId)
                .Skip(pageIndex)
                .Take(pageSize)
                .ToListAsync();
        });

    public static Task<DbResult<B2cCustomer?>> GetB2cCustomerByIdAsync(int id) =>
        DbErrorHandler.GuardAsync<B2cCustomer?>(nameof(GetB2cCustomerByIdAsync), async () =>
        {
            using CybsDbContext db = new();
            return await db.B2cCustomers.AsNoTracking()
                .FirstOrDefaultAsync(model => model.B2cCustomerId == id);
        });

    public static Task<DbResult<int>> UpdateB2cCustomer(int id, B2cCustomer b2cCustomer) =>
        DbErrorHandler.GuardAsync(nameof(UpdateB2cCustomer), async () =>
        {
            using CybsDbContext db = new();
            return await db.B2cCustomers
                .Where(model => model.B2cCustomerId == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.FirstName, b2cCustomer.FirstName)
                    .SetProperty(m => m.LastName, b2cCustomer.LastName)
                    .SetProperty(m => m.Email, b2cCustomer.Email)
                    .SetProperty(m => m.Address1, b2cCustomer.Address1)
                    .SetProperty(m => m.Address2, b2cCustomer.Address2)
                    .SetProperty(m => m.City, b2cCustomer.City)
                    .SetProperty(m => m.Region, b2cCustomer.Region)
                    .SetProperty(m => m.PostalCode, b2cCustomer.PostalCode)
                    .SetProperty(m => m.Country, b2cCustomer.Country)
                    .SetProperty(m => m.Phone, b2cCustomer.Phone));
        });

    public static Task<DbResult<B2cCustomer?>> CreateB2cCustomerSimple(B2cCustomer b2cCustomer) =>
        DbErrorHandler.GuardAsync<B2cCustomer?>(nameof(CreateB2cCustomerSimple), async () =>
        {
            using CybsDbContext db = new();
            db.B2cCustomers.Add(b2cCustomer);
            await db.SaveChangesAsync();
            return b2cCustomer;
        });

    public static Task<DbResult<int>> DeleteB2cCustomer(int id) =>
        DbErrorHandler.GuardAsync(nameof(DeleteB2cCustomer), async () =>
        {
            using CybsDbContext db = new();
            return await db.B2cCustomers
                .Where(model => model.B2cCustomerId == id)
                .ExecuteDeleteAsync();
        });
}
