using Microsoft.EntityFrameworkCore;
using CybsClass.EntityModels;
using CybsClass.Cybersource.Models.DTOs;

namespace CybsClass.WebApi.Service.Services.DBOperations
{
    public static class DBNetworkTokenTestCardServices
    {
        public static async Task<List<NetworkTokenTestCardDto>> GetAllAsync(CybsDbContext context)
        {
            try
            {
                Console.WriteLine("[DBNetworkTokenTestCardServices] Fetching network token test cards ...");
                var entities = await context.NetworkTokenTestCard.ToListAsync();

                return entities.Select(e => new NetworkTokenTestCardDto
                {
                    NetworkTokenTestCardId = e.NetworkTokenTestCardId,
                    CardBrand = e.CardBrand,
                    AccountNumber = e.AccountNumber,
                    ExpMonth = e.ExpMonth,
                    ExpYear = e.ExpYear,
                    Cvv = e.Cvv,
                    TestCategory = e.TestCategory,
                    ScenarioOutcome = e.ScenarioOutcome,
                    IsSuccess = e.IsSuccess,
                    DisplayLabel = e.DisplayLabel
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DBNetworkTokenTestCardServices] Error fetching network token test cards: {ex.Message}");
                return new List<NetworkTokenTestCardDto>
                {
                    new() { Error = new() { Message = ex.Message } }
                };
            }
        }
    }
}
