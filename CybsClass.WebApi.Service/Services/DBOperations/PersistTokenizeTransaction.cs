using CybsClass.EntityModels;
using System.Text.Json.Nodes;

namespace CybsClass.WebApi.Service.Services.DBOperations;

public static class PersistTokenizeTransaction
{
    /// <summary>
    /// Inserts one row into dbo.TokenizeTransaction for a successful network-token response.
    /// Returns the new primary key, or null if the insert fails (failures do not propagate —
    /// the caller's HTTP response must not be interrupted by a DB error here).
    /// </summary>
    public static async Task<int?> InsertAsync(
        string  endpointMode,
        string? requestSuffix,
        string? expMonth,
        string? expYear,
        JsonNode response)
    {
        try
        {
            Console.WriteLine($"[PersistTokenizeTransaction] Inserting for mode={endpointMode}");

            var entity = new TokenizeTransaction
            {
                EndpointMode            = endpointMode,
                RequestCardSuffix       = requestSuffix,
                ExpMonth                = expMonth,
                ExpYear                 = expYear,
                TokenId                 = response["id"]?.ToString(),
                TokenState              = response["state"]?.ToString(),
                NetworkTokenNumber      = response["number"]?.ToString(),
                CardType                = response["type"]?.ToString(),
                PaymentAccountReference = response["paymentAccountReference"]?.ToString(),
                ResponseTransactionJson = response.ToJsonString()
            };

            using CybsDbContext db = new();
            db.TokenizeTransactions.Add(entity);
            await db.SaveChangesAsync();

            Console.WriteLine($"[PersistTokenizeTransaction] Inserted TokenizeTransactionId={entity.TokenizeTransactionId}");
            return entity.TokenizeTransactionId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PersistTokenizeTransaction] InsertAsync failed (non-fatal): {ex.Message}");
            return null;
        }
    }
}
