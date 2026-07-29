using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Models.Json;
using CybsClass.EntityModels;
using System.Text.Json;
using System.Text.Json.Nodes;


namespace CybsClass.WebApi.Service.Services.DBOperations
{
    public static class PersistStandAloneCredit
    {
        public static async Task<Dictionary<string, object>> InsertStandAloneCredit(B2cCustomerDto b2cCustomerDto, JsonNode creditTransNode)
        {
            Dictionary<string, object> dbResults = new();

            Console.WriteLine("Inserting stand alone credit data ...");

            try
            { 
                StandAloneCreditTransResponseJson creditTransResponseJson = new StandAloneCreditTransResponseJson();

                creditTransResponseJson = JsonSerializer.Deserialize<StandAloneCreditTransResponseJson>(creditTransNode.ToString())!;

                using CybsDbContext db = new();

                StandAloneCredit s = new StandAloneCredit();

                s.AccountNumber = b2cCustomerDto.AccountNumber!;
                s.ExpMonth = b2cCustomerDto.ExpMonth!;
                s.ExpYear = b2cCustomerDto.ExpYear!;
                s.Cvv = b2cCustomerDto.Cvv!;
                s.ResponseTransactionJson = creditTransNode.ToString();

                EntityEntry<StandAloneCredit> entity = db.StandAloneCredits.Add(s);
                Console.WriteLine($"StandAloneCredit State: {entity.State}, StandAloneCreditId: {s.StandAloneCreditCardId}");

                int affected0 = await db.SaveChangesAsync();
                Console.WriteLine($"StandAloneCredit State: {entity.State}, StandAloneCreditId: {s.StandAloneCreditCardId}");
                dbResults.Add("StandAloneCreditId", s.StandAloneCreditCardId);

                return dbResults;
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(InsertStandAloneCredit), ex);
                return new Dictionary<string, object> { [DbErrorHandler.ErrorKey] = ex.GetBaseException().Message };
            }
        }
    }
}
