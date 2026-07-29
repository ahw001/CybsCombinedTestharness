using CybsClient.Model;
using CybsClient.Model.CyberStore;
using CybsClient.Model.Cybersource.Transactions;
using CybsClient.Model.OutboundObjects;
using CybsClient.Services.DIServices;
using CybsClient.Services.DTOs;
using CybsClient.Services.ErrorHandling;
using CybsClient.Model.Cybersource.BaseData;
using CybsClient.Model.Cybersource.Boarding;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CybsClient.Services.Utilities
{
    public static class CallMinAPIs
    {

        public static async Task<SessionTransJson> SubmitForFollowOn(string followOnInput, ISessionTransactions _sessionTransactions, CcTransactionTypes current)
        {

            string statusNode = string.Empty;
            string id = string.Empty;
            string orderId = string.Empty;
            string error = string.Empty;
            string amount = string.Empty;
            string combinedError = string.Empty;

            SessionTransJson sessionTransJson = new();
            B2cCustomer b2cCustomer = new B2cCustomer();
            string? transToken = null;

            var currentTransaction = current;

            if (_sessionTransactions is not null && _sessionTransactions.Transactions is not null
                && _sessionTransactions.Transactions.LastOrDefault() is not null)
            {
                sessionTransJson = _sessionTransactions.Transactions.LastOrDefault()!;
            }

            if (sessionTransJson.Customer is not null)
            {
                b2cCustomer = sessionTransJson.Customer!;
            }

            if (sessionTransJson.TransientToken is not null)
            { 
                transToken = sessionTransJson.TransientToken!;
            }


            // Safely check if followOnInput can be deserialized into B2cCustomer

            /*
            bool canDeserializeToB2cCustomer = false;
            B2cCustomer? deserializedCustomer = null;
            try
            {
                deserializedCustomer = JsonSerializer.Deserialize<B2cCustomer>(followOnInput);
                if (deserializedCustomer != null)
                {
                    canDeserializeToB2cCustomer = true;
                }
            }
            catch (JsonException)
            {
                // Invalid or incompatible JSON for B2cCustomer, do nothing
            }

            // Optionally, use the deserialized customer if needed
            if (canDeserializeToB2cCustomer) { b2cCustomer = deserializedCustomer!; }
            */

            // Parse the JSON string into a JsonNode and cast it to JsonObject
            JsonObject? jsonObject = JsonNode.Parse(followOnInput) as JsonObject;

            /*
            // Check if the conversion was successful
            if (jsonObject != null)
            {
                Console.WriteLine("Successfully parsed JSON to JsonObject.");
                //Console.WriteLine(jsonObject.ToString()); // To print the JSON string representation of the JsonObject
            }
            else
            {
                Console.WriteLine("Failed to parse JSON to JsonObject.");
            }
            */
            try
            {
                Console.WriteLine("****************** CALLING SubmitForFollowOn ******************\n");
                Console.WriteLine($"Current Transaction Type: {currentTransaction}");
                Console.WriteLine($"REQUEST JSON = {PrettyForLog(followOnInput)}");

                using HttpClient client = HttpClientHelper.CreateClient("Cybs.WebApi.Service");

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent(followOnInput, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response;

                if (currentTransaction == CcTransactionTypes.SAMPLE_CARD_LIST)
                {
                    // GET the transaction for list of sample cards *******************************

                    response = await client.GetAsync("api/samplecards/");

                    // GET the transaction for list of sample cards *******************************
                }
                else if (currentTransaction == CcTransactionTypes.SESSION_STATE_STORE)
                {
                    // Store the state of Session Transactions *******************************

                    response = await client.PostAsync("/api/session/sessionstore", content);

                    // Store the state of Session Transactions *******************************
                }
                else if (currentTransaction == CcTransactionTypes.SESSION_STATE_RETRIEVE)
                {
                    // Retrieve the customer transaction *******************************

                    string guid = followOnInput.Trim('"');

                    // If client.BaseAddress is set (as it should be when using named clients), this will give the full absolute URL:
                    string relativePath = $"/api/session/sessionretrieve/{guid}";
                    string fullUrl = client.BaseAddress != null
                        ? new Uri(client.BaseAddress, relativePath).ToString()
                        : relativePath;

                    Console.WriteLine("GET request to: " + fullUrl);

                    response = await client.GetAsync($"/api/session/sessionretrieve/{guid}");

                    // Retrieve the customer transaction *******************************
                }
                else if (currentTransaction == CcTransactionTypes.RANDOM_CUSTOMER)
                {
                    // GET the transaction for list of random customers *******************************

                    response = await client.GetAsync("api/randomcustomer/");

                    // GET the transaction for list of random customers *******************************
                }
                else if (currentTransaction == CcTransactionTypes.SAMPLE_INVOICE)
                {
                    // GET the transaction for list of sample invoice data *******************************

                    response = await client.GetAsync("/api/SampleInvoiceDetail");

                    // GET the transaction for list of sample invoice data *******************************
                }
                else if (currentTransaction == CcTransactionTypes.SAMPLE_PA_CARDS)
                {
                    // GET the transaction for list of sample payer auth cards *******************************

                    response = await client.GetAsync("api/samplecards/");

                    // GET the transaction for list of sample payer auth cards *******************************
                }
                else if (currentTransaction == CcTransactionTypes.SAMPLE_CART)
                {
                    // GET the transaction for list of sample cart items *******************************

                    response = await client.GetAsync("api/randomproducts");

                    // GET the transaction for list of sample cart items *******************************
                }
                else if (currentTransaction == CcTransactionTypes.GET_CATEGORY_LIST)
                {
                    // GET the transaction for list of sample cards *******************************

                    response = await client.GetAsync("api/Category/");

                    // GET the transaction for list of sample cards *******************************
                }
                else if (currentTransaction == CcTransactionTypes.SINGLE_SAMPLE_MERCHANT)
                {
                    // GET the transaction for a random merchant *******************************

                    response = await client.GetAsync("/api/merchantsampledatum/");

                    // GET the transaction for a random merchant *******************************
                }
                else if (currentTransaction == CcTransactionTypes.SAMPLE_AFT)
                {
                    // GET the transaction for a random aft merchant *******************************

                    response = await client.GetAsync("/api/merchantsampledatum/");

                    // GET the transaction for a random aft merchant *******************************
                }
                else if (currentTransaction == CcTransactionTypes.CREDIT)
                {
                    // POST the transaction for stand alone credit transaction *******************************

                    response = await client.PostAsync("api/standalonecredit", content);

                    // POST the transaction for stand alone credit transaction *******************************
                }
                else if (currentTransaction == CcTransactionTypes.SIMPLE_STRING)
                {
                    // POST the transaction for simple json transaction *******************************

                    response = await client.PostAsync("/api/json/processor", content);

                    // POST the transaction for simple json credit transaction *******************************
                }
                else if (currentTransaction == CcTransactionTypes.STANDALONE_AFT_TRANSACTION)
                {
                    // POST the transaction for aft pull transaction *******************************

                    response = await client.PostAsync("/api/payouts/sendaft", content);

                    // POST the transaction for aft pull transaction *******************************
                }
                else if (currentTransaction == CcTransactionTypes.TOKEN_AFT_TRANSACTION)
                {
                    // POST the transaction for aft pull transaction *******************************

                    response = await client.PostAsync("/api/payouts/sendaft", content);

                    // POST the transaction for aft pull transaction *******************************
                }
                else if (currentTransaction == CcTransactionTypes.FLEX_AFT_TRANSACTION)
                {
                    // POST the transaction for aft pull transaction using flex *******************************

                    response = await client.PostAsync("/api/payouts/sendaftflex", content);

                    // POST the transaction for aft pull transaction using flex  *******************************
                }
                else if (currentTransaction == CcTransactionTypes.FLEX_AFT_CHECK_ENROLL_AUTH)
                {
                    // POST the transaction for aft pull transaction using flex *******************************

                    response = await client.PostAsync("/api/payerauth/flexaftpacheckenroll", content);

                    // POST the transaction for aft pull transaction using flex  *******************************
                }
                else if (currentTransaction == CcTransactionTypes.FLEX_PA_SETUP)
                {
                    // POST the transaction for pa setup using flex JWT *******************************

                    response = await client.PostAsync("/api/payerauth/flexpayerauthsetup", content);

                    // POST the transaction for pa setup using flex JWT *******************************
                }
                else if (currentTransaction == CcTransactionTypes.FLEX_AFT_VALIDATE_AUTH)
                {
                    // POST the transaction for pa auth using Transient Token *******************************

                    response = await client.PostAsync("/api/payerauth/flexaftpavalidateauth", content);

                    // POST the transaction for pa auth using Transient Token *******************************
                }
                else if (currentTransaction == CcTransactionTypes.PA_ENROLL)
                {
                    // POST the transaction for pa check enroll using flex JWT *******************************

                    response = await client.PostAsync("/api/payerauth/flexpacheckenroll", content);

                    // POST the transaction for pa check enroll using flex JWT *******************************
                }
                else if (currentTransaction == CcTransactionTypes.PA_VALIDATE)
                {
                    // POST the transaction for pa validate *******************************

                    response = await client.PostAsync("/api/payerauth/flexaftpavalidate", content);

                    // POST the transaction for pa validate *******************************
                }
                else if (currentTransaction == CcTransactionTypes.SEMI_POS_SETUP)
                {
                    // POST the transaction for semi integrated pos setup *******************************

                    response = await client.PostAsync("/api/semiintpos/setup", content);

                    // POST the transaction for semi integrated pos setup *******************************
                }
                else if (currentTransaction == CcTransactionTypes.SEMI_POS_SALE)
                {
                    //POST the transaction for semi integrated pos sale ******************************

                    response = await client.PostAsync("/api/semiintpos/sale", content);

                    //POST the transaction for semi integrated pos sale *******************************
                }
                else if (currentTransaction == CcTransactionTypes.CLOUD_POS_BEARER_CREATE)
                {
                    // POST the transaction for cloud POS bearer create *******************************

                    response = await client.PostAsync("/api/cloudpos/bearer", content);

                    // POST the transaction for cloud POS bearer create *******************************
                }
                else if (currentTransaction == CcTransactionTypes.CLOUD_POS_BEARER_SALE)
                {
                    // POST the transaction for cloud pos sale *******************************

                    response = await client.PostAsync("/api/cloudpos/sale", content);

                    // POST the transaction cloud pos sale *******************************
                }
                else if (currentTransaction == CcTransactionTypes.CLOUD_POS_BEARER_RETURN || currentTransaction == CcTransactionTypes.CLOUD_POS_BEARER_STATUS_CHECK
                    || currentTransaction == CcTransactionTypes.CLOUD_POS_BEARER_STANDALONE_RETURN || currentTransaction == CcTransactionTypes.CLOUD_POS_CANCEL
                    || currentTransaction == CcTransactionTypes.CLOUD_POS_TOKEN_RETURN || currentTransaction == CcTransactionTypes.CLOUD_POS_CAPTURE)
                {
                    // POST the transaction for cloud pos sale *******************************

                    response = await client.PostAsync("/api/cloudpos/followon", content);

                    // POST the transaction cloud pos sale *******************************
                }
                else if (currentTransaction == CcTransactionTypes.FLEX_CHECKOUT)
                {
                    // POST the transaction for flex checkout *******************************

                    response = await client.PostAsync("/api/tokens/flexcapturecontext", content);

                    // POST the transaction for flex checkout *******************************
                }
                else if (currentTransaction == CcTransactionTypes.PA_SETUP)
                {
                    // POST the transaction for payer auth setup *******************************

                    response = await client.PostAsync("/api/payerauth/payerauthsetup", content);

                    // POST the transaction for payer auth setup *******************************
                }
                else if (currentTransaction == CcTransactionTypes.FLEX_CHECKOUT_PAYMENT)
                {
                    // POST the transaction for flex checkout *******************************

                    response = await client.PostAsync("/api/unified/unifiedpayment", content);

                    // POST the transaction for flex checkout *******************************
                }
                else if (currentTransaction == CcTransactionTypes.UNIFIED_CHECKOUT)
                {
                    // POST the transaction for unified checkout capture context*******************************

                    response = await client.PostAsync("/api/tokens/capturecontext", content);

                    // POST the transaction for unified  capture context *******************************
                }
                else if (currentTransaction == CcTransactionTypes.UNIFIED_CHECKOUT_V1)
                {
                    // POST the transaction for the real Unified Checkout v1 session (uc/v1/sessions) ****

                    response = await client.PostAsync("/api/tokens/v1sessioncontext", content);

                    // POST the transaction for the real Unified Checkout v1 session *******************
                }
                else if (currentTransaction == CcTransactionTypes.UNIFIED_CHECKOUT_V0_CONTEXT)
                {
                    // POST for the config-driven v0 capture context (manual transient-token vehicle) **

                    response = await client.PostAsync("/api/tokens/v0sessioncontext", content);

                    // POST for the config-driven v0 capture context ***********************************
                }
                else if (currentTransaction == CcTransactionTypes.UNIFIED_CHECKOUT_TOKEN_PAYMENT)
                {
                    // POST the manual transient-token follow-on payment (/pts/v2/payments) ************

                    response = await client.PostAsync("/api/unified/v1tokenpayment", content);

                    // POST the manual transient-token follow-on payment *******************************
                }
                else if (currentTransaction == CcTransactionTypes.UNIFIED_CHECKOUT_PAYMENT)
                {
                    // POST the transaction for unified checkout *******************************

                    response = await client.PostAsync("/api/unified/unifiedpayment", content);

                    // POST the transaction for unified checkout *******************************
                }
                else if (currentTransaction == CcTransactionTypes.TRANS_TOKEN_INFORMATION)
                {
                    // POST the transaction for transient token retrieval *******************************

                    response = await client.PostAsync("/api/unified/transtokeninfo", content);

                    // POST the transaction for transient token retrieval  *******************************
                }
                else if ((currentTransaction == CcTransactionTypes.TOKEN_CREATE) ||
                    (currentTransaction == CcTransactionTypes.INST_ID_CREATE) ||
                    (currentTransaction == CcTransactionTypes.CUST_ID_CREATE) ||
                    (currentTransaction == CcTransactionTypes.PAY_ID_CREATE)
                    )
                {
                    bool performZeroAuth = false;
                    if (jsonObject is not null)
                    {
                        JsonNode? performZeroAuthNode = jsonObject["PerformZeroAuth"];
                        if (performZeroAuthNode != null && performZeroAuthNode.GetValue<bool>())
                        {
                            performZeroAuth = performZeroAuthNode.GetValue<bool>();
                            Console.WriteLine("PerformZeroAuth: " + performZeroAuth);
                        }
                        else
                        {
                            Console.WriteLine("PerformZeroAuth property not found or is null.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Parsed JSON is not a JsonObject.");
                    }

                    if (performZeroAuth)
                    {
                        // POST for the TOKEN TRANSACTION *******************

                        response = await client.PostAsync("/api/tokens/zeroauthtoken", content);

                        // POST for the TOKEN TRANSACTION *******************
                    }
                    else
                    {
                        // POST for the INDIVIDUAL TOKEN TRANSACTION *******************

                        response = await client.PostAsync("api/tokens/combined", content);

                        // POST for the INDIVIDUAL TOKEN TRANSACTION *******************
                    }
                }
                else if (currentTransaction == CcTransactionTypes.AUTH || currentTransaction == CcTransactionTypes.SALE
                    || currentTransaction == CcTransactionTypes.PARTIAL_AUTH_BALANCE)
                {
                    // POST for the AUTH/SALE/PARTIAL_AUTH_BALANCE TRANSACTION *******************

                    response = await client.PostAsync("api/authtransaction", content);

                    // POST for the AUTH/SALE/PARTIAL_AUTH_BALANCE TRANSACTION *******************
                }
                else if (currentTransaction == CcTransactionTypes.APPLE_PAY_AUTH)
                {
                    // POST for the APPLE PAY (merchant decryption) TRANSACTION ****************
                    // Dedicated endpoint — decrypt + tokenizedCard auth happen server-side; the
                    // response shape is the same CyberSource /pts/v2/payments JSON as
                    // api/authtransaction, so the shared post-processing below (status/id/
                    // orderInformation parsing) applies unchanged.

                    response = await client.PostAsync("api/applepay/authorize", content);

                    // POST for the APPLE PAY TRANSACTION ****************
                }
                else if (currentTransaction == CcTransactionTypes.SHIPPING_ID_RETRIEVE)
                {
                    // POST the transaction for token retrieval transaction *******************************

                    response = await client.PostAsync("/api/tokens/retrieval", content);

                    // POST the transaction for token retrieval credit transaction *******************************
                }
                else if (currentTransaction == CcTransactionTypes.INVOICE_CREATE)
                {
                    // POST the transaction for invoice creation *******************************

                    response = await client.PostAsync("/api/invoice/createinvoice", content);

                    // POST the transaction for invoice creation *******************************
                }
                else if (currentTransaction == CcTransactionTypes.MERCHANT_BOARDING_CREATE)
                {
                    // POST the transaction for merchant boarding *******************************

                    response = await client.PostAsync("/api/merchantboarding/createmerchant", content);

                    // POST the transaction for merchant boarding *******************************
                }
                else if (currentTransaction == CcTransactionTypes.TRANSACTION_BOARDING_CREATE)
                {
                    // POST the transaction for transacting merchant boarding *******************************

                    response = await client.PostAsync("/api/merchantboarding/createtransmerchant", content);

                    // POST the transaction for transacting merchant boarding *******************************
                }
                else
                {
                    // POST the transaction for follow on transaction *******************************

                    response = await client.PostAsync("api/followontrans", content);

                    // POST the transaction for follow on transaction *******************************
                }

                Console.WriteLine("---------- RESPONSE HEADERS ----------");
                Console.WriteLine($"Status: {(int)response.StatusCode} {response.ReasonPhrase}");
                var responseHeaders = new Dictionary<string, string>();
                foreach (var header in response.Headers)
                {
                    var value = string.Join(", ", header.Value);
                    responseHeaders[header.Key] = value;
                    Console.WriteLine($"  {header.Key}: {value}");
                }
                foreach (var header in response.Content.Headers)
                {
                    var value = string.Join(", ", header.Value);
                    responseHeaders[header.Key] = value;
                    Console.WriteLine($"  {header.Key}: {value}");
                }
                Console.WriteLine("--------------------------------------");
                sessionTransJson.ResponseHeaders = responseHeaders;

                JsonNode? jsonResponseNode = null;
                string jsonResponse = string.Empty;

                try
                {
                    jsonResponse = await response.Content.ReadAsStringAsync();
                    jsonResponseNode = JsonNode.Parse(jsonResponse);
                }
                catch (Exception ex)
                {
                    var reason = ex is JsonException ? "Invalid JSON from server." : "Unable to read server response.";
                    Console.Error.WriteLine($"[Error] {reason} Details: {ex.Message}");
                    // jsonResponse holds the raw body whenever ReadAsStringAsync succeeded (i.e.
                    // whenever it was JsonNode.Parse that failed) — exactly the raw content needed
                    // to diagnose an "Invalid JSON from server" failure. Surface it via
                    // CybersourceJson instead of discarding it.
                    return CreateErrorResponse(reason, rawResponseBody: jsonResponse);
                }

                if ((int)response.StatusCode >= 500)
                {
                    error = response.ReasonPhrase!;
                    _sessionTransactions?.DeleteAll();
                    sessionTransJson = new();
                    sessionTransJson.TransactionStatus = error;
                    sessionTransJson.error = error;
                    sessionTransJson.ResponseHeaders = responseHeaders;
                    return sessionTransJson;
                }
                else if ((int)response.StatusCode >= 400)
                {
                    error = response.ReasonPhrase!;
                    _sessionTransactions?.DeleteAll();
                    sessionTransJson = new();
                    sessionTransJson.TransactionStatus = error;
                    sessionTransJson.error = error;
                    sessionTransJson.ResponseHeaders = responseHeaders;
                    sessionTransJson.TransactionJson = jsonResponseNode;
                    return sessionTransJson;
                }
                else if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Request was a 200 status response from PracticaleApps.\n");
                    Console.WriteLine($"JSON RESPONSE for {currentTransaction} = {jsonResponseNode?.ToJsonString(_logOptions) ?? jsonResponse}");
                    Console.WriteLine("****************** CALLING GENERIC ERROR HANDLER ******************\n");
                    var basicErrorInfos = JsonErrorExtractor.ExtractErrorObjects(jsonResponse);

                    string[] errorKeywords = AppErrorConfig.ErrorKeywords;

                    foreach (var err in basicErrorInfos)
                    {
                        Console.WriteLine($"[ErrorInfo] Status: {err.Status}, Reason: {err.Reason}, Message: {err.Message}, Action: {err.Action}");
                    }

                    bool containsError = basicErrorInfos.Any(e =>
                    {
                        // Skip error detection for excluded statuses (e.g. PARTIAL_AUTHORIZED)
                        if (AppErrorConfig.IsExcludedStatus(e.Status)) return false;

                        combinedError = string.Empty;
                        combinedError = $"{e.Status} {e.Reason} {e.Message} {e.Action}".ToLowerInvariant();
                        return errorKeywords.Any(k => combinedError.Contains(k.ToLowerInvariant()));
                    });

                    if (containsError)
                    {
                        var errorResponse = CreateErrorResponse(combinedError, jsonResponseNode);
                        errorResponse.ResponseHeaders = responseHeaders;
                        _sessionTransactions?.DeleteAll();
                        _sessionTransactions?.AddTrans(errorResponse);
                        return errorResponse;
                    }
                    else if (jsonResponse is null)
                    {
                        _sessionTransactions?.DeleteAll();
                        sessionTransJson = new();
                        error = "NULL TRANSACTION RESPONSE.";
                        sessionTransJson.TransactionStatus = error;
                        sessionTransJson.error = error;
                        sessionTransJson.ResponseHeaders = responseHeaders;
                        return sessionTransJson;
                    }
                    else
                    {
                        sessionTransJson = new();
                        sessionTransJson.ResponseHeaders = responseHeaders;

                        sessionTransJson.TransactionJson = jsonResponseNode;

                        string requestedTotal = "0";

                        if (jsonResponseNode is JsonObject jsonResponseObject)
                        {
                            statusNode = (string?)jsonResponseObject["status"] ?? "null";
                            orderId = (string?)jsonResponseObject["OrderId"] ?? "null";
                            id = (string?)jsonResponseObject["id"] ?? "null";
                            amount = (string?)jsonResponseObject["orderInformation"]?["amountDetails"]?["authorizedAmount"] ?? "0";
                            requestedTotal = (string?)jsonResponseObject["orderInformation"]?["amountDetails"]?["totalAmount"] ?? "0";
                        }

                        // Detect Partial Authorization
                        if (string.Equals(statusNode, "PARTIAL_AUTHORIZED", StringComparison.OrdinalIgnoreCase))
                        {
                            if (decimal.TryParse(requestedTotal, out decimal reqAmt) &&
                                decimal.TryParse(amount, out decimal authAmt))
                            {
                                sessionTransJson.RequestedAmount = requestedTotal;
                                sessionTransJson.RemainingBalance = (reqAmt - authAmt).ToString("F2");
                            }
                            else
                            {
                                sessionTransJson.RequestedAmount = requestedTotal;
                                sessionTransJson.RemainingBalance = "0";
                            }

                            Console.WriteLine($"*** PARTIAL AUTHORIZATION DETECTED: Requested={requestedTotal}, Authorized={amount}, Remaining={sessionTransJson.RemainingBalance} ***");
                        }

                        if (b2cCustomer is not null)
                        {
                            if (transToken is not null)
                            {
                                b2cCustomer.TransientToken = transToken;
                            }

                            sessionTransJson.Customer = b2cCustomer;

                        }

                        sessionTransJson.TransientToken = transToken;

                        sessionTransJson.TransactionType = statusNode ?? "null";
                        sessionTransJson.TransactionId = id ?? "null";
                        sessionTransJson.TransactionOrderId = orderId ?? "0";
                        sessionTransJson.TransactionAmount = amount ?? "0";
                        sessionTransJson.JsonTransactionStateValues = TransactionStateValues.Complete;
                        sessionTransJson.CurrentTransactionType = currentTransaction;
                        sessionTransJson.FollowOnTransaction = currentTransaction;
                        _sessionTransactions!.AddTrans(sessionTransJson);

                        return sessionTransJson;
                    }
                }

                Console.WriteLine("******** TRANSACTION STATE UNKNOWN \n");

                sessionTransJson.TransactionJson = jsonResponseNode;
                sessionTransJson.JsonTransactionStateValues = TransactionStateValues.Unknown;
                sessionTransJson.CurrentTransactionType = currentTransaction;
                sessionTransJson.FollowOnTransaction = currentTransaction;

                return sessionTransJson;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return CreateErrorResponse(ex.Message);
            }
        }

        private static SessionTransJson CreateErrorResponse(string message, JsonNode? responseJson = null, string? rawResponseBody = null)
        {
            ErrorObject errorObject = new ErrorObject
            {
                Id = "0",
                Error = "Error",
                Message = message,
                Reason = "An error occurred during the transaction.",
                Action = "Please check the input and try again.",
                CreatedAt = DateTime.UtcNow,
                // Prefer an explicit raw body (e.g. a non-JSON response that failed to parse);
                // otherwise pull cybersourceJson out of a successfully-parsed response node
                // (top-level, or nested under "error").
                CybersourceJson = !string.IsNullOrWhiteSpace(rawResponseBody)
                    ? rawResponseBody
                    : responseJson?["cybersourceJson"]?.GetValue<string>()
                        ?? responseJson?["error"]?["cybersourceJson"]?.GetValue<string>()
            };

            responseJson ??= JsonSerializer.SerializeToNode(errorObject, new JsonSerializerOptions { WriteIndented = true });

            return new SessionTransJson
            {
                TransactionStatus = "error",
                error = message,
                JsonTransactionStateValues = TransactionStateValues.Error,
                TransactionJson = responseJson
            };
        }

        // ── Centralized HTTP helpers ────────────────────────────────────────────
        //
        // These helpers mirror the SubmitForFollowOn gauntlet for every non-session
        // HTTP call: HTTP status tiering (5xx / 4xx / other non-2xx), JSON-parse guard,
        // embedded-error scan via JsonErrorExtractor + AppErrorConfig.ErrorKeywords, and
        // always-produce an ErrorObject that is propagated to callers as ApiResult<T>.

        private static readonly JsonSerializerOptions _defaultOptions = new(JsonSerializerDefaults.Web);

        // Console-log-only formatting — never used for actual outgoing/incoming wire payloads.
        private static readonly JsonSerializerOptions _logOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

        // Best-effort pretty-print for console logging: returns indented JSON if the input
        // parses, otherwise the original raw string unchanged (it may be non-JSON debug text).
        private static string PrettyForLog(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return raw ?? string.Empty;
            try
            {
                var node = JsonNode.Parse(raw);
                return node?.ToJsonString(_logOptions) ?? raw;
            }
            catch (JsonException)
            {
                return raw;
            }
        }

        private static Dictionary<string, string> CollectHeaders(HttpResponseMessage response)
        {
            var dict = new Dictionary<string, string>();
            foreach (var h in response.Headers) dict[h.Key] = string.Join(", ", h.Value);
            foreach (var h in response.Content.Headers) dict[h.Key] = string.Join(", ", h.Value);
            return dict;
        }

        private static JsonNode? SafeParseJsonNode(string? body)
        {
            if (string.IsNullOrWhiteSpace(body)) return null;
            try { return JsonNode.Parse(body); }
            catch (JsonException) { return null; }
        }

        /// <summary>
        /// Scans a 2xx response body for embedded error signals using the same logic as
        /// SubmitForFollowOn. Returns a combined error string if found, otherwise null.
        /// </summary>
        private static string? DetectEmbeddedError(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return null;
            try
            {
                var basicErrorInfos = JsonErrorExtractor.ExtractErrorObjects(body);
                string[] errorKeywords = AppErrorConfig.ErrorKeywords;

                foreach (var e in basicErrorInfos)
                {
                    if (AppErrorConfig.IsExcludedStatus(e.Status)) continue;
                    var combined = $"{e.Status} {e.Reason} {e.Message} {e.Action}".ToLowerInvariant();
                    if (errorKeywords.Any(k => combined.Contains(k.ToLowerInvariant())))
                    {
                        return combined.Trim();
                    }
                }
            }
            catch (JsonException) { /* malformed body — let deserialization surface the failure */ }
            return null;
        }

        /// <summary>
        /// Runs the full status/JSON/embedded-error gauntlet on an HttpResponseMessage and
        /// produces an ApiResult&lt;T&gt;. On success, deserializes the body into T (or returns
        /// the JsonNode directly when T is JsonNode, or true when T is bool).
        /// </summary>
        private static async Task<ApiResult<T>> ProcessResponseAsync<T>(
            HttpResponseMessage response, string methodName, string url)
        {
            var headers = CollectHeaders(response);
            int status = (int)response.StatusCode;

            string body;
            try
            {
                body = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[CallMinAPIs] {methodName} {url} failed reading body: {ex.Message}");
                return ApiResult<T>.Fail(
                    $"Unable to read server response: {ex.Message}",
                    "Response body read failed.",
                    "Retry the request.",
                    null, status, headers);
            }

            JsonNode? bodyNode = SafeParseJsonNode(body);
            bool parseFailed = !string.IsNullOrWhiteSpace(body) && bodyNode is null;

            if (status >= 500)
            {
                Console.Error.WriteLine($"[CallMinAPIs] {methodName} {url} server error: {status} {response.ReasonPhrase}");
                return ApiResult<T>.Fail(
                    $"Server error: HTTP {status} {response.ReasonPhrase}",
                    "Upstream server returned a 5xx status.",
                    "Retry later or contact support.",
                    bodyNode, status, headers);
            }

            if (status >= 400)
            {
                Console.Error.WriteLine($"[CallMinAPIs] {methodName} {url} client error: {status} {response.ReasonPhrase}");
                return ApiResult<T>.Fail(
                    $"Client error: HTTP {status} {response.ReasonPhrase}",
                    $"Server rejected the request with {status}.",
                    "Review the payload and try again.",
                    bodyNode, status, headers);
            }

            if (!response.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"[CallMinAPIs] {methodName} {url} unexpected status: {status} {response.ReasonPhrase}");
                return ApiResult<T>.Fail(
                    $"Unexpected HTTP {status} {response.ReasonPhrase}",
                    "Non-2xx/non-4xx/non-5xx status code.",
                    "Inspect ResponseJson and StatusCode.",
                    bodyNode, status, headers);
            }

            if (parseFailed)
            {
                Console.Error.WriteLine($"[CallMinAPIs] {methodName} {url} returned body that is not valid JSON.");
                return ApiResult<T>.Fail(
                    "Invalid JSON from server.",
                    "Response body could not be parsed as JSON.",
                    "Inspect the raw response on the server.",
                    null, status, headers);
            }

            var embedded = DetectEmbeddedError(body);
            if (embedded is not null)
            {
                Console.Error.WriteLine($"[CallMinAPIs] {methodName} {url} 2xx with embedded error: {embedded}");
                return ApiResult<T>.Fail(
                    embedded,
                    "Server returned 2xx but body contained error signals.",
                    "Inspect ResponseJson for details.",
                    bodyNode, status, headers);
            }

            if (typeof(T) == typeof(bool))
            {
                return ApiResult<T>.Success((T)(object)true, bodyNode, status, headers);
            }

            if (typeof(T) == typeof(JsonNode))
            {
                return ApiResult<T>.Success((T?)(object?)bodyNode, bodyNode, status, headers);
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return ApiResult<T>.Success(default, bodyNode, status, headers);
            }

            try
            {
                var data = JsonSerializer.Deserialize<T>(body, _defaultOptions);
                return ApiResult<T>.Success(data, bodyNode, status, headers);
            }
            catch (JsonException ex)
            {
                Console.Error.WriteLine($"[CallMinAPIs] {methodName} {url} deserialization error: {ex.Message}");
                return ApiResult<T>.Fail(
                    $"Deserialization error: {ex.Message}",
                    "Server response could not be deserialized into the expected type.",
                    "Inspect ResponseJson for the raw body.",
                    bodyNode, status, headers);
            }
        }

        private static async Task<ApiResult<T>> ExecuteGetAsync<T>(string relativeUrl)
        {
            try
            {
                using HttpClient client = HttpClientHelper.CreateClient("Cybs.WebApi.Service");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(relativeUrl);
                return await ProcessResponseAsync<T>(response, "GET", relativeUrl);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[CallMinAPIs] GET {relativeUrl} threw: {ex.Message}");
                return ApiResult<T>.Fail(ex.Message, $"Exception during GET {relativeUrl}.", "Check connectivity and retry.");
            }
        }

        private static async Task<ApiResult<TResult>> ExecuteWriteAsync<TPayload, TResult>(
            HttpMethod method, string relativeUrl, TPayload payload)
        {
            try
            {
                using HttpClient client = HttpClientHelper.CreateClient("Cybs.WebApi.Service");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var json = JsonSerializer.Serialize(payload, _defaultOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = method == HttpMethod.Put
                    ? await client.PutAsync(relativeUrl, content)
                    : method == HttpMethod.Patch
                        ? await client.PatchAsync(relativeUrl, content)
                        : await client.PostAsync(relativeUrl, content);

                return await ProcessResponseAsync<TResult>(response, method.Method, relativeUrl);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[CallMinAPIs] {method} {relativeUrl} threw: {ex.Message}");
                return ApiResult<TResult>.Fail(ex.Message, $"Exception during {method} {relativeUrl}.", "Check connectivity and retry.");
            }
        }

        private static async Task<ApiResult<bool>> ExecuteDeleteAsync(string relativeUrl)
        {
            try
            {
                using HttpClient client = HttpClientHelper.CreateClient("Cybs.WebApi.Service");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.DeleteAsync(relativeUrl);
                return await ProcessResponseAsync<bool>(response, "DELETE", relativeUrl);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[CallMinAPIs] DELETE {relativeUrl} threw: {ex.Message}");
                return ApiResult<bool>.Fail(ex.Message, $"Exception during DELETE {relativeUrl}.", "Check connectivity and retry.");
            }
        }

        // ── End Centralized HTTP helpers ────────────────────────────────────────


        // ── ElectronicProduct CRUD ──────────────────────────────────────────────

        public static Task<ApiResult<List<ElectronicProduct>>> GetElectronicProductsAsync()
            => ExecuteGetAsync<List<ElectronicProduct>>("api/ElectronicProduct");

        public static Task<ApiResult<ElectronicProduct>> GetElectronicProductByIdAsync(int id)
            => ExecuteGetAsync<ElectronicProduct>($"api/ElectronicProduct/{id}");

        public static Task<ApiResult<ElectronicProduct>> CreateElectronicProductAsync(ElectronicProduct product)
            => ExecuteWriteAsync<ElectronicProduct, ElectronicProduct>(
                HttpMethod.Post, "api/ElectronicProduct", product);

        public static Task<ApiResult<ElectronicProduct>> UpdateElectronicProductAsync(int id, ElectronicProduct product)
            => ExecuteWriteAsync<ElectronicProduct, ElectronicProduct>(
                HttpMethod.Put, $"api/ElectronicProduct/{id}", product);

        public static Task<ApiResult<bool>> DeleteElectronicProductAsync(int id)
            => ExecuteDeleteAsync($"api/ElectronicProduct/{id}");

        // ── End ElectronicProduct CRUD ─────────────────────────────────────────

        // ── Unified Checkout Configuration CRUD (Phase 0, UnifiedCheckoutPlanning.md) ───────

        public static Task<ApiResult<List<UnifiedCheckoutConfigurationDto>>> GetUnifiedCheckoutConfigsAsync()
        {
            Console.WriteLine("\n\n[UnifiedCheckoutConfig] GET /api/uc-config");
            return ExecuteGetAsync<List<UnifiedCheckoutConfigurationDto>>("/api/uc-config");
        }

        public static Task<ApiResult<UnifiedCheckoutConfigurationDto>> GetUnifiedCheckoutConfigAsync(int id)
            => ExecuteGetAsync<UnifiedCheckoutConfigurationDto>($"/api/uc-config/{id}");

        public static Task<ApiResult<UnifiedCheckoutConfigurationDto>> SaveUnifiedCheckoutConfigAsync(UnifiedCheckoutConfigurationDto dto)
        {
            Console.WriteLine($"\n\n[UnifiedCheckoutConfig] SAVE POST /api/uc-config");
            return ExecuteWriteAsync<UnifiedCheckoutConfigurationDto, UnifiedCheckoutConfigurationDto>(
                HttpMethod.Post, "/api/uc-config", dto);
        }

        public static Task<ApiResult<bool>> DeleteUnifiedCheckoutConfigAsync(int id)
            => ExecuteDeleteAsync($"/api/uc-config/{id}");

        // ── End Unified Checkout Configuration CRUD ─────────────────────────────

        // ── Boarding Data CRUD ─────────────────────────────────────────────────

        // Portfolio
        public static Task<ApiResult<List<BoardingPortfolioDto>>> GetBoardingPortfoliosAsync()
        {
            Console.WriteLine("\n\n[BoardingData] GET /api/boardingdata/portfolio/all");
            return ExecuteGetAsync<List<BoardingPortfolioDto>>("/api/boardingdata/portfolio/all");
        }

        public static Task<ApiResult<BoardingPortfolioDto>> GetBoardingPortfolioAsync(int id)
            => ExecuteGetAsync<BoardingPortfolioDto>($"/api/boardingdata/portfolio/{id}");

        public static Task<ApiResult<BoardingPortfolioDto>> SaveBoardingPortfolioAsync(BoardingPortfolioDto dto)
        {
            var method = dto.BoardingPortfolioId == 0 ? HttpMethod.Post : HttpMethod.Put;
            var url = dto.BoardingPortfolioId == 0
                ? "/api/boardingdata/portfolio"
                : $"/api/boardingdata/portfolio/{dto.BoardingPortfolioId}";
            Console.WriteLine($"\n\n[BoardingData] SAVE portfolio {method} {url}");
            return ExecuteWriteAsync<BoardingPortfolioDto, BoardingPortfolioDto>(method, url, dto);
        }

        public static Task<ApiResult<bool>> DeleteBoardingPortfolioAsync(int id)
            => ExecuteDeleteAsync($"/api/boardingdata/portfolio/{id}");

        // Organization
        public static Task<ApiResult<List<BoardingOrganizationDto>>> GetBoardingOrganizationsAsync()
        {
            Console.WriteLine("\n\n[BoardingData] GET /api/boardingdata/organizations");
            return ExecuteGetAsync<List<BoardingOrganizationDto>>("/api/boardingdata/organizations");
        }

        public static Task<ApiResult<BoardingOrganizationDto>> SaveBoardingOrganizationAsync(BoardingOrganizationDto dto)
        {
            var method = dto.BoardingOrganizationId == 0 ? HttpMethod.Post : HttpMethod.Put;
            var url = dto.BoardingOrganizationId == 0
                ? "/api/boardingdata/organization"
                : $"/api/boardingdata/organization/{dto.BoardingOrganizationId}";
            Console.WriteLine($"\n\n[BoardingData] SAVE org {method} {url}");
            return ExecuteWriteAsync<BoardingOrganizationDto, BoardingOrganizationDto>(method, url, dto);
        }

        public static Task<ApiResult<bool>> DeleteBoardingOrganizationAsync(int id)
            => ExecuteDeleteAsync($"/api/boardingdata/organization/{id}");

        // Transacting Merchant
        public static Task<ApiResult<List<BoardingTransactingMerchantDto>>> GetBoardingTransactingMerchantsAsync()
        {
            Console.WriteLine("\n\n[BoardingData] GET /api/boardingdata/transactingmerchants");
            return ExecuteGetAsync<List<BoardingTransactingMerchantDto>>("/api/boardingdata/transactingmerchants");
        }

        public static Task<ApiResult<List<BoardingTransactingMerchantDto>>> GetBoardingTransactingMerchantsByOrgAsync(int orgId)
            => ExecuteGetAsync<List<BoardingTransactingMerchantDto>>(
                $"/api/boardingdata/transactingmerchants/byorg/{orgId}");

        public static Task<ApiResult<BoardingTransactingMerchantDto>> SaveBoardingTransactingMerchantAsync(BoardingTransactingMerchantDto dto)
        {
            var method = dto.BoardingTransactingMerchantId == 0 ? HttpMethod.Post : HttpMethod.Put;
            var url = dto.BoardingTransactingMerchantId == 0
                ? "/api/boardingdata/transactingmerchant"
                : $"/api/boardingdata/transactingmerchant/{dto.BoardingTransactingMerchantId}";
            Console.WriteLine($"\n\n[BoardingData] SAVE transacting {method} {url}");
            return ExecuteWriteAsync<BoardingTransactingMerchantDto, BoardingTransactingMerchantDto>(method, url, dto);
        }

        public static Task<ApiResult<bool>> DeleteBoardingTransactingMerchantAsync(int id)
            => ExecuteDeleteAsync($"/api/boardingdata/transactingmerchant/{id}");

        // Card Product Subscriptions
        public static Task<ApiResult<List<BoardingCardProductSubscriptionDto>>> GetAllCardProductSubscriptionsAsync()
        {
            Console.WriteLine("\n\n[BoardingData] GET /api/boardingdata/cardproductsubscriptions");
            return ExecuteGetAsync<List<BoardingCardProductSubscriptionDto>>("/api/boardingdata/cardproductsubscriptions");
        }

        public static Task<ApiResult<List<BoardingCardProductSubscriptionDto>>> GetCardProductSubscriptionsByTransactingAsync(int transactingMerchantId)
            => ExecuteGetAsync<List<BoardingCardProductSubscriptionDto>>(
                $"/api/boardingdata/cardproductsubscriptions/bytransacting/{transactingMerchantId}");

        public static Task<ApiResult<BoardingCardProductSubscriptionDto>> SaveCardProductSubscriptionAsync(BoardingCardProductSubscriptionDto dto)
        {
            Console.WriteLine($"\n\n[BoardingData] SAVE cardproductsubscription POST /api/boardingdata/cardproductsubscription");
            return ExecuteWriteAsync<BoardingCardProductSubscriptionDto, BoardingCardProductSubscriptionDto>(
                HttpMethod.Post, "/api/boardingdata/cardproductsubscription", dto);
        }

        public static Task<ApiResult<BoardingCardProductSubscriptionDto>> UpdateCardProductSubscriptionAsync(int id, BoardingCardProductSubscriptionDto dto)
        {
            Console.WriteLine($"\n\n[BoardingData] UPDATE cardproductsubscription PUT /api/boardingdata/cardproductsubscription/{id}");
            return ExecuteWriteAsync<BoardingCardProductSubscriptionDto, BoardingCardProductSubscriptionDto>(
                HttpMethod.Put, $"/api/boardingdata/cardproductsubscription/{id}", dto);
        }

        public static Task<ApiResult<bool>> DeleteCardProductSubscriptionAsync(int id)
            => ExecuteDeleteAsync($"/api/boardingdata/cardproductsubscription/{id}");

        public static Task<ApiResult<BoardingCardProductSubscriptionDto>> CloneCardProductSubscriptionAsync(int sourceSubscriptionId)
        {
            Console.WriteLine($"\n\n[BoardingData] CLONE subscription POST /api/boardingdata/clonesubscription/{sourceSubscriptionId}");
            return ExecuteWriteAsync<object, BoardingCardProductSubscriptionDto>(
                HttpMethod.Post, $"/api/boardingdata/clonesubscription/{sourceSubscriptionId}", new { });
        }

        // Transacting Merchant ↔ Subscription Junction
        public static Task<ApiResult<BoardingTransactingMerchantSubscriptionDto>> LinkSubscriptionToMerchantAsync(BoardingTransactingMerchantSubscriptionDto dto)
        {
            Console.WriteLine($"\n\n[BoardingData] LINK subscription POST /api/boardingdata/transactingmerchantsubscription");
            return ExecuteWriteAsync<BoardingTransactingMerchantSubscriptionDto, BoardingTransactingMerchantSubscriptionDto>(
                HttpMethod.Post, "/api/boardingdata/transactingmerchantsubscription", dto);
        }

        public static Task<ApiResult<bool>> UnlinkSubscriptionFromMerchantAsync(int boardingTransactingMerchantSubscriptionId)
            => ExecuteDeleteAsync(
                $"/api/boardingdata/transactingmerchantsubscription/{boardingTransactingMerchantSubscriptionId}");

        public static Task<ApiResult<BoardingProcessorConfigDto>> SaveProcessorConfigAsync(BoardingProcessorConfigDto dto)
        {
            Console.WriteLine($"\n\n[BoardingData] SAVE processorconfig POST /api/boardingdata/processorconfig");
            return ExecuteWriteAsync<BoardingProcessorConfigDto, BoardingProcessorConfigDto>(
                HttpMethod.Post, "/api/boardingdata/processorconfig", dto);
        }

        // ── Supplemental (non-card) product subscriptions ──────────────────────

        // Digital Payments
        public static Task<ApiResult<List<BoardingDigitalPaymentsSubscriptionDto>>> GetAllDigitalPaymentsAsync()
            => ExecuteGetAsync<List<BoardingDigitalPaymentsSubscriptionDto>>("/api/boardingdata/digitalpayments/all");
        public static Task<ApiResult<BoardingDigitalPaymentsSubscriptionDto>> SaveDigitalPaymentsAsync(BoardingDigitalPaymentsSubscriptionDto dto)
            => ExecuteWriteAsync<BoardingDigitalPaymentsSubscriptionDto, BoardingDigitalPaymentsSubscriptionDto>(
                dto.BoardingDigitalPaymentsSubscriptionId == 0 ? HttpMethod.Post : HttpMethod.Put,
                dto.BoardingDigitalPaymentsSubscriptionId == 0 ? "/api/boardingdata/digitalpayments"
                    : $"/api/boardingdata/digitalpayments/{dto.BoardingDigitalPaymentsSubscriptionId}", dto);
        public static Task<ApiResult<bool>> DeleteDigitalPaymentsAsync(int id)
            => ExecuteDeleteAsync($"/api/boardingdata/digitalpayments/{id}");

        // Invoicing
        public static Task<ApiResult<List<BoardingInvoicingSubscriptionDto>>> GetAllInvoicingAsync()
            => ExecuteGetAsync<List<BoardingInvoicingSubscriptionDto>>("/api/boardingdata/invoicing/all");
        public static Task<ApiResult<BoardingInvoicingSubscriptionDto>> SaveInvoicingAsync(BoardingInvoicingSubscriptionDto dto)
            => ExecuteWriteAsync<BoardingInvoicingSubscriptionDto, BoardingInvoicingSubscriptionDto>(
                dto.BoardingInvoicingSubscriptionId == 0 ? HttpMethod.Post : HttpMethod.Put,
                dto.BoardingInvoicingSubscriptionId == 0 ? "/api/boardingdata/invoicing"
                    : $"/api/boardingdata/invoicing/{dto.BoardingInvoicingSubscriptionId}", dto);
        public static Task<ApiResult<bool>> DeleteInvoicingAsync(int id)
            => ExecuteDeleteAsync($"/api/boardingdata/invoicing/{id}");

        // Pay By Link
        public static Task<ApiResult<List<BoardingPayByLinkSubscriptionDto>>> GetAllPayByLinkAsync()
            => ExecuteGetAsync<List<BoardingPayByLinkSubscriptionDto>>("/api/boardingdata/paybylink/all");
        public static Task<ApiResult<BoardingPayByLinkSubscriptionDto>> SavePayByLinkAsync(BoardingPayByLinkSubscriptionDto dto)
            => ExecuteWriteAsync<BoardingPayByLinkSubscriptionDto, BoardingPayByLinkSubscriptionDto>(
                dto.BoardingPayByLinkSubscriptionId == 0 ? HttpMethod.Post : HttpMethod.Put,
                dto.BoardingPayByLinkSubscriptionId == 0 ? "/api/boardingdata/paybylink"
                    : $"/api/boardingdata/paybylink/{dto.BoardingPayByLinkSubscriptionId}", dto);
        public static Task<ApiResult<bool>> DeletePayByLinkAsync(int id)
            => ExecuteDeleteAsync($"/api/boardingdata/paybylink/{id}");

        // Token Management
        public static Task<ApiResult<List<BoardingTokenManagementSubscriptionDto>>> GetAllTokenManagementAsync()
            => ExecuteGetAsync<List<BoardingTokenManagementSubscriptionDto>>("/api/boardingdata/tokenmanagement/all");
        public static Task<ApiResult<BoardingTokenManagementSubscriptionDto>> SaveTokenManagementAsync(BoardingTokenManagementSubscriptionDto dto)
            => ExecuteWriteAsync<BoardingTokenManagementSubscriptionDto, BoardingTokenManagementSubscriptionDto>(
                dto.BoardingTokenManagementSubscriptionId == 0 ? HttpMethod.Post : HttpMethod.Put,
                dto.BoardingTokenManagementSubscriptionId == 0 ? "/api/boardingdata/tokenmanagement"
                    : $"/api/boardingdata/tokenmanagement/{dto.BoardingTokenManagementSubscriptionId}", dto);
        public static Task<ApiResult<bool>> DeleteTokenManagementAsync(int id)
            => ExecuteDeleteAsync($"/api/boardingdata/tokenmanagement/{id}");
        public static Task<ApiResult<System.Text.Json.Nodes.JsonNode>> SubmitNtTmsToCybersourceAsync(int boardingTransactingMerchantId)
            => ExecuteWriteAsync<object, System.Text.Json.Nodes.JsonNode>(
                HttpMethod.Post, $"/api/merchantboarding/submitnttmsfromsql/{boardingTransactingMerchantId}", new { });
        public static Task<ApiResult<BoardingTokenManagementSubscriptionDto>> GetOrCreateTokenManagementForMerchantAsync(int boardingTransactingMerchantId)
            => ExecuteWriteAsync<object, BoardingTokenManagementSubscriptionDto>(
                HttpMethod.Post, $"/api/merchantboarding/getorcreatenttms/{boardingTransactingMerchantId}", new { });

        // Unified Checkout
        public static Task<ApiResult<List<BoardingUnifiedCheckoutSubscriptionDto>>> GetAllUnifiedCheckoutAsync()
            => ExecuteGetAsync<List<BoardingUnifiedCheckoutSubscriptionDto>>("/api/boardingdata/unifiedcheckout/all");
        public static Task<ApiResult<BoardingUnifiedCheckoutSubscriptionDto>> SaveUnifiedCheckoutAsync(BoardingUnifiedCheckoutSubscriptionDto dto)
            => ExecuteWriteAsync<BoardingUnifiedCheckoutSubscriptionDto, BoardingUnifiedCheckoutSubscriptionDto>(
                dto.BoardingUnifiedCheckoutSubscriptionId == 0 ? HttpMethod.Post : HttpMethod.Put,
                dto.BoardingUnifiedCheckoutSubscriptionId == 0 ? "/api/boardingdata/unifiedcheckout"
                    : $"/api/boardingdata/unifiedcheckout/{dto.BoardingUnifiedCheckoutSubscriptionId}", dto);
        public static Task<ApiResult<bool>> DeleteUnifiedCheckoutAsync(int id)
            => ExecuteDeleteAsync($"/api/boardingdata/unifiedcheckout/{id}");

        // Value Added Services
        public static Task<ApiResult<List<BoardingValueAddedServicesSubscriptionDto>>> GetAllValueAddedServicesAsync()
            => ExecuteGetAsync<List<BoardingValueAddedServicesSubscriptionDto>>("/api/boardingdata/valueaddedservices/all");
        public static Task<ApiResult<BoardingValueAddedServicesSubscriptionDto>> SaveValueAddedServicesAsync(BoardingValueAddedServicesSubscriptionDto dto)
            => ExecuteWriteAsync<BoardingValueAddedServicesSubscriptionDto, BoardingValueAddedServicesSubscriptionDto>(
                dto.BoardingValueAddedServicesSubscriptionId == 0 ? HttpMethod.Post : HttpMethod.Put,
                dto.BoardingValueAddedServicesSubscriptionId == 0 ? "/api/boardingdata/valueaddedservices"
                    : $"/api/boardingdata/valueaddedservices/{dto.BoardingValueAddedServicesSubscriptionId}", dto);
        public static Task<ApiResult<bool>> DeleteValueAddedServicesAsync(int id)
            => ExecuteDeleteAsync($"/api/boardingdata/valueaddedservices/{id}");

        // Payer Authentication
        public static Task<ApiResult<List<BoardingPayerAuthenticationSubscriptionDto>>> GetAllPayerAuthenticationAsync()
            => ExecuteGetAsync<List<BoardingPayerAuthenticationSubscriptionDto>>("/api/boardingdata/payerauthentication/all");
        public static Task<ApiResult<BoardingPayerAuthenticationSubscriptionDto>> GetPayerAuthenticationByIdAsync(int id)
            => ExecuteGetAsync<BoardingPayerAuthenticationSubscriptionDto>($"/api/boardingdata/payerauthentication/{id}");
        public static Task<ApiResult<BoardingPayerAuthenticationSubscriptionDto>> SavePayerAuthenticationAsync(BoardingPayerAuthenticationSubscriptionDto dto)
            => ExecuteWriteAsync<BoardingPayerAuthenticationSubscriptionDto, BoardingPayerAuthenticationSubscriptionDto>(
                dto.BoardingPayerAuthenticationSubscriptionId == 0 ? HttpMethod.Post : HttpMethod.Put,
                dto.BoardingPayerAuthenticationSubscriptionId == 0 ? "/api/boardingdata/payerauthentication"
                    : $"/api/boardingdata/payerauthentication/{dto.BoardingPayerAuthenticationSubscriptionId}", dto);
        public static Task<ApiResult<bool>> DeletePayerAuthenticationAsync(int id)
            => ExecuteDeleteAsync($"/api/boardingdata/payerauthentication/{id}");

        // Virtual Terminal
        public static Task<ApiResult<List<BoardingVirtualTerminalSubscriptionDto>>> GetAllVirtualTerminalAsync()
            => ExecuteGetAsync<List<BoardingVirtualTerminalSubscriptionDto>>("/api/boardingdata/virtualterminal/all");
        public static Task<ApiResult<BoardingVirtualTerminalSubscriptionDto>> GetVirtualTerminalByIdAsync(int id)
            => ExecuteGetAsync<BoardingVirtualTerminalSubscriptionDto>($"/api/boardingdata/virtualterminal/{id}");
        public static Task<ApiResult<BoardingVirtualTerminalSubscriptionDto>> SaveVirtualTerminalAsync(BoardingVirtualTerminalSubscriptionDto dto)
            => ExecuteWriteAsync<BoardingVirtualTerminalSubscriptionDto, BoardingVirtualTerminalSubscriptionDto>(
                dto.BoardingVirtualTerminalSubscriptionId == 0 ? HttpMethod.Post : HttpMethod.Put,
                dto.BoardingVirtualTerminalSubscriptionId == 0 ? "/api/boardingdata/virtualterminal"
                    : $"/api/boardingdata/virtualterminal/{dto.BoardingVirtualTerminalSubscriptionId}", dto);
        public static Task<ApiResult<bool>> DeleteVirtualTerminalAsync(int id)
            => ExecuteDeleteAsync($"/api/boardingdata/virtualterminal/{id}");

        // Polymorphic junction
        public static Task<ApiResult<List<BoardingTransactingMerchantProductSubscriptionDto>>> GetProductLinksByMerchantAsync(int merchantId)
            => ExecuteGetAsync<List<BoardingTransactingMerchantProductSubscriptionDto>>($"/api/boardingdata/productlinks/bymerchant/{merchantId}");
        public static Task<ApiResult<BoardingTransactingMerchantProductSubscriptionDto>> LinkProductSubscriptionAsync(BoardingTransactingMerchantProductSubscriptionDto dto)
            => ExecuteWriteAsync<BoardingTransactingMerchantProductSubscriptionDto, BoardingTransactingMerchantProductSubscriptionDto>(
                HttpMethod.Post, "/api/boardingdata/productlink", dto);
        public static Task<ApiResult<bool>> UnlinkProductSubscriptionAsync(int junctionId)
            => ExecuteDeleteAsync($"/api/boardingdata/productlink/{junctionId}");

        // IncludeInBoarding flag updates
        public static Task<ApiResult<bool>> UpdateCardConfigIncludeInBoardingAsync(int junctionId, bool include)
        {
            Console.WriteLine($"\n\n[BoardingData] PATCH /api/boardingdata/transactingmerchantsubscription/{junctionId}/includeinboarding include={include}");
            return ExecuteWriteAsync<object, bool>(
                HttpMethod.Patch,
                $"/api/boardingdata/transactingmerchantsubscription/{junctionId}/includeinboarding",
                new { Include = include });
        }

        public static Task<ApiResult<bool>> UpdateProductLinkIncludeInBoardingAsync(int junctionId, bool include)
        {
            Console.WriteLine($"\n\n[BoardingData] PATCH /api/boardingdata/productlink/{junctionId}/includeinboarding include={include}");
            return ExecuteWriteAsync<object, bool>(
                HttpMethod.Patch,
                $"/api/boardingdata/productlink/{junctionId}/includeinboarding",
                new { Include = include });
        }

        // Dashboard
        public static Task<ApiResult<BoardingDashboardDto>> GetBoardingDashboardAsync()
        {
            Console.WriteLine("\n\n[BoardingData] GET /api/boardingdata/dashboard");
            return ExecuteGetAsync<BoardingDashboardDto>("/api/boardingdata/dashboard");
        }

        // Submit org-level merchant from SQL to Cybersource
        public static Task<ApiResult<JsonNode>> SubmitOrgToCybersourceAsync(int boardingOrganizationId)
        {
            Console.WriteLine($"\n\n[MerchantBoarding] POST /api/merchantboarding/submitorgfromsql/{boardingOrganizationId}");
            return ExecuteWriteAsync<object, JsonNode>(
                HttpMethod.Post,
                $"/api/merchantboarding/submitorgfromsql/{boardingOrganizationId}",
                new { });
        }

        // Submit transacting merchant from SQL to Cybersource
        public static Task<ApiResult<JsonNode>> SubmitTransactingMerchantToCybersourceAsync(int boardingTransactingMerchantId)
        {
            Console.WriteLine($"\n\n[MerchantBoarding] POST /api/merchantboarding/submittransactingfromsql/{boardingTransactingMerchantId}");
            return ExecuteWriteAsync<object, JsonNode>(
                HttpMethod.Post,
                $"/api/merchantboarding/submittransactingfromsql/{boardingTransactingMerchantId}",
                new { });
        }

        // ── End Boarding Data CRUD ─────────────────────────────────────────────

        // ── MLE Tokenize ─────────────────────────────────────────────────────────

        public static Task<ApiResult<JsonNode>> TokenizeCardAsync(B2cCustomer customer)
        {
            Console.WriteLine("\n\n[Tokenize] POST /api/tokens/tokenize");
            return ExecuteWriteAsync<B2cCustomer, JsonNode>(
                HttpMethod.Post, "/api/tokens/tokenize", customer);
        }

        public static Task<ApiResult<List<PaymentCardSampleDatumDto>>> GetNtSampleCardsAsync()
        {
            Console.WriteLine("\n\n[Tokenize] GET /api/tokens/sample-nt-cards");
            return ExecuteGetAsync<List<PaymentCardSampleDatumDto>>("/api/tokens/sample-nt-cards");
        }

        public static Task<ApiResult<List<NetworkTokenTestCardDto>>> GetNetworkTokenTestCardsAsync()
        {
            Console.WriteLine("\n\n[Tokenize] GET /api/tokens/sample-nt-cards");
            return ExecuteGetAsync<List<NetworkTokenTestCardDto>>("/api/tokens/sample-nt-cards");
        }

        // ── Payer Authentication Test Cards (full 3DS 2.x outcome matrix) ──────────

        public static Task<ApiResult<List<PayerAuthTestCardDto>>> GetPayerAuthTestCardsAsync()
        {
            Console.WriteLine("\n\n[PayerAuth] GET /api/PayerAuthCardSampleDatum");
            return ExecuteGetAsync<List<PayerAuthTestCardDto>>("/api/PayerAuthCardSampleDatum");
        }

        public static Task<ApiResult<JsonNode>> SubmitTokenizedCardAsync(TokenizedCardNetworkRequest request)
        {
            Console.WriteLine("\n\n[Tokenize] POST /api/tokens/tokenized-cards");
            return ExecuteWriteAsync<TokenizedCardNetworkRequest, JsonNode>(
                HttpMethod.Post, "/api/tokens/tokenized-cards", request);
        }

        public static Task<ApiResult<JsonNode>> SubmitTokenizedCardMleAsync(TokenizedCardNetworkRequest request)
        {
            Console.WriteLine("\n\n[Tokenize] POST /api/tokens/tokenized-cards-mle");
            return ExecuteWriteAsync<TokenizedCardNetworkRequest, JsonNode>(
                HttpMethod.Post, "/api/tokens/tokenized-cards-mle", request);
        }

        // ── Network Token Provisioning (Transient Token -> Network Token) ───────────
        // Non-payment flow: POST /tms/v2/tokenize with a Flex Microform transient token
        // JWT provisions the network token directly (step 5), then the server retrieves
        // its full details via GET /tms/v2/tokenized-cards/{id} (step 6) in the same call.

        public static Task<ApiResult<NetworkTokenProvisionResultDto>> ProvisionNetworkTokenAsync(string transientTokenJwt)
        {
            Console.WriteLine("\n\n[NetworkTokenProvision] POST /api/tokens/network-token/provision");
            return ExecuteWriteAsync<NetworkTokenProvisionRequestDto, NetworkTokenProvisionResultDto>(
                HttpMethod.Post, "/api/tokens/network-token/provision",
                new NetworkTokenProvisionRequestDto { TransientTokenJwt = transientTokenJwt });
        }

        // ── End Network Token Provisioning ────────────────────────────────────────

        // ── End MLE Tokenize ──────────────────────────────────────────────────────

        // ── Tokenize Transaction History ──────────────────────────────────────────

        public static Task<ApiResult<PagedResultDto<TokenizeTransactionDto>>> GetTokenizeTransactionsAsync(
            int pageNumber, int pageSize, string? search)
        {
            Console.WriteLine($"\n\n[TokenizeHistory] GET /api/tokens/tokenize-transactions page={pageNumber} size={pageSize}");
            return ExecuteGetAsync<PagedResultDto<TokenizeTransactionDto>>(
                $"/api/tokens/tokenize-transactions?pageNumber={pageNumber}&pageSize={pageSize}&search={Uri.EscapeDataString(search ?? "")}");
        }

        public static Task<ApiResult<TokenizeTransactionDto>> GetTokenizeTransactionByIdAsync(int id)
        {
            Console.WriteLine($"\n\n[TokenizeHistory] GET /api/tokens/tokenize-transactions/{id}");
            return ExecuteGetAsync<TokenizeTransactionDto>($"/api/tokens/tokenize-transactions/{id}");
        }

        public static Task<ApiResult<System.Text.Json.Nodes.JsonNode>> GeneratePaymentCredentialsAsync(string tokenId)
        {
            Console.WriteLine($"\n\n[TokenizeHistory] POST /api/tokens/{tokenId}/payment-credentials");
            return ExecuteWriteAsync<object, System.Text.Json.Nodes.JsonNode>(
                HttpMethod.Post,
                $"/api/tokens/{Uri.EscapeDataString(tokenId)}/payment-credentials",
                new { });
        }

        // ── End Tokenize Transaction History ─────────────────────────────────────

        public static async Task<ApiResult<JsonNode>> SubmitForTokenDecryption(string tokenStringInput)
        {
            Console.WriteLine("**************** SUBMITTING FOR TOKEN DECRYPT *********\n");

            string payloadJson;
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                payloadJson = JsonSerializer.Serialize(tokenStringInput, options);
                var parsed = JsonNode.Parse(payloadJson);
                Console.WriteLine($"REQUEST JSON = {parsed?.ToString()}");
                payloadJson = parsed?.ToString() ?? payloadJson;
            }
            catch (JsonException ex)
            {
                Console.Error.WriteLine($"[CallMinAPIs] SubmitForTokenDecryption payload serialization failed: {ex.Message}");
                return ApiResult<JsonNode>.Fail(
                    $"Invalid token payload: {ex.Message}",
                    "Could not serialize the token input.",
                    "Check the caller's input.");
            }

            try
            {
                using HttpClient client = HttpClientHelper.CreateClient("Cybs.WebApi.Service");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent(payloadJson, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("api/ntdecrypt", content);
                return await ProcessResponseAsync<JsonNode>(response, "POST", "api/ntdecrypt");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[CallMinAPIs] SubmitForTokenDecryption threw: {ex.Message}");
                return ApiResult<JsonNode>.Fail(
                    ex.Message,
                    "Exception during POST api/ntdecrypt.",
                    "Check connectivity and retry.");
            }
        }

        // ── Pay by Link ────────────────────────────────────────────────────────

        public static Task<ApiResult<PayByLinkTransactionDto>> CreatePayByLinkAsync(PayByLinkRequestDto dto)
            => ExecuteWriteAsync<PayByLinkRequestDto, PayByLinkTransactionDto>(
                HttpMethod.Post, "/api/paybylink/create", dto);

        public static Task<ApiResult<List<PayByLinkTransactionDto>>> GetAllPayByLinksAsync()
            => ExecuteGetAsync<List<PayByLinkTransactionDto>>("/api/paybylink/all");

        public static Task<ApiResult<PayByLinkTransactionDto>> CheckPayByLinkStatusAsync(int id)
            => ExecuteWriteAsync<object, PayByLinkTransactionDto>(
                HttpMethod.Post, $"/api/paybylink/checkstatus/{id}", new { });

        // ── End Pay by Link ────────────────────────────────────────────────────

        // ── Apple Pay ──────────────────────────────────────────────────────────

        // onvalidatemerchant proxy — the browser cannot call Apple's validationURL directly
        // (requires mutual TLS with the Merchant Identity Certificate, which must never reach the
        // client). Server relays the request and returns Apple's merchant session JSON.
        public static Task<ApiResult<JsonNode>> ValidateApplePayMerchantAsync(string validationUrl)
            => ExecuteWriteAsync<object, JsonNode>(
                HttpMethod.Post, "/api/applepay/validate-merchant", new { validationUrl });

        // ── End Apple Pay ──────────────────────────────────────────────────────
    }
}
