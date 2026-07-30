using CybsClient.Model;
using CybsClient.Model.Cybersource.BaseData;
using CybsClient.Model.Cybersource.Transactions;
using CybsClient.Model.DBQueries;
using CybsClient.Model.OutboundObjects;
using CybsClient.Services.DIServices;
using CybsClient.Services.DTOs;
using System.Text.Json;
using System.Text.Json.Nodes;


namespace CybsClient.Services.Utilities
{
    public static class GeneralNetworkUtilities
    {
        // Console-log-only formatting — never used for actual outgoing/incoming wire payloads.
        private static readonly JsonSerializerOptions _logOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

        private static string? error = null;
        private static DBSampleCustomerDatum? customer = new();
        private static IList<PayerAuthCardSampleDto>? sampleCards;
        private static List<CategoryDto>? categoryDtos;
        private static List<SampleInvoiceDetailDto>? sampleInvoiceDetailDtos;
        private static PayerAuthCardSampleDto? sampleCard = new();
        private static CategoryDto categoryDto = new();
        private static IList<PayerAuthCardSampleDto>? payerAuthCardSampleDtos;
        private static DBRandomCart? DBCartService = new();
        private static MerchantModelDto? merchantModel = new();
        private static DBMerchantSampleDatum? merchantSampleDatum = new();
        private static CcTransactionTypes currentTransaction;
        private static ISessionTransactions sessionTransactions = new SessionTransactions();

        public static async Task<List<CategoryDto>> GetCategories()
        {

            categoryDtos = new List<CategoryDto>();

            JsonSerializerOptions jsonOptions = new()
            {
                PropertyNameCaseInsensitive = true
            };

            currentTransaction = new CcTransactionTypes();
            currentTransaction = CcTransactionTypes.GET_CATEGORY_LIST;

            string input = "{}";

            try
            {
                SessionTransJson sessionResponse = await CallMinAPIs.SubmitForFollowOn(input,
                    sessionTransactions, currentTransaction);

                if (sessionResponse != null && sessionResponse.TransactionJson != null && sessionResponse.JsonTransactionStateValues == TransactionStateValues.Complete)
                {
                    var jsonNode = sessionResponse.TransactionJson;
                    categoryDtos = JsonSerializer.Deserialize<List<CategoryDto>>(jsonNode); // Fixed instantiation
                    if (categoryDtos is not null)
                    {
                        return categoryDtos.ToList();
                    }
                    else
                    {
                        sessionResponse!.error = "Error: " + "No or bad response from server";
                        sessionTransactions.AddTrans(sessionResponse);
                        error = "Error: " + "No or bad response from server";
                        categoryDto = new();
                        categoryDtos = new List<CategoryDto>(); // Fixed instantiation
                        categoryDto.Error = error;
                        categoryDtos.Add(categoryDto);
                        return categoryDtos.ToList();
                    }

                }
                else if (sessionResponse != null && sessionResponse.TransactionJson != null && sessionResponse.JsonTransactionStateValues == TransactionStateValues.Error)
                {
                    var jsonNode = sessionResponse.TransactionJson;
                    sessionResponse!.error = "Error: " + sessionResponse.TransactionStatus;
                    sessionTransactions.AddTrans(sessionResponse);
                    categoryDto = new();
                    categoryDtos = new List<CategoryDto>(); // Fixed instantiation
                    categoryDto.Error = "Error: " + sessionResponse.TransactionStatus;
                    categoryDtos.Add(categoryDto);
                    return categoryDtos.ToList();
                }
                else
                {
                    error = "Error: " + "No or bad response from server";
                    sessionResponse!.error = error;
                    sessionTransactions.AddTrans(sessionResponse);
                    categoryDto = new();
                    categoryDtos = new List<CategoryDto>(); // Fixed instantiation
                    categoryDto.Error = error;
                    categoryDtos.Add(categoryDto);
                    return categoryDtos.ToList();
                }
            }
            catch (Exception ex)
            {
                // Return an empty list with the error message
                error = "Error: " + ex;
                SessionTransJson sessionResponse = new();
                sessionResponse!.error = error;
                sessionTransactions.AddTrans(sessionResponse);
                categoryDto = new();
                categoryDtos = new List<CategoryDto>(); // Fixed instantiation
                categoryDto.Error = error;
                categoryDtos.Add(categoryDto);
                return categoryDtos.ToList();
            }
        }

        public static async Task<B2cCustomer> PopulateBilling(List<PayerAuthCardSampleDto> cardList)
        {
            string input = "{}";

            currentTransaction = new CcTransactionTypes();
            currentTransaction = CcTransactionTypes.RANDOM_CUSTOMER;

            const int maxRetries = 3;
            int attempt = 0;
            bool success = false;

            if (cardList is not null)
            {
                JsonSerializerOptions jsonOptions = new()
                {
                    PropertyNameCaseInsensitive = true
                };

                try
                {
                    Console.WriteLine($"Calling SubmitForFollowOn with type: {currentTransaction}");

                    SessionTransJson sessionResponse = await CallMinAPIs.SubmitForFollowOn(input,
                        sessionTransactions, currentTransaction);

                    if (sessionResponse != null && sessionResponse.TransactionJson != null && sessionResponse.JsonTransactionStateValues == TransactionStateValues.Complete)
                    {
                        var jsonNode = sessionResponse.TransactionJson;

                        Console.WriteLine(jsonNode?.ToJsonString(_logOptions));

                        while (attempt < maxRetries && !success)
                        {
                            try
                            {
                                attempt++;

                                // Optional: dump JSON on first failure
                                if (attempt > 1)
                                    Console.WriteLine($"Retrying deserialization attempt {attempt}...\nJSON: {jsonNode?.ToJsonString(_logOptions)}");

                                if (jsonNode is JsonObject)
                                {
                                    customer = JsonSerializer.Deserialize<DBSampleCustomerDatum>(jsonNode, jsonOptions);
                                }
                                else if (jsonNode is JsonArray)
                                {
                                    Console.WriteLine("❌ ERROR: Expected a customer object, but got an array.");
                                    error = "Unexpected JSON structure: array instead of object";
                                }
                                else
                                {
                                    Console.WriteLine("❌ ERROR: Unexpected JSON type for customer data.");
                                    error = "Unexpected JSON type for customer data";
                                }


                                if (customer == null)
                                {
                                    Console.WriteLine($"❌ Deserialization returned null. Raw JSON: {jsonNode!.ToJsonString(_logOptions)}");
                                }

                                success = true;
                            }
                            catch (JsonException ex)
                            {
                                Console.WriteLine($"Deserialization failed on attempt {attempt}: {ex.Message}");

                                if (attempt == maxRetries)
                                {
                                    // Log and handle failure after all retries
                                    error = $"Deserialization failed after {maxRetries} attempts: {ex.Message}";
                                    customer = null;
                                }

                                await Task.Delay(50); // Optional short delay between retries
                            }
                        }


                        if (customer is not null && cardList is not null)
                        {
                            Random random = new Random();

                            int index = random.Next(0, cardList.Count);
                            sampleCard = cardList[index];

                            var sampleCustomer = new B2cCustomer
                            {
                                FirstName = customer.FirstName,
                                LastName = customer.LastName,
                                FullName = customer.FirstName + " " + customer.LastName,
                                Email = customer.Email,
                                Address1 = customer.Address1,
                                Address2 = customer.Address2,
                                AdministrativeArea = customer.Region,
                                City = customer.City,
                                PostalCode = customer.PostalCode,
                                Country = customer.Country,
                                Phone = customer.Phone,
                                SaveFormData = true,
                                ShippingSameAsBilling = true,
                                AccountNumber = sampleCard.AccountNumber,
                                ExpMonth = sampleCard.ExpMonth,
                                ExpYear = sampleCard.ExpYear,
                                Cvv = sampleCard.Cvv
                            };

                            return sampleCustomer!;
                        }
                        else
                        {
                            if (customer is null)
                            {
                                Console.WriteLine("INSIDE GeneralUtilities.PopulateBilling() CUSTOMER OBJECT IS NULL!!!!");
                                error = "Error: " + "GeneralUtilities.PopulateBilling() CUSTOMER OBJECT IS NULL";
                            }
                            else if (cardList is null)
                            {
                                Console.WriteLine("INSIDE GeneralUtilities.PopulateBilling() SAMPLE CARDS OBJECT IS NULL!!!!");
                                error = "Error: " + "GeneralUtilities.PopulateBilling() SAMPLE CARDS OBJECT IS NULL";
                            }
                            else
                            {
                                error = "Error: " + "UNKNOWN ISSUE: No or bad response from server";
                            }

                            sessionResponse!.error = error;
                            sessionTransactions.AddTrans(sessionResponse);
                            error = sessionResponse!.error;
                            var sampleCustomer = new B2cCustomer();
                            sampleCustomer.Error = error;
                            return sampleCustomer;
                        }
                    }
                    else if (sessionResponse != null && sessionResponse.TransactionJson != null && sessionResponse.JsonTransactionStateValues == TransactionStateValues.Error)
                    {
                        var jsonNode = sessionResponse.TransactionJson;
                        sessionTransactions.AddTrans(sessionResponse);
                        error = sessionResponse!.error;
                        var sampleCustomer = new B2cCustomer(); // Fixed instantiation
                        sampleCustomer.Error = "Error: " + sessionResponse.TransactionJson.ToString();
                        return sampleCustomer;
                    }
                    else
                    {
                        error = "Error: " + "No or bad response from server";
                        var sampleCustomer = new B2cCustomer();
                        sampleCustomer.Error = error;
                        return sampleCustomer;
                    }
                }
                catch (Exception ex)
                {
                    error = "Error: " + ex.Message;
                    SessionTransJson sessionResponse = new();
                    sessionResponse!.error = error;
                    sessionTransactions.AddTrans(sessionResponse);

                    var sampleCustomer = new B2cCustomer();
                    sampleCustomer.Error = error;
                    return sampleCustomer;
                }
            }
            else
            {
                error = "Error: " + "No or bad response from server";
                var sampleCustomer = new B2cCustomer();
                sampleCustomer.Error = error;
                return sampleCustomer;
            }
        }

        /// <summary>
        /// "Use Defaults" for the eCheck pages. Draws the same random customer the card checkout
        /// draws — by calling PopulateBilling itself, so the billing half is provably identical
        /// rather than a parallel copy that can drift — then discards the card it came back with
        /// and fills bank-account fields from a random seeded ECheckTestAccount instead.
        /// </summary>
        /// <param name="cardList">
        /// Only used to satisfy PopulateBilling's random-card draw. The card is thrown away.
        /// </param>
        /// <param name="includeBankAccount">
        /// False on the token-debit page, which collects no bank account — the stored token
        /// supplies it — and therefore wants billing and cart defaults only.
        /// </param>
        public static async Task<B2cCustomer> PopulateBillingECheck(
            List<PayerAuthCardSampleDto> cardList, bool includeBankAccount)
        {
            B2cCustomer customer = await PopulateBilling(cardList);

            if (customer is null || customer.Error is not null)
            {
                return customer ?? new B2cCustomer { Error = "Error: PopulateBillingECheck returned no customer" };
            }

            // eCheck is not a card transaction. Leaving these populated would put a PAN on a
            // bank-account form and, worse, ship it to the server on submit.
            customer.AccountNumber = null;
            customer.ExpMonth = null;
            customer.ExpYear = null;
            customer.Cvv = null;
            customer.CardType = null;

            customer.IsECheck = true;

            if (!includeBankAccount) return customer;

            ApiResult<List<ECheckTestAccountDto>> result = await CallMinAPIs.GetECheckTestAccountsAsync();

            if (result.Error is not null)
            {
                // Billing and cart are already populated and every bank field is hand-editable,
                // so this degrades the convenience rather than the page. Surfaced, not swallowed.
                Console.WriteLine($"[PopulateBillingECheck] Test account lookup failed: {result.Error.Message}");
                customer.Error = "Error: could not load eCheck test accounts - "
                                 + (result.Error.Message ?? result.Error.Error);
                return customer;
            }

            List<ECheckTestAccountDto> accounts = result.Data ?? new List<ECheckTestAccountDto>();

            if (accounts.Count == 0)
            {
                Console.WriteLine("[PopulateBillingECheck] No eCheck test accounts are seeded.");
                customer.Error = "Error: no eCheck test accounts are seeded - run the eCheck DDL";
                return customer;
            }

            Random random = new Random();
            ECheckTestAccountDto account = accounts[random.Next(0, accounts.Count)];

            customer.RoutingNumber = account.RoutingNumber;
            customer.BankAccountNumber = account.AccountNumber;
            customer.BankAccountType = account.AccountType;
            customer.SecCode = account.SecCode;
            customer.BankName = account.BankName;

            Console.WriteLine($"[PopulateBillingECheck] Drew test account: {account.DisplayLabel}");

            return customer;
        }

        public static async Task<List<SampleInvoiceDetailDto>> GetSampleInvoices()
        {
            JsonSerializerOptions jsonOptions = new()
            {
                PropertyNameCaseInsensitive = true
            };

            currentTransaction = new CcTransactionTypes();
            currentTransaction = CcTransactionTypes.SAMPLE_INVOICE;

            string input = "{}";

            try
            {
                SessionTransJson sessionResponse = await CallMinAPIs.SubmitForFollowOn(input,
                    sessionTransactions, currentTransaction);

                if (sessionResponse != null && sessionResponse.TransactionJson != null && sessionResponse.JsonTransactionStateValues == TransactionStateValues.Complete)
                {
                    var jsonNode = sessionResponse.TransactionJson;
                    sampleInvoiceDetailDtos = JsonSerializer.Deserialize<List<SampleInvoiceDetailDto>>(jsonNode); // Fixed instantiation
                    if (sampleInvoiceDetailDtos is not null)
                    {
                        return sampleInvoiceDetailDtos.ToList();
                    }
                    else
                    {
                        sessionResponse!.error = "Error: " + "Empty Object Returned";
                        sessionTransactions.AddTrans(sessionResponse);
                        error = sessionResponse!.error;
                        SampleInvoiceDetailDto sampleInvoiceDetailDto = new();
                        sampleInvoiceDetailDtos = new List<SampleInvoiceDetailDto>(); // Fixed instantiation
                        sampleInvoiceDetailDto.Error = error;
                        sampleInvoiceDetailDtos.Add(sampleInvoiceDetailDto);
                        return sampleInvoiceDetailDtos.ToList();
                    }

                }
                else if (sessionResponse != null && sessionResponse.TransactionJson != null && sessionResponse.JsonTransactionStateValues == TransactionStateValues.Error)
                {
                    var jsonNode = sessionResponse.TransactionJson;
                    sessionResponse!.error = "Error: Unexpected Error";
                    sessionTransactions.AddTrans(sessionResponse);
                    SampleInvoiceDetailDto sampleInvoiceDetailDto = new();
                    sampleInvoiceDetailDtos = new List<SampleInvoiceDetailDto>(); // Fixed instantiation
                    sampleInvoiceDetailDto.Error = "Error: " + sessionResponse.TransactionStatus;
                    sampleInvoiceDetailDtos.Add(sampleInvoiceDetailDto);
                    return sampleInvoiceDetailDtos.ToList();
                }
                else
                {
                    error = "Error: " + "No or bad response from server";
                    sessionResponse!.error = error;
                    sessionTransactions.AddTrans(sessionResponse);
                    SampleInvoiceDetailDto sampleInvoiceDetailDto = new();
                    sampleInvoiceDetailDtos = new List<SampleInvoiceDetailDto>(); // Fixed instantiation
                    sampleInvoiceDetailDto.Error = error;
                    sampleInvoiceDetailDtos.Add(sampleInvoiceDetailDto);
                    return sampleInvoiceDetailDtos.ToList();
                }
            }
            catch (Exception ex)
            {
                error = "Error: " + ex.Message;
                SessionTransJson sessionResponse = new();
                sessionResponse!.error = error;
                sessionTransactions.AddTrans(sessionResponse);

                // Return an empty list with the error message
                SampleInvoiceDetailDto sampleInvoiceDetailDto = new();
                sampleInvoiceDetailDtos = new List<SampleInvoiceDetailDto>(); // Fixed instantiation
                sampleInvoiceDetailDto.Error = error;
                sampleInvoiceDetailDtos.Add(sampleInvoiceDetailDto);
                return [.. sampleInvoiceDetailDtos];
            }
        }

        public static async Task<DBRandomCart> PopulateCart()
        {
            DBProduct[]? productsArray = null;
            DBCartService = new();

            JsonSerializerOptions jsonOptions = new()
            {
                PropertyNameCaseInsensitive = true
            };

            currentTransaction = new CcTransactionTypes();
            currentTransaction = CcTransactionTypes.SAMPLE_CART;

            string input = "{}";

            try
            {
                SessionTransJson sessionResponse = await CallMinAPIs.SubmitForFollowOn(input,
                    sessionTransactions, currentTransaction);

                if (sessionResponse != null && sessionResponse.TransactionJson != null && sessionResponse.JsonTransactionStateValues == TransactionStateValues.Complete)
                {
                    var jsonNode = sessionResponse.TransactionJson;
                    productsArray = JsonSerializer.Deserialize<DBProduct[]?>(jsonNode);

                    if (productsArray is not null)
                    {
                        decimal? total = 0;
                        foreach (DBProduct p in productsArray)
                        {

                            DBCartService?.cart?.Add(p);
                            total += p.UnitPrice;

                        }
                        DBCartService!.TotalPrice = total;

                        return DBCartService!;
                    }
                    else
                    {
                        sessionResponse!.error = "Error: " + sessionResponse.TransactionStatus;
                        sessionTransactions.AddTrans(sessionResponse);
                        error = "Error: " + "No or bad response from server";
                        DBCartService = new();
                        DBCartService.error = error;
                        return DBCartService;
                    }

                }
                else if (sessionResponse != null && sessionResponse.TransactionJson != null && sessionResponse.JsonTransactionStateValues == TransactionStateValues.Error)
                {
                    var jsonNode = sessionResponse.TransactionJson;
                    sessionResponse!.error = "Error: " + sessionResponse.TransactionStatus;
                    sessionTransactions.AddTrans(sessionResponse);
                    DBCartService.error = "Error: " + sessionResponse.TransactionStatus;
                    DBCartService = new();
                    return DBCartService;
                }
                else
                {
                    error = "Error: " + "No or bad response from server";
                    sessionResponse!.error = error;
                    sessionTransactions.AddTrans(sessionResponse);
                    DBCartService = new();
                    DBCartService.error = error;
                    return DBCartService;
                }

            }
            catch (Exception ex)
            {
                error = "Error: " + ex.Message;
                SessionTransJson sessionResponse = new();
                sessionResponse!.error = error;
                sessionTransactions.AddTrans(sessionResponse);

                DBCartService = new();
                DBCartService.error = error;
                return DBCartService;
            }
        }

        public static async Task<MerchantModelDto> PopulateSampleMerchant()
        {
            merchantModel = new();
            merchantSampleDatum = new();

            string input = "{}";

            currentTransaction = new CcTransactionTypes();
            currentTransaction = CcTransactionTypes.SINGLE_SAMPLE_MERCHANT;

            merchantModel = new();
            merchantSampleDatum = new();

            JsonSerializerOptions jsonOptions = new()
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                SessionTransJson sessionResponse = await CallMinAPIs.SubmitForFollowOn(input,
                    sessionTransactions, currentTransaction);

                if (sessionResponse != null && sessionResponse.TransactionJson != null && sessionResponse.JsonTransactionStateValues == TransactionStateValues.Complete)
                {
                    var jsonNode = sessionResponse.TransactionJson;
                    Console.WriteLine(jsonNode?.ToJsonString(_logOptions));
                    merchantSampleDatum = JsonSerializer.Deserialize<DBMerchantSampleDatum>(jsonNode); // Fixed instantiation
                    if (merchantSampleDatum is not null)
                    {
                        merchantModel!.OrganizationId = merchantSampleDatum.OrganizationId;
                        merchantModel!.Status = merchantSampleDatum.Status;
                        merchantModel!.Type = merchantSampleDatum.Type;
                        merchantModel!.Configurable = merchantSampleDatum.Configurable;
                        merchantModel!.Country = merchantSampleDatum.Country;
                        merchantModel!.Address1 = merchantSampleDatum.Address1;
                        merchantModel!.PostalCode = merchantSampleDatum.PostalCode;
                        merchantModel!.AdministrativeArea = merchantSampleDatum.AdministrativeArea;
                        merchantModel!.Locality = merchantSampleDatum.Locality;
                        merchantModel!.Address1 = merchantSampleDatum.Address1;
                        merchantModel!.BusinessContactFirstName = merchantSampleDatum.BusinessContactFirstName;
                        merchantModel!.BusinessContactLastName = merchantSampleDatum.BusinessContactLastName;
                        merchantModel!.BusinessContactPhoneNumber = merchantSampleDatum.BusinessContactPhoneNumber;
                        merchantModel!.BusinessContactEmail = merchantSampleDatum.BusinessContactEmail;
                        merchantModel!.TechnicalContactFirstName = merchantSampleDatum.TechnicalContactFirstName;
                        merchantModel!.TechnicalContactLastName = merchantSampleDatum.TechnicalContactLastName;
                        merchantModel!.TechnicalContactPhoneNumber = merchantSampleDatum.TechnicalContactphoneNumber;
                        merchantModel!.TechnicalContactEmail = merchantSampleDatum.TechnicalContactEmail;
                        merchantModel!.EmergencyContactFirstName = merchantSampleDatum.EmergencyContactFirstName;
                        merchantModel!.EmergencyContactLastName = merchantSampleDatum.EmergencyContactLastName;
                        merchantModel!.EmergencyContactPhoneNumber = merchantSampleDatum.EmergencyContactPhoneNumber;
                        merchantModel!.EmergencyContactEmail = merchantSampleDatum.EmergencyContactEmail;
                        merchantModel!.Name = merchantSampleDatum.Name;
                        merchantModel!.WebsiteUrl = merchantSampleDatum.WebsiteUrl;
                        merchantModel!.BusinessInformationPhoneNumber = merchantSampleDatum.BusinessInformationPhoneNumber;
                        merchantModel!.BusinessInformationTimeZone = merchantSampleDatum.BusinessInformationTimeZone;
                        merchantModel!.MerchantCategoryCode = merchantSampleDatum.MerchantCategoryCode;

                        return merchantModel!;
                    }
                    else
                    {
                        sessionResponse!.error = "Error: " + "Empty Object Returned";
                        sessionTransactions.AddTrans(sessionResponse);
                        error = sessionResponse!.error;
                        var merchantModel = new MerchantModelDto();
                        merchantModel.error = error;
                        return merchantModel;
                    }
                }
                else if (sessionResponse != null && sessionResponse.TransactionJson != null && sessionResponse.JsonTransactionStateValues == TransactionStateValues.Error)
                {
                    var jsonNode = sessionResponse.TransactionJson;
                    sessionTransactions.AddTrans(sessionResponse);
                    error = sessionResponse!.error;
                    var merchantModel = new MerchantModelDto(); // Fixed instantiation
                    merchantModel.error = "Error: " + sessionResponse.TransactionJson.ToString();
                    return merchantModel;
                }
                else
                {
                    error = "Error: " + "No or bad response from server";
                    sessionResponse!.error = error;
                    sessionTransactions.AddTrans(sessionResponse);
                    var merchantModel = new MerchantModelDto();
                    merchantModel.error = error;
                    return merchantModel;
                }

            }
            catch (Exception ex)
            {
                error = "Error: " + ex.Message;
                SessionTransJson sessionResponse = new();
                sessionResponse!.error = error;
                sessionTransactions.AddTrans(sessionResponse);

                var merchantModel = new MerchantModelDto();
                merchantModel.error = error;
                return merchantModel;
            }
        }

        //TODO Change to CallMInAPIs! --- Or get rid of this method
        public static async Task<AftRequestDto> PopulateSampleAft(bool aftOnly)
        {
            merchantSampleDatum = new();

            string input = "{}";

            currentTransaction = new CcTransactionTypes();
            currentTransaction = CcTransactionTypes.SINGLE_SAMPLE_MERCHANT;

            AftRequestDto aftRequestDto = new();
            aftRequestDto.AftOnly = aftOnly;
            aftRequestDto.SenderInformation = new SenderInformation();
            aftRequestDto.RecipientInformation = new RecipientInformation();
            aftRequestDto.MerchantInformation = new MerchantInformation();
            aftRequestDto.OrderInformation = new OrderInformation();
            aftRequestDto.OrderInformation.AmountDetails = new();
            aftRequestDto.ClientReferenceInformation = new ClientReferenceInformation();
            aftRequestDto.PaymentInformation = new PaymentInformation();
            aftRequestDto.RecipientInformation = new RecipientInformation();
            aftRequestDto.ProcessingInformation = new ProcessingInformation();
            aftRequestDto.PaymentInformation!.Card = new FullCard();
            merchantSampleDatum = new();

            JsonSerializerOptions jsonOptions = new()
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                SessionTransJson sessionResponse = await CallMinAPIs.SubmitForFollowOn(input,
                    sessionTransactions, currentTransaction);

                if (sessionResponse != null && sessionResponse.TransactionJson != null && sessionResponse.JsonTransactionStateValues == TransactionStateValues.Complete)
                {
                    var jsonNode = sessionResponse.TransactionJson;
                    Console.WriteLine(jsonNode?.ToJsonString(_logOptions));
                    merchantSampleDatum = JsonSerializer.Deserialize<DBMerchantSampleDatum>(jsonNode); // Fixed instantiation 

                    aftRequestDto.OrderInformation.AmountDetails.Currency = "USD";

                    //TODO FIGURE OUT HOW TO GET THE AMOUNT FROM THE CART
                    aftRequestDto.OrderInformation.AmountDetails.TotalAmount = "100.00";

                    if (merchantSampleDatum is not null && aftOnly)
                    {
                        aftRequestDto.SenderInformation!.FirstName = merchantSampleDatum.BusinessContactFirstName;
                        aftRequestDto.SenderInformation!.LastName = merchantSampleDatum.BusinessContactLastName;
                        aftRequestDto.SenderInformation!.Address1 = merchantSampleDatum.Address1;
                        aftRequestDto.SenderInformation!.AdministrativeArea = merchantSampleDatum.AdministrativeArea;
                        aftRequestDto.SenderInformation!.Locality = merchantSampleDatum.Locality;
                        aftRequestDto.SenderInformation!.CountryCode = merchantSampleDatum.Country;
                        aftRequestDto.OrderInformation.BillTo!.Address1 = merchantSampleDatum.Address1 ?? null;
                        aftRequestDto.OrderInformation.BillTo!.AdministrativeArea = merchantSampleDatum.AdministrativeArea ?? null;
                        aftRequestDto.OrderInformation.BillTo!.Locality = merchantSampleDatum.Locality ?? null;
                        aftRequestDto.OrderInformation.BillTo!.Country = merchantSampleDatum.Country ?? null;
                        aftRequestDto.OrderInformation.BillTo!.PostalCode = merchantSampleDatum.PostalCode ?? null;
                        aftRequestDto.OrderInformation.BillTo!.FirstName = merchantSampleDatum.BusinessContactFirstName ?? null;
                        aftRequestDto.OrderInformation.BillTo!.LastName = merchantSampleDatum.BusinessContactLastName ?? null;
                        aftRequestDto.OrderInformation.BillTo!.Email = merchantSampleDatum.BusinessContactEmail ?? null;

                    }
                    else if (merchantSampleDatum is not null)
                    {
                        aftRequestDto.SenderInformation!.FirstName = merchantSampleDatum.BusinessContactFirstName;
                        aftRequestDto.SenderInformation!.LastName = merchantSampleDatum.BusinessContactLastName;
                        aftRequestDto.SenderInformation!.Address1 = merchantSampleDatum.Address1;
                        aftRequestDto.SenderInformation!.AdministrativeArea = merchantSampleDatum.AdministrativeArea;
                        aftRequestDto.SenderInformation!.Locality = merchantSampleDatum.Locality;
                        aftRequestDto.SenderInformation!.CountryCode = merchantSampleDatum.Country;

                        aftRequestDto.RecipientInformation!.FirstName = merchantSampleDatum.BusinessContactFirstName;
                        aftRequestDto.RecipientInformation!.LastName = merchantSampleDatum.BusinessContactLastName;
                        aftRequestDto.RecipientInformation!.Address1 = merchantSampleDatum.Address1;
                        aftRequestDto.RecipientInformation!.AdministrativeArea = merchantSampleDatum.AdministrativeArea;
                        aftRequestDto.RecipientInformation!.Locality = merchantSampleDatum.Locality;
                        aftRequestDto.RecipientInformation!.CountryCode = merchantSampleDatum.Country;

                        aftRequestDto.OrderInformation.BillTo!.Address1 = merchantSampleDatum.Address1 ?? null;
                        aftRequestDto.OrderInformation.BillTo!.AdministrativeArea = merchantSampleDatum.AdministrativeArea ?? null;
                        aftRequestDto.OrderInformation.BillTo!.Locality = merchantSampleDatum.Locality ?? null;
                        aftRequestDto.OrderInformation.BillTo!.Country = merchantSampleDatum.Country ?? null;
                        aftRequestDto.OrderInformation.BillTo!.PostalCode = merchantSampleDatum.PostalCode ?? null;
                        aftRequestDto.OrderInformation.BillTo!.FirstName = merchantSampleDatum.BusinessContactFirstName ?? null;
                        aftRequestDto.OrderInformation.BillTo!.LastName = merchantSampleDatum.BusinessContactLastName ?? null;
                        aftRequestDto.OrderInformation.BillTo!.Email = merchantSampleDatum.BusinessContactEmail ?? null;
                    }
                }
                else if (sessionResponse != null && sessionResponse.TransactionJson != null && sessionResponse.JsonTransactionStateValues == TransactionStateValues.Error)
                {
                    var jsonNode = sessionResponse.TransactionJson;
                    sessionTransactions.AddTrans(sessionResponse);
                    aftRequestDto!.Error = "Error: " + sessionResponse.TransactionStatus;
                    return aftRequestDto;
                }
                else
                {
                    error = "Error: " + "No or bad response from server";
                    sessionResponse!.error = error;
                    sessionTransactions.AddTrans(sessionResponse);
                    aftRequestDto!.Error = error;
                    return aftRequestDto;
                }
            }
            catch (Exception ex)
            {
                error = "Error: " + ex.Message;
                Console.WriteLine($"{ex.GetType()}: {ex.Message}");
                SessionTransJson sessionResponse = new();
                sessionResponse!.error = error;
                sessionTransactions.AddTrans(sessionResponse);

                aftRequestDto = new AftRequestDto();
                aftRequestDto.Error = error;
                return aftRequestDto;
            }
            return aftRequestDto!;
        }
    }
}
