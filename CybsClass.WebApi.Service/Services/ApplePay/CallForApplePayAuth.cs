using CybsClass.Cybersource.Authentication;
using CybsClass.Cybersource.Models.BaseData;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Models.Json;
using CybsClass.Cybersource.Models.OutboundTransObjects;
using CybsClass.Cybersource.Transactions;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CybsClass.WebApi.Service.Services.ApplePay
{
    // Request builder for Apple Pay (merchant decryption) — mirrors the shape of
    // CallForCybsAuthTokenCreate.RunAsyncJsonObject, but decrypts the Apple Pay payload first and
    // emits ApplePayAuthorizeData (paymentInformation.tokenizedCard) instead of a plain FullCard.
    public static class CallForApplePayAuth
    {
        // Diagnostic/persistence capture of the last decrypted Apple Pay token JSON — mirrors
        // CallCyberSource.LastRequestSent's in-process capture pattern. Read by the endpoint
        // (ApplePayEndpoints) immediately after RunAsyncJsonObject returns, to persist the
        // decrypted token alongside the CyberSource response for test-environment inspection.
        public static string LastDecryptedTokenJson { get; private set; } = string.Empty;

        private static readonly Dictionary<string, string> NetworkToCyberSourceType = new(StringComparer.OrdinalIgnoreCase)
        {
            ["visa"] = "001",
            ["masterCard"] = "002",
            ["mastercard"] = "002",
            ["amex"] = "003",
            ["americanExpress"] = "003",
            ["discover"] = "004",
            ["jcb"] = "007",
        };

        public static async Task<JsonObject> RunAsyncJsonObject(B2cCustomerDto b2cCustomerDto)
        {
            JsonObject jsonObject = new JsonObject();
            string resource = "/pts/v2/payments";
            Guid guid = Guid.NewGuid();
            string formattedAmount = (b2cCustomerDto.TotalAmount ?? 0m).ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

            try
            {
                if (string.IsNullOrWhiteSpace(b2cCustomerDto.ApplePayPaymentData))
                    throw new ArgumentException("applePayPaymentData is required for an Apple Pay authorization.");

                // 1. MERCHANT DECRYPTION — decrypt the Apple Pay EC_v1 payload locally.
                // A merchant can have more than one Payment Processing Certificate registered, and
                // Apple encrypts each token to exactly one of them. Try every loaded key: AES-GCM
                // is authenticated, so a key that does not match fails its tag check and throws
                // rather than yielding plausible-looking garbage — which makes "try the next one"
                // safe. Without this, a certificate rotation silently breaks decryption the moment
                // Apple switches which certificate it encrypts to.
                var decryptionKeys = ApplePayCredentials.PaymentProcessingPrivateKeys;
                if (decryptionKeys.Count == 0)
                    throw new InvalidOperationException("No Apple Pay Payment Processing private key is loaded — cannot decrypt the payment token.");

                string? decryptedJson = null;
                Exception? lastDecryptError = null;

                for (int i = 0; i < decryptionKeys.Count; i++)
                {
                    try
                    {
                        decryptedJson = ApplePayDecryptor.Decrypt(
                            b2cCustomerDto.ApplePayPaymentData,
                            decryptionKeys[i],
                            ApplePayCredentials.MerchantIdentifier ?? string.Empty);

                        if (i > 0)
                            Console.WriteLine($"[ApplePay] Decrypted with Payment Processing key #{i + 1} of {decryptionKeys.Count} (primary key did not match this token).");
                        break;
                    }
                    catch (Exception ex)
                    {
                        lastDecryptError = ex;
                        Console.WriteLine($"[ApplePay] Payment Processing key #{i + 1} of {decryptionKeys.Count} did not decrypt this token: {ex.Message}");
                    }
                }

                if (decryptedJson is null)
                    throw new InvalidOperationException(
                        $"Apple Pay token could not be decrypted with any of the {decryptionKeys.Count} loaded Payment Processing key(s). "
                        + "The token was likely encrypted to a certificate whose key is not deployed. "
                        + $"Last error: {lastDecryptError?.Message}", lastDecryptError);

                LastDecryptedTokenJson = decryptedJson;

                var decryptedToken = JsonSerializer.Deserialize<ApplePayDecryptedToken>(decryptedJson)
                    ?? throw new InvalidOperationException("Decrypted Apple Pay token could not be parsed.");

                Console.WriteLine("[ApplePay] Decrypted token — DPAN present: "
                    + !string.IsNullOrEmpty(decryptedToken.ApplicationPrimaryAccountNumber)
                    + ", cryptogram present: "
                    + !string.IsNullOrEmpty(decryptedToken.PaymentData?.OnlinePaymentCryptogram));

                // Apple's applicationExpirationDate is YYMMDD.
                string? expMonth = null;
                string? expYear = null;
                if (!string.IsNullOrEmpty(decryptedToken.ApplicationExpirationDate) && decryptedToken.ApplicationExpirationDate.Length == 6)
                {
                    expYear = "20" + decryptedToken.ApplicationExpirationDate.Substring(0, 2);
                    expMonth = decryptedToken.ApplicationExpirationDate.Substring(2, 2);
                }

                string cardTypeCode = NetworkToCyberSourceType.TryGetValue(b2cCustomerDto.ApplePayCardNetwork ?? string.Empty, out var mapped)
                    ? mapped
                    : "001"; // default to Visa type code if the client didn't supply a recognized network

                var applePayAuthorizeData = new ApplePayAuthorizeData
                {
                    ClientReferenceInformation = new ClientReferenceInformation { Code = guid.ToString() },

                    PaymentInformation = new ApplePayPaymentInformation
                    {
                        TokenizedCard = new WalletTokenizedCard
                        {
                            Number = decryptedToken.ApplicationPrimaryAccountNumber,
                            ExpirationMonth = expMonth,
                            ExpirationYear = expYear,
                            Cryptogram = decryptedToken.PaymentData?.OnlinePaymentCryptogram,
                            TransactionType = "1",
                            Type = cardTypeCode
                        }
                    },

                    OrderInformation = new OrderInformation
                    {
                        AmountDetails = new AmountDetails { Currency = b2cCustomerDto.Currency ?? "USD", TotalAmount = formattedAmount },
                        BillTo = new BillTo
                        {
                            FirstName = b2cCustomerDto.FirstName,
                            LastName = b2cCustomerDto.LastName,
                            Address1 = b2cCustomerDto.Address1,
                            Locality = b2cCustomerDto.City,
                            AdministrativeArea = b2cCustomerDto.AdministrativeArea,
                            PostalCode = b2cCustomerDto.PostalCode,
                            Country = "US",
                            Email = b2cCustomerDto.Email,
                            PhoneNumber = b2cCustomerDto.Phone
                        },
                        Freight = null
                    },

                    ProcessingInformation = new ProcessingInformation
                    {
                        // Apple Pay digital wallet indicator — see CLAUDE.md "Deferred" note:
                        // merchant-validation isn't wired yet, but paymentSolution is required on
                        // every Apple Pay authorization regardless of that.
                        PaymentSolution = "001"
                    }
                };

                var options = new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
                string jsonString = JsonSerializer.Serialize(applePayAuthorizeData, options);

                Console.WriteLine("\n************* CALLING FOR APPLE PAY AUTH *****\n");

                jsonObject = await CallCyberSource.CallCyberSourceApiJson(jsonString, resource, false);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ApplePay] ERROR building/sending authorization: {e.Message}");
                jsonObject = new JsonObject();
                jsonObject["error"] = e.Message;
                return jsonObject;
            }

            return jsonObject;
        }
    }
}
