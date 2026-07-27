using CybsClass.Cybersource.Authentication;
using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CybsClass.WebApi.Service.Services.ApplePay
{
    // Implements the server side of Apple Pay's onvalidatemerchant handshake: POSTs to the
    // validationURL the client received from ApplePaySession, authenticating the request via
    // mutual TLS with the Merchant Identity Certificate (a completely separate credential from
    // the Payment Processing Certificate used for decryption — see ApplePayCredentials).
    public static class CallForMerchantValidation
    {
        public static async Task<JsonObject> RunAsyncJsonObject(string validationUrl, string merchantIdentifier, string? initiativeContext)
        {
            var jsonObject = new JsonObject();

            try
            {
                if (ApplePayCredentials.MerchantIdentityCertificate is null)
                    throw new InvalidOperationException("Merchant Identity certificate is not loaded — check ApplePaySettings.MerchantIdentityCertPath.");

                if (string.IsNullOrWhiteSpace(validationUrl))
                    throw new ArgumentException("validationUrl is required.");

                using var handler = new HttpClientHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Manual
                };
                handler.ClientCertificates.Add(ApplePayCredentials.MerchantIdentityCertificate);

                using var client = new HttpClient(handler);

                var requestBody = new
                {
                    merchantIdentifier,
                    displayName = "Apple Pay Test Harness",
                    initiative = "web",
                    initiativeContext
                };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"[ApplePay] POSTing merchant validation to {validationUrl} with initiativeContext={initiativeContext}");

                var response = await client.PostAsync(validationUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    string prettyBody = responseBody;
                    try
                    {
                        var node = JsonNode.Parse(responseBody);
                        if (node is not null)
                            prettyBody = node.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
                    }
                    catch (JsonException) { /* not valid JSON — log as-is */ }

                    Console.WriteLine($"[ApplePay] Merchant validation failed: HTTP {(int)response.StatusCode} —\n{prettyBody}");
                    jsonObject["error"] = $"Apple merchant validation failed: HTTP {(int)response.StatusCode}";
                    jsonObject["cybersourceJson"] = responseBody;
                    return jsonObject;
                }

                return JsonNode.Parse(responseBody)?.AsObject() ?? new JsonObject();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ApplePay] ERROR during merchant validation: {e.Message}");
                jsonObject = new JsonObject();
                jsonObject["error"] = e.Message;
                return jsonObject;
            }
        }
    }
}
