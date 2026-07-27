using CybsClass.Cybersource.Models;
using CybsClass.Cybersource.Models.BaseData;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Models.OutboundTransObjects;
using CybsClass.Cybersource.Transactions;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;


namespace CybsClass.WebApi.Service.Services.CcTransatcionProcessing
{
    public static class CallForCybsAuthTokenCreate
    {
        // This class populates the Authorization object for serialization
        public static async Task<JsonObject> RunAsyncJsonObject(B2cCustomerDto b2cCustomerDto)
        {
            JsonObject jsonObject = new JsonObject();
            string resource = "/pts/v2/payments";
            bool markedForCapture = b2cCustomerDto.MarkedForCapture;
            bool allowPartialAuth = b2cCustomerDto.AllowPartialAuth;
            Guid guid = Guid.NewGuid();
            // TotalAmount.ToString() uses the server process's current culture — on a host
            // whose default locale doesn't use "." as the decimal separator (e.g. "10,00"),
            // this produces an amount CyberSource rejects outright as an invalid request.
            // Format against InvariantCulture so this is stable regardless of server locale.
            string formattedAmount = (b2cCustomerDto.TotalAmount ?? 0m).ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

            if (b2cCustomerDto.CardType == "002")
            { 
                b2cCustomerDto.Reason = "7";
            }

            try
            {

                var authorizeData = new AuthorizeData
                {

                    ClientReferenceInformation = new ClientReferenceInformation
                    {
                        Code = guid.ToString()

                    },
                    PaymentInformation = new Dictionary<string, FullCard>
                    {

                        ["card"] = new FullCard
                        {
                            Number = b2cCustomerDto.AccountNumber,
                            ExpirationMonth = b2cCustomerDto.ExpMonth,
                            ExpirationYear = b2cCustomerDto.ExpYear,
                            SecurityCode = b2cCustomerDto.Cvv,
                            Type = b2cCustomerDto.CardType
                        },

                    },
                    OrderInformation = new OrderInformation
                    {

                        AmountDetails = new AmountDetails { Currency = "USD", TotalAmount = formattedAmount }, //Convert.ToString(b2cCustomerDto.TotalAmount)

                        BillTo = new BillTo { FirstName = b2cCustomerDto.FirstName, LastName = b2cCustomerDto.LastName, Address1 = b2cCustomerDto.Address1, Locality = b2cCustomerDto.City, AdministrativeArea = b2cCustomerDto.AdministrativeArea, PostalCode = b2cCustomerDto.PostalCode, Country = "US", Email = b2cCustomerDto.Email, PhoneNumber = b2cCustomerDto.Phone },
                        ShipTo = new BillTo { FirstName = b2cCustomerDto.FirstName, LastName = b2cCustomerDto.LastName, Address1 = b2cCustomerDto.Address1, Locality = b2cCustomerDto.City, AdministrativeArea = b2cCustomerDto.AdministrativeArea, PostalCode = b2cCustomerDto.PostalCode, Country = "US" },
                        // OrderInformation.Freight defaults to `new()` (all-null Freight) unless
                        // overridden, which serializes as an empty "freight": {} object — this
                        // transaction has no freight/shipping charges to report, so omit it entirely.
                        Freight = null

                    },
                    ProcessingInformation = new ProcessingInformation
                    {
                        ActionList = new[] { "TOKEN_CREATE" },
                        ActionTokenTypes = b2cCustomerDto.ActionTokenTypes,
                        Capture = markedForCapture,
                        // CyberSource's documented "Authorization and Creating TMS Tokens" request
                        // shape omits authorizationOptions/initiator entirely for a plain (non-MIT)
                        // transaction. Sending initiator.type="customer" + credentialStoredOnFile=true
                        // unconditionally (as before) is not part of that documented shape and was a
                        // candidate for the "INVALID_DATA"/"Invalid Json Request" decline — only build
                        // AuthorizationOptions when partial-auth actually requires it.
                        AuthorizationOptions = allowPartialAuth
                            ? new AuthorizationOptions
                            {
                                PartialAuthIndicator = true,
                                IgnoreAvsResult = true,
                                Initiator = new Initiator
                                {
                                    Type = "customer",
                                    CredentialStoredOnFile = true,
                                    MerchantInitiatedTransaction = string.IsNullOrEmpty(b2cCustomerDto.Reason) ? null : new MerchantInitiatedTransaction { Reason = b2cCustomerDto.Reason }
                                }
                            }
                            : null
                    }
                    //ProcessingInformation = new ProcessingInformation { Capture = markedForCapture }
                };

                var options = new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull};
                string jsonString = JsonSerializer.Serialize(authorizeData, options);

                /* USE IF YOU NEED TO MODIFIY JSON ---------
                 * 
                JsonNode jsonNode = JsonNode.Parse(jsonString);

                // Navigate to the 'ProcessingInformation' object
                JsonNode processingInfo = jsonNode["ProcessingInformation"];

                // Check if the 'Capture' field exists and remove it
                if (processingInfo["Capture"] != null)
                {
                    processingInfo.AsObject().Remove("Capture");
                }

                // Convert the JsonNode back to a string to see the result
                string modifiedJsonString = jsonNode.ToJsonString();
                Console.WriteLine($"MODIFIED STRING WITHOUT Capture: { modifiedJsonString}");
                *
                */

                Console.WriteLine("\n************* CALLING FOR AUTH AND TOKEN CREATE *****\n");

                jsonObject = await CallCyberSource.CallCyberSourceApiJson(jsonString, resource, false);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                jsonObject = new JsonObject();
                jsonObject["error"] = e.Message;
                return jsonObject;
            }

            return jsonObject;
        }
    }
}