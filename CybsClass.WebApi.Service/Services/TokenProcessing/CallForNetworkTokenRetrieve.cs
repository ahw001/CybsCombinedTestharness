using CybsClass.Cybersource.Transactions;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CybsClass.WebApi.Service.Services.TokenProcessing;

// Step 6 of the tokenize -> instrument identifier -> network token provisioning flow.
// Plain GET first (per tms-net-tkn-card-retrieve-tkn-consumer-intro doc) — unlike the
// POST /tms/v2/tokenized-cards shortcut (CallForTokenizedCards), this GET's response
// encryption (plain JSON vs BLOB/JOSE) is unconfirmed by CyberSource's docs. If live
// testing shows a BLOB, switch to CallCyberSourceApiJsonApplicationJose-style handling.
public static class CallForNetworkTokenRetrieve
{
    public static async Task<JsonObject> RunAsync(string instrumentIdentifierId)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        Console.WriteLine($"\n[CallForNetworkTokenRetrieve] instrumentIdentifierId={instrumentIdentifierId}");

        try
        {
            string resource = $"/tms/v2/tokenized-cards/{instrumentIdentifierId}";
            var jsonObject = await CallCyberSource.CallCyberSourceApiGet(resource, false);

            Console.WriteLine($"\n[CallForNetworkTokenRetrieve] RESPONSE:\n{JsonSerializer.Serialize(jsonObject, options)}");
            return jsonObject;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[CallForNetworkTokenRetrieve] Exception: {e.Message}");
            string safe = e.Message.Replace("\"", "\\\"");
            JsonDocument doc = JsonDocument.Parse($"{{\"Exception\":\"{safe}\"}}");
            return JsonObject.Create(doc.RootElement)!;
        }
    }
}
