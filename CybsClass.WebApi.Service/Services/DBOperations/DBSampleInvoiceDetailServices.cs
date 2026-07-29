using Microsoft.EntityFrameworkCore;
using CybsClass.EntityModels;

namespace CybsClass.WebApi.Service.Services.DBOperations
{
    public class DBSampleInvoiceDetailServices
    {
        public static Task<DbResult<List<SampleInvoiceDetail>>> GetAllSampleInvoiceDetails() =>
            DbErrorHandler.GuardAsync(nameof(GetAllSampleInvoiceDetails), async () =>
            {
                Console.WriteLine("[DBSampleInvoiceDetailServices] Fetching all sample invoice details.");
                using CybsDbContext db = new();
                return await db.SampleInvoiceDetails.ToListAsync();
            });

        public static Task<DbResult<SampleInvoiceDetail?>> GetSampleInvoiceDetailById(int id) =>
            DbErrorHandler.GuardAsync<SampleInvoiceDetail?>(nameof(GetSampleInvoiceDetailById), async () =>
            {
                Console.WriteLine($"[DBSampleInvoiceDetailServices] Fetching sample invoice detail with ID {id}.");
                using CybsDbContext db = new();
                return await db.SampleInvoiceDetails.AsNoTracking()
                    .FirstOrDefaultAsync(model => model.SampleInvoiceId == id);
            });

        public static Task<DbResult<int>> UpdateSampleInvoiceDetail(int id, SampleInvoiceDetail sampleInvoiceDetail) =>
            DbErrorHandler.GuardAsync(nameof(UpdateSampleInvoiceDetail), async () =>
            {
                Console.WriteLine($"[DBSampleInvoiceDetailServices] Updating sample invoice detail with ID {id}.");
                using CybsDbContext db = new();
                return await db.SampleInvoiceDetails
                    .Where(model => model.SampleInvoiceId == id)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(m => m.CustomerInformationName, sampleInvoiceDetail.CustomerInformationName)
                        .SetProperty(m => m.CustomerInformationEmail, sampleInvoiceDetail.CustomerInformationEmail)
                        .SetProperty(m => m.CustomerInformationMerchantCustomerId, sampleInvoiceDetail.CustomerInformationMerchantCustomerId)
                        .SetProperty(m => m.CustomerInformationCompanyName, sampleInvoiceDetail.CustomerInformationCompanyName)
                        .SetProperty(m => m.InvoiceInformationInvoiceNumber, sampleInvoiceDetail.InvoiceInformationInvoiceNumber)
                        .SetProperty(m => m.InvoiceInformationDueDate, sampleInvoiceDetail.InvoiceInformationDueDate)
                        .SetProperty(m => m.InvoiceInformationSendImmediately, sampleInvoiceDetail.InvoiceInformationSendImmediately)
                        .SetProperty(m => m.InvoiceInformationAllowPartialPayments, sampleInvoiceDetail.InvoiceInformationAllowPartialPayments)
                        .SetProperty(m => m.InvoiceInformationDeliveryMode, sampleInvoiceDetail.InvoiceInformationDeliveryMode)
                        .SetProperty(m => m.OrderInformationAmountDetailsTotalAmount, sampleInvoiceDetail.OrderInformationAmountDetailsTotalAmount)
                        .SetProperty(m => m.OrderInformationAmountDetailsCurrency, sampleInvoiceDetail.OrderInformationAmountDetailsCurrency)
                        .SetProperty(m => m.OrderInformationAmountDetailsDiscountAmount, sampleInvoiceDetail.OrderInformationAmountDetailsDiscountAmount)
                        .SetProperty(m => m.OrderInformationAmountDetailsDiscountPercent, sampleInvoiceDetail.OrderInformationAmountDetailsDiscountPercent)
                        .SetProperty(m => m.OrderInformationAmountDetailsSubAmount, sampleInvoiceDetail.OrderInformationAmountDetailsSubAmount)
                        .SetProperty(m => m.OrderInformationAmountDetailsMinimumPartialAmount, sampleInvoiceDetail.OrderInformationAmountDetailsMinimumPartialAmount)
                        .SetProperty(m => m.OrderInformationAmountDetailsTaxDetailsType, sampleInvoiceDetail.OrderInformationAmountDetailsTaxDetailsType)
                        .SetProperty(m => m.OrderInformationAmountDetailsTaxDetailsAmount, sampleInvoiceDetail.OrderInformationAmountDetailsTaxDetailsAmount)
                        .SetProperty(m => m.OrderInformationAmountDetailsTaxDetailsRate, sampleInvoiceDetail.OrderInformationAmountDetailsTaxDetailsRate)
                        .SetProperty(m => m.OrderInformationAmountDetailsFreightAmount, sampleInvoiceDetail.OrderInformationAmountDetailsFreightAmount)
                        .SetProperty(m => m.OrderInformationAmountDetailsFreightTaxable, sampleInvoiceDetail.OrderInformationAmountDetailsFreightTaxable)
                        .SetProperty(m => m.OrderInformationLineItemsProductSku, sampleInvoiceDetail.OrderInformationLineItemsProductSku)
                        .SetProperty(m => m.OrderInformationLineItemsQuantity, sampleInvoiceDetail.OrderInformationLineItemsQuantity)
                        .SetProperty(m => m.OrderInformationLineItemsUnitPrice, sampleInvoiceDetail.OrderInformationLineItemsUnitPrice)
                        .SetProperty(m => m.OrderInformationLineItemsDiscountAmount, sampleInvoiceDetail.OrderInformationLineItemsDiscountAmount)
                        .SetProperty(m => m.OrderInformationLineItemsDiscountRate, sampleInvoiceDetail.OrderInformationLineItemsDiscountRate)
                        .SetProperty(m => m.OrderInformationLineItemsTaxAmount, sampleInvoiceDetail.OrderInformationLineItemsTaxAmount)
                        .SetProperty(m => m.OrderInformationLineItemsTaxRate, sampleInvoiceDetail.OrderInformationLineItemsTaxRate)
                        .SetProperty(m => m.OrderInformationLineItemsTotalAmount, sampleInvoiceDetail.OrderInformationLineItemsTotalAmount));
            });

        public static Task<DbResult<SampleInvoiceDetail?>> CreateSampleInvoiceDetail(SampleInvoiceDetail sampleInvoiceDetail) =>
            DbErrorHandler.GuardAsync<SampleInvoiceDetail?>(nameof(CreateSampleInvoiceDetail), async () =>
            {
                Console.WriteLine("[DBSampleInvoiceDetailServices] Inserting new sample invoice detail.");
                using CybsDbContext db = new();
                db.SampleInvoiceDetails.Add(sampleInvoiceDetail);
                await db.SaveChangesAsync();
                Console.WriteLine($"[DBSampleInvoiceDetailServices] Sample invoice detail created with ID {sampleInvoiceDetail.SampleInvoiceId}.");
                return sampleInvoiceDetail;
            });

        public static Task<DbResult<int>> DeleteSampleInvoiceDetail(int id) =>
            DbErrorHandler.GuardAsync(nameof(DeleteSampleInvoiceDetail), async () =>
            {
                Console.WriteLine($"[DBSampleInvoiceDetailServices] Deleting sample invoice detail with ID {id}.");
                using CybsDbContext db = new();
                return await db.SampleInvoiceDetails
                    .Where(model => model.SampleInvoiceId == id)
                    .ExecuteDeleteAsync();
            });
    }
}
