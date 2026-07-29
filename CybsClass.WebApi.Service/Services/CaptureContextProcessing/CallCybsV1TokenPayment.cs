using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Transactions;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CybsClass.WebApi.Service.Services.CaptureContextProcessing
{
    // Manual Unified Checkout follow-on: POST /pts/v2/payments with
    // tokenInformation.transientTokenJwt, built as a clean wire shape (contrast with the legacy
    // CallCybsCtxPayment, which is kept as-is for the /unifiedcheckout page):
    //   - orderInformation.amountDetails.totalAmount (NOT authorizedAmount)
    //   - no top-level billTo duplicate (not a /pts/v2/payments field)
    //   - billTo/shipTo included only when provided — never "shipTo": {} / "freight": {}
    //   - actionList ["TOKEN_CREATE"] only when requested, with actionTokenTypes as a proper
    //     array of individual values
    // The body is assembled as a JsonObject specifically so ApiData.cs's `= new()` sub-object
    // defaults can never leak empty objects onto the wire.
    public static class CallCybsV1TokenPayment
    {
        public static async Task<JsonObject> RunAsync(UnifiedTokenPaymentDto dto)
        {
            JsonObject responseBlob;
            string resource = "/pts/v2/payments";

            try
            {
                var serializeOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

                var orderInformation = new JsonObject
                {
                    ["amountDetails"] = new JsonObject
                    {
                        ["totalAmount"] = dto.TotalAmount ?? "0.00",
                        ["currency"] = dto.Currency ?? "USD"
                    }
                };

                if (dto.BillTo is not null)
                {
                    orderInformation["billTo"] = JsonSerializer.SerializeToNode(dto.BillTo, serializeOptions);
                }

                if (dto.ShipTo is not null)
                {
                    orderInformation["shipTo"] = JsonSerializer.SerializeToNode(dto.ShipTo, serializeOptions);
                }

                var body = new JsonObject
                {
                    ["clientReferenceInformation"] = new JsonObject
                    {
                        ["code"] = string.IsNullOrWhiteSpace(dto.ClientReferenceCode)
                            ? Guid.NewGuid().ToString()
                            : dto.ClientReferenceCode
                    },
                    ["orderInformation"] = orderInformation,
                    ["tokenInformation"] = new JsonObject
                    {
                        ["transientTokenJwt"] = dto.TransientTokenJwt
                    }
                };

                if (dto.EnableTokenCreate)
                {
                    var processingInformation = new JsonObject
                    {
                        ["actionList"] = new JsonArray("TOKEN_CREATE")
                    };

                    var tokenTypes = dto.ActionTokenTypes?
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .Select(t => t.Trim())
                        .ToArray();

                    if (tokenTypes is { Length: > 0 })
                    {
                        processingInformation["actionTokenTypes"] =
                            new JsonArray(tokenTypes.Select(t => (JsonNode)t!).ToArray());
                    }

                    body["processingInformation"] = processingInformation;
                }

                string jsonString = body.ToJsonString(new JsonSerializerOptions { WriteIndented = true });

                Console.WriteLine("\n************* CALLING FOR MANUAL TRANSIENT-TOKEN PAYMENT (/pts/v2/payments) *****\n");

                responseBlob = await CallCyberSource.CallCyberSourceApiJson(jsonString, resource, false);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR IN MANUAL TRANSIENT-TOKEN PAYMENT");
                Console.WriteLine(e.Message);
                responseBlob = new JsonObject { ["error"] = e.Message };
            }

            return responseBlob;
        }
    }
}
