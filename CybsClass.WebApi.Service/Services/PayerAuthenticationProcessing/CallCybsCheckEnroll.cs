using CybsClass.Cybersource.Models;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Models.BaseData;
using CybsClass.Cybersource.Transactions;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;


namespace CybsClass.WebApi.Service.Services.PayerAuthenticationProcessing
{
    public static class CallCybsCheckEnroll
    {

        // Combined check-enrollment + authorization (recommended integration per the
        // CyberSource combining-services guide): the authorization endpoint is used with
        // CONSUMER_AUTHENTICATION in the actionList. Frictionless outcomes authorize in
        // this single call; challenge outcomes stop before authorization and return the
        // step-up fields, after which VALIDATE_CONSUMER_AUTHENTICATION completes both.
        public static async Task<JsonObject> RunAsyncFlexJsonObject(CheckEnrollDto checkEnrollDto)
        {
            JsonObject jsonObject = new JsonObject();
            string resource = "/pts/v2/payments";

            try
            {
                var inboundAuthorizationOptions = checkEnrollDto.ProcessingInformation?.AuthorizationOptions;

                checkEnrollDto.ProcessingInformation = new ProcessingInformation
                {
                    AuthorizationOptions = inboundAuthorizationOptions,
                    ActionList = new string[] { "CONSUMER_AUTHENTICATION", "TOKEN_CREATE" },
                    ActionTokenTypes = new string[] { "paymentInstrument, instrumentIdentifier" }
                };

                checkEnrollDto.ClientReferenceInformation = new ClientReferenceInformation
                {
                    Code = "PAENROLLAUTH" + DateTime.UtcNow.ToString("yyyyMMddHHmmss")
                };

                var options = new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
                string jsonString = JsonSerializer.Serialize(checkEnrollDto, options);

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

                Console.WriteLine("\n************* CALLING FOR PAYER AUTH CHECK ENROLLMENT + AUTHORIZATION (COMBINED) *****\n");


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

        public static async Task<JsonObject> RunAsyncFlexAftJsonPaymentObject(AftCheckEnrollDto aftCheckEnrollDto)
        {
            JsonObject jsonObject = new JsonObject();
            string resource = "/pts/v2/payments";

            var inboundPartialAuthIndicator = aftCheckEnrollDto.ProcessingInformation?.AuthorizationOptions?.PartialAuthIndicator;

            if (aftCheckEnrollDto.ProcessingInformation is not null)
            {
                aftCheckEnrollDto.ProcessingInformation.ActionList = new string[] { "TOKEN_CREATE" };
                aftCheckEnrollDto.ProcessingInformation.ActionTokenTypes = new string[] { "paymentInstrument, instrumentIdentifier" };
            }

            aftCheckEnrollDto.ClientReferenceInformation = new ClientReferenceInformation
            {
                Code = "AFT" + DateTime.UtcNow.ToString("yyyyMMddHHmmss")
            };

            aftCheckEnrollDto.ProcessingInformation = new ProcessingInformation
            {
                AuthorizationOptions = new AuthorizationOptions
                {
                    AftIndicator = true,
                    PartialAuthIndicator = inboundPartialAuthIndicator,
                },

                CommerceIndicator = "internet",
                BusinessApplicationId = "AA",
                PurposeOfPayment = "ISACCT",
                ActionList = new[] { "CONSUMER_AUTHENTICATION", "TOKEN_CREATE" },
                ActionTokenTypes = new string[] { "paymentInstrument, instrumentIdentifier" }
            };

            aftCheckEnrollDto.MerchantInformation = new MerchantInformation
            {
                MerchantCategoryCode = "6012",
                MerchantDescriptor = new MerchantDescriptor
                {
                    Name = "Test Merchant",
                    Locality = "San Francisco",
                    AdministrativeArea = "CA",
                    Address1 = "123 Test St",
                    PostalCode = "94105"
                }
            };

            try
            {

                var options = new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
                string jsonString = JsonSerializer.Serialize(aftCheckEnrollDto, options);

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

                Console.WriteLine("\n************* CALLING FOR PAYER AUTH CHECK ENROLLMENT WITH AUTHORIZATION *****\n");


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