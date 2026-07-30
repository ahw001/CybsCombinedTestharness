using CybsClass.Cybersource.Models.BaseData;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Models.OutboundTransObjects;
using CybsClass.Cybersource.Transactions;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CybsClass.WebApi.Service.Services.CcTransatcionProcessing
{
    // Builds and sends the /pts/v2/payments request for all four documented eCheck flows.
    // Modelled on CallForCybsAuth.RunAsyncJsonObject — same resource, same transport, different
    // request envelope (ECheckAuthorizeData, because AuthorizeData cannot carry a bank node).
    public static class CallForECheckDebit
    {
        public const string TransactionTypeDebit = "DEBIT";
        public const string TransactionTypeRecurring = "RECURRING";
        public const string TransactionTypeTokenCreate = "TOKEN_CREATE";
        public const string TransactionTypeTokenDebit = "TOKEN_DEBIT";

        /// <summary>
        /// Returns the CyberSource response alongside the literal request JSON that produced it,
        /// so the endpoint can persist the request without rebuilding it, and the resolved
        /// transaction type for the ECheckTransaction row.
        /// </summary>
        public static async Task<(JsonObject Response, string RequestJson, string TransactionType)> RunAsyncJsonObject(
            B2cCustomerDto b2cCustomerDto)
        {
            const string resource = "/pts/v2/payments";
            string requestJson = string.Empty;
            string transactionType = ResolveTransactionType(b2cCustomerDto);

            try
            {
                bool useStoredToken = !string.IsNullOrWhiteSpace(b2cCustomerDto.ECheckCustomerTokenId);

                var echeckData = new ECheckAuthorizeData
                {
                    ClientReferenceInformation = new ClientReferenceInformation
                    {
                        Code = "ABC123"
                    },

                    PaymentInformation = new EcheckPaymentInformation
                    {
                        // Always "check" — this is what routes the transaction to ACH rather
                        // than card processing.
                        PaymentType = new EcheckPaymentTypeName { Name = "check" },

                        // The two payment sources are mutually exclusive. A stored-token request
                        // carries paymentInformation.customer.id and NO bank node at all; the
                        // bank-account flows carry the reverse. Sending both is not a documented
                        // shape and there is no reason to risk it.
                        Bank = useStoredToken
                            ? null
                            : new EcheckBank
                            {
                                RoutingNumber = b2cCustomerDto.RoutingNumber,
                                Account = new EcheckBankAccount
                                {
                                    Number = b2cCustomerDto.BankAccountNumber,
                                    Type = b2cCustomerDto.BankAccountType
                                }
                            },

                        Customer = useStoredToken
                            ? new Customer { Id = b2cCustomerDto.ECheckCustomerTokenId }
                            : null
                    },

                    OrderInformation = new OrderInformation
                    {
                        // eCheck is USD-only per the eCheck REST guide.
                        AmountDetails = new AmountDetails
                        {
                            Currency = "USD",
                            TotalAmount = Convert.ToString(b2cCustomerDto.TotalAmount)
                        },

                        // Every billTo field below is required for eCheck — unlike a card
                        // authorization, where most are optional. administrativeArea must be a
                        // USPS two-letter code and country must be US.
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
                        }
                    },

                    ProcessingInformation = BuildProcessingInformation(b2cCustomerDto, useStoredToken)
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                requestJson = JsonSerializer.Serialize(echeckData, options);

                Console.WriteLine($"\n************* CALLING FOR ECHECK {transactionType} *****\n");
                Console.WriteLine(requestJson);

                JsonObject response = await CallCyberSource.CallCyberSourceApiJson(requestJson, resource, false);

                return (response, requestJson, transactionType);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                var errorObject = new JsonObject
                {
                    ["error"] = e.Message
                };
                return (errorObject, requestJson, transactionType);
            }
        }

        private static ProcessingInformation BuildProcessingInformation(B2cCustomerDto dto, bool useStoredToken)
        {
            var processing = new ProcessingInformation
            {
                // "internet" is the documented default; "recurring" is what makes the debit a
                // recurring one, together with recurringOptions below.
                CommerceIndicator = dto.IsRecurring ? "recurring" : "internet"
            };

            if (dto.IsRecurring)
            {
                processing.RecurringOptions = new RecurringOptions
                {
                    FirstRecurringPayment = dto.FirstRecurringPayment
                };
            }

            // secCode is optional. Null leaves bankTransferOptions off the request entirely,
            // which is a valid documented shape — don't send an empty object.
            if (!string.IsNullOrWhiteSpace(dto.SecCode))
            {
                processing.BankTransferOptions = new BankTransferOptions { SecCode = dto.SecCode };
            }

            // Token creation only makes sense when we are supplying a bank account to tokenize.
            // Asking to tokenize an already-stored token is not a documented flow.
            if (dto.CreateECheckToken && !useStoredToken)
            {
                processing.ActionList = new[] { "TOKEN_CREATE" };

                // The client picks the token types; "customer" is the one the token-debit flow
                // actually needs, so it is the fallback when nothing was chosen.
                processing.ActionTokenTypes = dto.ActionTokenTypes is { Length: > 0 }
                    ? dto.ActionTokenTypes
                    : new[] { "customer" };
            }

            return processing;
        }

        private static string ResolveTransactionType(B2cCustomerDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.ECheckCustomerTokenId)) return TransactionTypeTokenDebit;
            if (dto.CreateECheckToken) return TransactionTypeTokenCreate;
            if (dto.IsRecurring) return TransactionTypeRecurring;
            return TransactionTypeDebit;
        }
    }
}
