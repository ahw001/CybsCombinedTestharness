using CybsClass.Cybersource.Models.BaseData;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Models.OutboundTransObjects;
using CybsClass.Cybersource.Transactions;
using CybsClass.WebApi.Service.Services.DBOperations;
using CybsClass.WebApi.Service.Services.JWTProcessing;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CybsClass.WebApi.Service.Services.FlexUcContextProcessing
{
    // v0 capture context (/up/v1/capture-contexts, SecureAcceptance.js) built from a saved
    // UnifiedCheckoutConfiguration row — the "manual transient token" vehicle for the store
    // checkout. Unlike v1 (/uc/v1/sessions), the v0 widget never auto-processes: show()'s
    // promise resolves with the transient token, which the client then submits to the
    // /api/unified/v1tokenpayment follow-on. Deliberately separate from the legacy
    // CallForCaptureContext (hardcoded mandate, /unifiedcheckout page), which stays untouched.
    public static class CallForCaptureContextV0FromConfig
    {
        private static string[]? ParseCsv(string? csv) =>
            string.IsNullOrWhiteSpace(csv)
                ? null
                : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        public static async Task<string> RunAsync(B2cCustomerDto b2cCustomerDto)
        {
            string flexToken = string.Empty;
            string resource = "/up/v1/capture-contexts";

            try
            {
                CybsClass.EntityModels.UnifiedCheckoutConfiguration? config = null;
                if (b2cCustomerDto.UnifiedCheckoutConfigurationId is int configId)
                {
                    config = await DBUnifiedCheckoutConfigurationServices.GetByIdAsync(configId);
                }

                bool requestShipping = config?.RequestShipping ?? true;

                var captureContext = new CaptureContext
                {
                    TargetOrigins = new List<string> { b2cCustomerDto.TargetOrigin ?? "https://localhost:7133" },
                    AllowedCardNetworks = ParseCsv(config?.AllowedCardNetworks)
                        ?? new string[] { "VISA", "MASTERCARD", "AMEX", "JCB", "DISCOVER", "DINERSCLUB" },
                    AllowedPaymentTypes = ParseCsv(config?.AllowedPaymentTypes)
                        ?? new string[] { "PANENTRY" },
                    Country = config?.Country ?? "US",
                    Locale = config?.Locale ?? "en_US",
                    ClientVersion = "0.30",
                    OrderInformation = new OrderInformation
                    {
                        AmountDetails = new AmountDetails
                        {
                            TotalAmount = (b2cCustomerDto.TotalAmount ?? 0m).ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                            Currency = "USD"
                        },
                        BillTo = new BillTo
                        {
                            Address1 = b2cCustomerDto.Address1,
                            Address2 = b2cCustomerDto.Address2,
                            Address3 = b2cCustomerDto.Address3,
                            AdministrativeArea = b2cCustomerDto.AdministrativeArea,
                            BuildingNumber = b2cCustomerDto.BuildingNumber,
                            Country = b2cCustomerDto.Country,
                            Locality = b2cCustomerDto.City,
                            PostalCode = b2cCustomerDto.PostalCode,
                            FirstName = b2cCustomerDto.FirstName,
                            LastName = b2cCustomerDto.LastName,
                            Email = b2cCustomerDto.Email,
                            MiddleName = b2cCustomerDto.MiddleName,
                            Title = b2cCustomerDto.Title,
                            PhoneNumber = b2cCustomerDto.Phone,
                            PhoneType = b2cCustomerDto.PhoneType,
                        },
                        // Only pre-fill shipping when the mandate actually requests it — and from
                        // the real shipping fields (the legacy builder's FirstName=PostalCode slip
                        // is deliberately not repeated here).
                        ShipTo = !requestShipping ? null : new BillTo
                        {
                            Address1 = b2cCustomerDto.ShippingAddress1 ?? b2cCustomerDto.Address1,
                            Address2 = b2cCustomerDto.ShippingAddress2 ?? b2cCustomerDto.Address2,
                            Address3 = b2cCustomerDto.ShippingAddress3 ?? b2cCustomerDto.Address3,
                            AdministrativeArea = b2cCustomerDto.ShippingAdministrativeArea ?? b2cCustomerDto.AdministrativeArea,
                            BuildingNumber = b2cCustomerDto.ShippingBuildingNumber ?? b2cCustomerDto.BuildingNumber,
                            Country = b2cCustomerDto.ShippingCountry ?? b2cCustomerDto.Country,
                            Locality = b2cCustomerDto.ShippingCity ?? b2cCustomerDto.City,
                            PostalCode = b2cCustomerDto.ShippingPostalCode ?? b2cCustomerDto.PostalCode,
                            FirstName = b2cCustomerDto.ShippingFirstName ?? b2cCustomerDto.FirstName,
                            LastName = b2cCustomerDto.ShippingLastName ?? b2cCustomerDto.LastName
                        },
                        Freight = null
                    },
                    CaptureMandate = new CaptureMandate
                    {
                        BillingType = config?.BillingType ?? "FULL",
                        RequestEmail = config?.RequestEmail ?? true,
                        RequestPhone = config?.RequestPhone ?? true,
                        RequestShipping = requestShipping,
                        ShipToCountries = new string[] { config?.Country ?? "US" },
                        ShowAcceptedNetworkIcons = config?.ShowAcceptedNetworkIcons ?? true
                    }
                };

                Console.WriteLine("\n************* CALLING FOR V0 CAPTURE CONTEXT FROM CONFIG (manual token) *****\n");

                var options = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
                string jsonString = JsonSerializer.Serialize(captureContext, options);

                // OrderInformation.ShipTo/Freight default to `= new()` in ApiData.cs — strip any
                // empty objects the serializer still emitted so the wire request never carries
                // "shipTo": {} / "freight": {}.
                JsonNode? jsonNode = JsonNode.Parse(jsonString);
                if (jsonNode?["orderInformation"] is JsonObject orderInfo)
                {
                    orderInfo.Remove("freight");
                    if (orderInfo["shipTo"] is JsonObject s && s.Count == 0) orderInfo.Remove("shipTo");
                }
                jsonString = jsonNode!.ToJsonString(new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

                Console.WriteLine($"V0-FROM-CONFIG CAPTURE CONTEXT REQUEST:\n{jsonString}");

                flexToken = await CallCyberSource.CallCyberSourceCaptureContext(jsonString, resource);

                Console.WriteLine("****************** DECODED V0-FROM-CONFIG CTX JWT **************************");
                Console.WriteLine(JWTDeCode.DeCodeJWT(flexToken));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return e.Message;
            }

            return flexToken;
        }
    }
}
