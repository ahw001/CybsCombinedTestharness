using CybsClass.Cybersource.Transactions;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CybsClass.WebApi.Service.Services.TokenProcessing;

public static class CallForPaymentCredentials
{
    public static async Task<JsonObject> RunAsync(string tokenId)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        Console.WriteLine($"\n[CallForPaymentCredentials] tokenId={tokenId}");

        try
        {
            var requestBody = new JsonObject
            {
                ["paymentCredentialType"] = "CRYPTOGRAM",
                ["transactionType"]       = "ECOM"
            };

            string jsonString = requestBody.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine($"\n[CallForPaymentCredentials] REQUEST:\n{jsonString}");

            // Request: System-1 MLE (encrypted JWS bearer body, v-c-merchant-mle-kid header sent).
            // Response: System-2 BLOB (Accept: application/jose, decrypted with LegacyMlePrivateKey / key/private.pem).
            // Grid entry: mle + blob — CyberSource accepts an MLE-encrypted body and always returns a BLOB for this endpoint.
            string resource = $"/tms/v2/tokens/{tokenId}/payment-credentials";
            var jsonObject = await CallCyberSource.CallCyberSourceApiJsonMleGrid(
                jsonString, resource, "mle", "blob");

            Console.WriteLine($"\n[CallForPaymentCredentials] RESPONSE:\n{JsonSerializer.Serialize(jsonObject, options)}");
            return jsonObject;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[CallForPaymentCredentials] Exception: {e.Message}");
            string safe = e.Message.Replace("\"", "\\\"");
            JsonDocument doc = JsonDocument.Parse($"{{\"Exception\":\"{safe}\"}}");
            return JsonObject.Create(doc.RootElement)!;
        }
    }
}
