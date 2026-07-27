using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Models.OutboundTransObjects;
using CybsClass.Cybersource.Transactions;
using CybsClass.WebApi.Service.Services.JWTProcessing;
using CybsClass.WebApi.Service.Services.DBOperations;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CybsClass.WebApi.Service.Services.FlexUcContextProcessing
{
    // Real Unified Checkout v1 session generator — POST /uc/v1/sessions (the VAS.UnifiedCheckout
    // client). Fully separate from CallForCaptureContext (v0, /up/v1/capture-contexts), which
    // backs the legacy /unifiedcheckout page and is never touched by this class or its endpoint.
    // See UnifiedCheckoutPlanning.md Execution Goal 1.
    public static class CallForCaptureContextV1
    {
        // Splits a comma-delimited UnifiedCheckoutConfiguration column (e.g. "VISA,MASTERCARD")
        // into the array shape the request expects. Returns null (not empty array) when the
        // config didn't set the field, so the hardcoded Card-only fallback still applies.
        private static string[]? ParseCsv(string? csv) =>
            string.IsNullOrWhiteSpace(csv)
                ? null
                : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        public static async Task<string> RunAsync(B2cCustomerDto b2cCustomerDto)
        {
            string resource = "/uc/v1/sessions";
            string flexToken = string.Empty;

            try
            {
                CybsClass.EntityModels.UnifiedCheckoutConfiguration? config = null;
                if (b2cCustomerDto.UnifiedCheckoutConfigurationId is int configId)
                {
                    config = await DBUnifiedCheckoutConfigurationServices.GetByIdAsync(configId);
                }

                var request = new UnifiedCheckoutV1SessionRequest
                {
                    TargetOrigins = new List<string> { b2cCustomerDto.TargetOrigin ?? "https://localhost:7133" },
                    AllowedCardNetworks = ParseCsv(config?.AllowedCardNetworks)
                        ?? new string[] { "VISA", "MASTERCARD", "AMEX", "JCB", "DISCOVER", "DINERSCLUB" },
                    AllowedPaymentTypes = ParseCsv(config?.AllowedPaymentTypes)
                        ?? new string[] { "PANENTRY" },
                    Country = config?.Country ?? "US",
                    Locale = config?.Locale ?? "en_US",
                    ButtonType = config?.ButtonType,
                    CaptureMandate = new UnifiedCheckoutV1CaptureMandate
                    {
                        BillingType = config?.BillingType ?? "FULL",
                        RequestEmail = config?.RequestEmail ?? true,
                        RequestPhone = config?.RequestPhone ?? true,
                        RequestShipping = config?.RequestShipping ?? true,
                        ShipToCountries = new string[] { config?.Country ?? "US" },
                        ShowAcceptedNetworkIcons = config?.ShowAcceptedNetworkIcons ?? true,
                        ShowConfirmationStep = config?.ShowConfirmationStep,
                        RequestSaveCredentials = config?.RequestSaveCredentials
                    },
                    Data = new UnifiedCheckoutV1Data
                    {
                        OrderInformation = new UnifiedCheckoutV1OrderInformation
                        {
                            AmountDetails = new UnifiedCheckoutV1AmountDetails
                            {
                                TotalAmount = (b2cCustomerDto.TotalAmount ?? 0m).ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                                Currency = "USD"
                            },
                            BillTo = new UnifiedCheckoutV1BillTo
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
                            ShipTo = new UnifiedCheckoutV1ShipTo
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
                                LastName = b2cCustomerDto.ShippingLastName ?? b2cCustomerDto.LastName,
                            }
                        }
                    }
                };

                Console.WriteLine("\n************* CALLING FOR UNIFIED CHECKOUT V1 SESSION (uc/v1/sessions) *****\n");

                var options = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
                string jsonString = JsonSerializer.Serialize(request, options);

                Console.WriteLine($"UC V1 SESSION REQUEST:\n{jsonString}");

                flexToken = await CallCyberSource.CallCyberSourceCaptureContext(jsonString, resource);

                Console.WriteLine("****************** DECODED UC V1 SESSION JWT **************************");
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
