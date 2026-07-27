using CybsClass.Cybersource.Transactions;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CybsClass.WebApi.Service.Services.TokenProcessing;

// Step 5 of the network token provisioning flow — per CyberSource's
// tms-tokenize-ii-nt-tt-intro doc ("Transient Token Tokenization with Instrument
// Identifier"): POST /tms/v2/tokenize with actionTokenTypes=["instrumentIdentifier"]
// and a Flex Microform transientTokenJwt provisions BOTH an instrumentIdentifier and
// a tokenizedCard in one call, returning both resources in the "responses" array.
//
// CONFIRMED 2026-07-17 via the official cybersource-rest-client-node SDK against a live
// transient token: the minimal documented body (tokenInformation.transientTokenJwt only,
// no "type" field anywhere) succeeded end-to-end (instrumentIdentifier + tokenizedCard
// both httpStatus 200) once request-body MLE was enabled. No instrumentIdentifier/type
// object is required or sent by the working reference call — see
// TestHarness\NodeServerTest\fuckYouClaudeNet.md for the full raw session log.
public static class CallForNetworkTokenProvision
{
    public static async Task<JsonObject> RunAsync(string transientTokenJwt)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        Console.WriteLine($"\n[CallForNetworkTokenProvision] transientTokenJwt (first 40): {transientTokenJwt[..Math.Min(40, transientTokenJwt.Length)]}...");

        try
        {
            var requestBody = new JsonObject
            {
                ["processingInformation"] = new JsonObject
                {
                    ["actionList"] = new JsonArray("TOKEN_CREATE"),
                    ["actionTokenTypes"] = new JsonArray("instrumentIdentifier")
                },
                ["tokenInformation"] = new JsonObject
                {
                    ["transientTokenJwt"] = transientTokenJwt
                }
            };

            // Log prettified (Hard Constraint), but send COMPACT JSON as the actual wire payload —
            // cybersource-rest-client-node's MLEUtility.encryptRequestPayload() encrypts
            // JSON.stringify(requestBody, null, 0) (zero whitespace). This helper previously
            // encrypted the pretty-printed logging string itself, embedding literal newlines/
            // indentation into the plaintext sent to CyberSource — a real, distinct bug from the
            // JSON *shape* questions investigated elsewhere.
            string prettyJsonForLog = requestBody.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine($"\n[CallForNetworkTokenProvision] REQUEST TO CYBERSOURCE:\n{prettyJsonForLog}");

            string compactJsonForWire = requestBody.ToJsonString();
            var jsonObject = await CallCyberSource.CallCyberSourceApiJsonMle(compactJsonForWire, "/tms/v2/tokenize");

            Console.WriteLine($"\n[CallForNetworkTokenProvision] RESPONSE FROM CYBERSOURCE:\n{JsonSerializer.Serialize(jsonObject, options)}");
            return jsonObject;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[CallForNetworkTokenProvision] Exception: {e.Message}");
            string safe = e.Message.Replace("\"", "\\\"");
            JsonDocument doc = JsonDocument.Parse($"{{\"Exception\":\"{safe}\"}}");
            return JsonObject.Create(doc.RootElement)!;
        }
    }
}
