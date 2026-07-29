using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using CybsClass.Cybersource.Models.BaseData;
using CybsClass.Cybersource.Models.Json;
using CybsClass.EntityModels;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CybsClass.WebApi.Service.Services.DBOperations
{
    public static class PersistInvoiceResponse
    {
        public static async Task<Dictionary<string, string>> InvoiceDBOps(JsonObject jsonObject)
        {
            Dictionary<string, string> dbResults = new();

            try
            {
                Console.WriteLine("Inserting invoice response data ...");

                JsonDocument document = JsonDocument.Parse(jsonObject.ToString());

                if (document.RootElement.TryGetProperty("InvoiceInformation", out JsonElement InvoiceInformation))
                {

                    string InvoiceNumber = InvoiceInformation.GetProperty("InvoiceNumber").GetString()!;


                    if (InvoiceNumber is not null)
                    {
                        Console.WriteLine("Updating InvoiceResponse table ...");
                        Console.WriteLine($"Invoice Number: {InvoiceNumber}");

                        using CybsDbContext db = new();

                        InvoiceResponse i = new InvoiceResponse();
                        i.InvoiceNumber = InvoiceNumber;
                        i.TransactionJson = jsonObject.ToString();

                        db.InvoiceResponses.Add(i);

                        int affected = await db.SaveChangesAsync();

                        // Read the identity value only after the save — before it, it is always 0.
                        dbResults.Add("Invoice Response ID:", i.InvoiceResponseId.ToString());
                        dbResults.Add("Affected: InvoiceResponse Added", affected.ToString());

                    }
                }
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(InvoiceDBOps), ex);
                return new Dictionary<string, string> { [DbErrorHandler.ErrorKey] = ex.GetBaseException().Message };
            }
            return dbResults;
        }
    }
}





