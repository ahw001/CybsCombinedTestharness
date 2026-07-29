using CybsClass.Cybersource.Models.DTOs;
using CybsClass.EntityModels;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CybsClass.WebApi.Service.Services.DBOperations;

public static class PersistPayByLinkData
{
    /// <summary>
    /// Persists a new Pay by Link record after a successful CyberSource create call.
    /// Returns the saved entity (with DB-assigned PK and UUID), or null if the write failed.
    /// </summary>
    public static async Task<PayByLinkTransaction?> CreatePayByLinkAsync(PayByLinkRequestDto dto, JsonObject cybsResponse)
    {
        Console.WriteLine("Inserting Pay by Link transaction ...");

        string cybsLinkId = string.Empty;
        string paymentLink = string.Empty;
        string purchaseNumber = string.Empty;
        string status = "ACTIVE";

        try
        {
            if (cybsResponse.TryGetPropertyValue("id", out JsonNode? idNode) && idNode is not null)
                cybsLinkId = idNode.GetValue<string>();

            if (cybsResponse.TryGetPropertyValue("status", out JsonNode? statusNode) && statusNode is not null)
                status = statusNode.GetValue<string>();

            if (cybsResponse.TryGetPropertyValue("purchaseNumber", out JsonNode? pnNode) && pnNode is not null)
                purchaseNumber = pnNode.GetValue<string>();

            // purchaseInformation.paymentLink
            if (cybsResponse.TryGetPropertyValue("purchaseInformation", out JsonNode? purchaseInfoNode)
                && purchaseInfoNode is JsonObject purchaseInfoObj
                && purchaseInfoObj.TryGetPropertyValue("paymentLink", out JsonNode? linkNode)
                && linkNode is not null)
            {
                paymentLink = linkNode.GetValue<string>();
            }
        }
        catch (Exception ex)
        {
            // Non-fatal: the row is still written with whatever fields were extracted.
            Console.WriteLine($"Warning: could not extract all fields from CyberSource response: {ex.Message}");
        }

        var entity = new PayByLinkTransaction
        {
            PayByLinkUuid = Guid.NewGuid(),
            CybersourceLinkId = cybsLinkId,
            PaymentLink = paymentLink,
            PurchaseNumber = purchaseNumber,
            TotalAmount = dto.TotalAmount,
            Currency = dto.Currency ?? "USD",
            CustomerName = dto.CustomerName,
            Email = dto.Email,
            Phone = dto.Phone,
            DeliveryMethod = dto.DeliveryMethod ?? "clipboard",
            Status = status,
            TransactionJson = cybsResponse.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            using CybsDbContext db = new();
            db.PayByLinkTransactions.Add(entity);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            DbErrorHandler.Log(nameof(CreatePayByLinkAsync), ex);
            return null;
        }

        Console.WriteLine($"Pay by Link transaction persisted. Id={entity.PayByLinkTransactionId}, CybsLinkId={entity.CybersourceLinkId}");

        return entity;
    }

    /// <summary>
    /// Updates the status of an existing Pay by Link record after a status check call.
    /// Returns true when the update succeeded.
    /// </summary>
    public static async Task<bool> UpdatePayByLinkStatusAsync(int payByLinkTransactionId, string newStatus, string updatedJson)
    {
        Console.WriteLine($"Updating Pay by Link status: Id={payByLinkTransactionId}, NewStatus={newStatus}");

        try
        {
            using CybsDbContext db = new();
            var entity = await db.PayByLinkTransactions.FindAsync(payByLinkTransactionId);

            if (entity is null)
            {
                Console.WriteLine($"Pay by Link record {payByLinkTransactionId} not found.");
                return false;
            }

            entity.Status = newStatus;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.TransactionJson = updatedJson;

            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            DbErrorHandler.Log(nameof(UpdatePayByLinkStatusAsync), ex);
            return false;
        }
    }
}
