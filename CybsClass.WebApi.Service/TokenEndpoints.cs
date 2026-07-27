using Microsoft.AspNetCore.Mvc;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.EntityModels;
using ErrorObject = CybsClass.Cybersource.Models.BaseData.ErrorObject;
using CybsClass.WebApi.Service.Services.CcTransatcionProcessing;
using CybsClass.WebApi.Service.Services.DBOperations;
using CybsClass.WebApi.Service.Services.FlexUcContextProcessing;
using CybsClass.WebApi.Service.Services.TokenProcessing;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CybsClass.WebApi.Service;

public static class TokenEndpoints
{
    public static RouteGroupBuilder GroupTokens(this RouteGroupBuilder group)
    {
        group.MapPost("/retrieval", async ([FromBody] FollowOnTransDto followOnTransDto) =>
        {
            var tokens = await CallForCybsTms.TokenRetrievals(followOnTransDto);
            return Results.Ok(tokens);
        }).Produces<JsonObject>().WithName("PerformTokenRetrievals");

        group.MapPost("/capturecontext", static async ([FromBody] B2cCustomerDto b2cCustomerDto) =>
        {
            Dictionary<string, string> dbResults = new Dictionary<string, string>();

            CaptureContextDto ctxDto = new CaptureContextDto();

            string ctx = await CallForCaptureContext.RunAsyncCaptureContextCreate(b2cCustomerDto);

            if (ctx == null)
            {
                return Results.NotFound();
            }
            else
            {
                dbResults = await DBCustomerServices.InsertB2CCustomerAsync(b2cCustomerDto);

                if (dbResults is not null && dbResults.Count() > 0)
                {
                    ctxDto.B2cCustomerId = dbResults["B2cCustomerId"];
                    ctxDto.OrderId = dbResults["OrderId"];
                    ctxDto.Ctx = ctx;
                }

            }

            return Results.Ok(ctxDto);
        }).Produces<string>().WithName("CreateCaptureContext");

        // Real Unified Checkout v1 session (uc/v1/sessions, VAS.UnifiedCheckout client) — new,
        // additive endpoint for the store checkout flow (UnifiedCheckoutPlanning.md Execution
        // Goal 1). Does not touch or replace /capturecontext above, which the legacy
        // /unifiedcheckout (v0) page keeps using unchanged.
        group.MapPost("/v1sessioncontext", static async ([FromBody] B2cCustomerDto b2cCustomerDto) =>
        {
            CaptureContextDto ctxDto = new CaptureContextDto();

            string ctx = await CallForCaptureContextV1.RunAsync(b2cCustomerDto);

            if (ctx == null)
            {
                return Results.NotFound();
            }
            else
            {
                var dbResults = await DBCustomerServices.InsertB2CCustomerAsync(b2cCustomerDto);

                if (dbResults is not null && dbResults.Count() > 0)
                {
                    ctxDto.B2cCustomerId = dbResults["B2cCustomerId"];
                    ctxDto.OrderId = dbResults["OrderId"];
                    ctxDto.Ctx = ctx;
                }
            }

            return Results.Ok(ctxDto);
        }).Produces<string>().WithName("CreateUnifiedCheckoutV1Session");

        group.MapPost("/flexcapturecontext", async ([FromBody] B2cCustomerDto b2cCustomerDto) =>
        {
            CaptureContextDto ctxDto = new CaptureContextDto();

            string ctx = await CallForFlexCaptureContext.RunAsyncCaptureContextCreate(b2cCustomerDto);

            if (ctx == null)
            {
                return Results.NotFound();
            }
            else
            {
                ctxDto.Ctx = ctx;
            }

            return Results.Ok(ctxDto);
        }).Produces<string>().WithName("CreateFlexCaptureContext");

        group.MapPost("/combined", async ([FromBody] B2cCustomerDto b2cCustomerDto) =>
        {
            Dictionary<string, string> dbResult = new Dictionary<string, string>();

            try
            {
                var combinedToken = await CallCybsTokenService.CallForCybsCombinedTokenService(b2cCustomerDto);
                if (combinedToken == null)
                {
                    return Results.NotFound();
                }
                else
                {
                    b2cCustomerDto.CustomerInstrumentId = (string)combinedToken!["id"]! ?? "null";
                    if (combinedToken is not null && (!combinedToken.ToString().Contains("error",
                        StringComparison.OrdinalIgnoreCase) &&
                        !combinedToken.ToString().Contains("exception", StringComparison.OrdinalIgnoreCase)))
                    {
                        dbResult = await PersistCybsTokenData.TokenDBOps(b2cCustomerDto.B2cCustomerId, combinedToken);
                    }
                    else
                    {
                        Console.WriteLine("Error: Customer token is null or contains an error or exception. DB RESULTS ARE SKIPPED.");
                    }
                }

                var options = new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
                string jsonString = JsonSerializer.Serialize(combinedToken, options);

                Console.WriteLine("**************************************************");
                Console.WriteLine($"RESPONSE JSON BEING SENT TO CLIENT:  {jsonString}");

                return Results.Ok(combinedToken);
            }
            catch (Exception e)
            {
                string jsonString = e.Message;
                JsonObject jsonObject = new JsonObject();
                Console.WriteLine(e.Message);
                jsonString = $"{{ \"Exception\": \"{e}\" }}";
                JsonDocument jsonDocument = JsonDocument.Parse(jsonString);
                JsonElement rootElement = jsonDocument.RootElement;
                jsonObject = JsonObject.Create(rootElement)!;
                return Results.Json(jsonObject);
            }

        }).WithName("CreateCustomerToken");

        group.MapPost("/zeroauthtoken", async ([FromBody] B2cCustomerDto b2cCustomerDto) =>
        {
            Dictionary<string, string> dbResult = new Dictionary<string, string>();

            try
            {
                var zeroAuthToken = await CallForCybsAuthTokenCreate.RunAsyncJsonObject(b2cCustomerDto);
                if (zeroAuthToken == null)
                {
                    return Results.NotFound();
                }
                else
                {
                    b2cCustomerDto.CustomerInstrumentId = (string)zeroAuthToken!["id"]! ?? "null";
                    if (zeroAuthToken is not null && (!zeroAuthToken.ToString().Contains("error",
                        StringComparison.OrdinalIgnoreCase) ||
                        !zeroAuthToken.ToString().Contains("exception", StringComparison.OrdinalIgnoreCase)
                        || !zeroAuthToken.ToString().Contains("INVALID", StringComparison.OrdinalIgnoreCase)
                        || !zeroAuthToken.ToString().Contains("DECLINED", StringComparison.OrdinalIgnoreCase)))
                    {
                        dbResult = await PersistCybsTokenData.TokenDBOps(b2cCustomerDto.B2cCustomerId, zeroAuthToken);
                    }
                    else
                    {
                        Console.WriteLine("Error: Customer token is null or contains an error or exception. DB RESULTS ARE SKIPPED.");
                    }
                }

                var options = new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
                string jsonString = JsonSerializer.Serialize(zeroAuthToken, options);

                return Results.Ok(zeroAuthToken);
            }
            catch (Exception e)
            {
                string jsonString = e.Message;
                JsonObject jsonObject = new JsonObject();
                Console.WriteLine(e.Message);
                jsonString = $"{{ \"Exception\": \"{e}\" }}";
                JsonDocument jsonDocument = JsonDocument.Parse(jsonString);
                JsonElement rootElement = jsonDocument.RootElement;
                jsonObject = JsonObject.Create(rootElement)!;
                return Results.Json(jsonObject);
            }

        }).WithName("CreateZeroAuthToken");

        group.MapPost("/nettokencount", async ([FromBody] PaymentCardDto paymentCardDto) =>
        {
            Dictionary<string, string> dbResult = new Dictionary<string, string>();

            int paymentCardId = 0;
            paymentCardId = paymentCardDto.PaymentCardId;

            if (paymentCardId > 0)
            {
                try
                {
                    dbResult = await DBCybsTokenServices.GetNetworkTokenCountById(paymentCardId);
                    if (dbResult == null)
                    {
                        return Results.NotFound();
                    }
                    else
                    {
                        var options = new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
                        string jsonString = JsonSerializer.Serialize(dbResult, options);

                        return Results.Ok(jsonString);
                    }

                }
                catch (Exception ex)
                {
                    dbResult.Add("Exception: ", ex.Message);
                    return Results.Json(dbResult);
                }
            }
            else
            {
                dbResult.Add("Error", "Payment Card ID is invalid.");
                return Results.Json(dbResult);
            }

        }).WithName("GetNetTokenCountById");

        group.MapPost("/tokenize", async ([FromBody] B2cCustomerDto tokenizeDto) =>
        {
            var options = new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            Console.WriteLine($"\n[TokenizeEndpoint] INBOUND:\n{JsonSerializer.Serialize(tokenizeDto, options)}");

            try
            {
                var result = await CallForTokenize.RunAsync(tokenizeDto);

                Console.WriteLine($"\n[TokenizeEndpoint] OUTBOUND:\n{JsonSerializer.Serialize(result, options)}");

                if (result is not null && !result.ToString().Contains("error", StringComparison.OrdinalIgnoreCase)
                    && !result.ToString().Contains("Exception", StringComparison.OrdinalIgnoreCase)
                    && tokenizeDto.B2cCustomerId > 0)
                {
                    await PersistTokenizeData.PersistTokenize(tokenizeDto.B2cCustomerId, result);
                }

                return Results.Ok(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                var errorObj = new JsonObject();
                errorObj["Exception"] = e.Message;
                return Results.Ok(errorObj);
            }
        }).WithName("TokenizeCard");

        group.MapGet("/sample-nt-cards", async ([FromServices] CybsDbContext db) =>
        {
            var options = new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            Console.WriteLine("\n[TokenEndpoints] GET /api/tokens/sample-nt-cards");
            var cards = await DBNetworkTokenTestCardServices.GetAllAsync(db);
            // FOR CLAUDE - I WILL MANUALLY CONTROL THIS LOGGING, AS IT IS TOO VERBOSE FOR NORMAL OPERATION
            // Console.WriteLine($"\n[TokenEndpoints] OUTBOUND: {cards.Count} network token test cards\n{JsonSerializer.Serialize(cards, options)}");
            return Results.Ok(cards);
        }).WithName("GetSampleNtCards");

        group.MapPost("/tokenized-cards", async ([FromBody] TokenizedCardNetworkRequestDto dto) =>
        {
            var options = new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            Console.WriteLine($"\n[TokenEndpoints] POST /api/tokens/tokenized-cards INBOUND:\n{JsonSerializer.Serialize(dto, options)}");

            if (string.IsNullOrWhiteSpace(dto.CardNumber))
            {
                var errorObj = new JsonObject();
                errorObj["error"] = new JsonObject
                {
                    ["message"] = "CardNumber is required."
                };
                return Results.Ok(errorObj);
            }

            try
            {
                var result = await CallForTokenizedCards.RunAsync(dto);
                Console.WriteLine($"\n[TokenEndpoints] OUTBOUND:\n{JsonSerializer.Serialize(result, options)}");

                // Persist on success (no "id" field means error response — skip)
                if (result["id"] is not null && result["Exception"] is null && result["error"] is null)
                {
                    try
                    {
                        string? suffix = dto.CardNumber?.Length >= 4 ? dto.CardNumber[^4..] : dto.CardNumber;
                        await PersistTokenizeTransaction.InsertAsync(
                            "tokenized-cards", suffix, dto.ExpMonth, dto.ExpYear, result);
                    }
                    catch (Exception pe)
                    {
                        Console.WriteLine($"[TokenEndpoints] PersistTokenizeTransaction (non-fatal): {pe.Message}");
                    }
                }

                return Results.Ok(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                var errorObj = new JsonObject();
                errorObj["Exception"] = e.Message;
                return Results.Ok(errorObj);
            }
        }).WithName("SubmitTokenizedCard");

        group.MapPost("/tokenized-cards-mle", async ([FromBody] TokenizedCardNetworkRequestDto dto) =>
        {
            var options = new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            Console.WriteLine($"\n[TokenEndpoints] POST /api/tokens/tokenized-cards-mle INBOUND:\n{JsonSerializer.Serialize(dto, options)}");

            if (string.IsNullOrWhiteSpace(dto.CardNumber))
            {
                var errorObj = new JsonObject();
                errorObj["error"] = new JsonObject
                {
                    ["message"] = "CardNumber is required."
                };
                return Results.Ok(errorObj);
            }

            try
            {
                var result = await CallForTokenizedCardsMle.RunAsync(dto);
                Console.WriteLine($"\n[TokenEndpoints] OUTBOUND:\n{JsonSerializer.Serialize(result, options)}");

                // Persist on success (no "id" field means error response — skip)
                if (result["id"] is not null && result["Exception"] is null && result["error"] is null)
                {
                    try
                    {
                        string? suffix = dto.CardNumber?.Length >= 4 ? dto.CardNumber[^4..] : dto.CardNumber;
                        await PersistTokenizeTransaction.InsertAsync(
                            "tokenized-cards-mle", suffix, dto.ExpMonth, dto.ExpYear, result);
                    }
                    catch (Exception pe)
                    {
                        Console.WriteLine($"[TokenEndpoints] PersistTokenizeTransaction (non-fatal): {pe.Message}");
                    }
                }

                return Results.Ok(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                var errorObj = new JsonObject();
                errorObj["Exception"] = e.Message;
                return Results.Ok(errorObj);
            }
        }).WithName("SubmitTokenizedCardMle");

        // ── Network Token Provisioning (Transient Token -> Network Token) ───────────
        // Non-payment flow, per CyberSource's tms-tokenize-ii-nt-tt-intro ("Transient Token
        // Tokenization with Instrument Identifier") and tms-net-tkn-intro (retrieve) docs:
        //   Step 5: POST /tms/v2/tokenize with a Flex Microform transientTokenJwt,
        //           actionTokenTypes=["instrumentIdentifier"], and tokenInformation.type=
        //           "enrollable card" provisions BOTH an instrumentIdentifier and a
        //           tokenizedCard, returning both resources in "responses"
        //           (CallForNetworkTokenProvision).
        //   Step 6: GET /tms/v2/tokenized-cards/{tokenizedCardId} retrieves the full
        //           token details for the tokenizedCard id specifically
        //           (CallForNetworkTokenRetrieve).

        group.MapPost("/network-token/provision", async ([FromBody] NetworkTokenProvisionRequestDto dto) =>
        {
            var options = new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            Console.WriteLine($"\n[TokenEndpoints] POST /api/tokens/network-token/provision INBOUND");

            if (string.IsNullOrWhiteSpace(dto.TransientTokenJwt))
            {
                return Results.Ok(new NetworkTokenProvisionResultDto
                {
                    Error = new ErrorObject { Error = "InvalidRequest", Message = "TransientTokenJwt is required." }
                });
            }

            try
            {
                // Step 5
                var provisionResult = await CallForNetworkTokenProvision.RunAsync(dto.TransientTokenJwt);
                Console.WriteLine($"\n[TokenEndpoints] Step 5 OUTBOUND:\n{JsonSerializer.Serialize(provisionResult, options)}");

                if (provisionResult["Exception"] is not null || provisionResult["errors"] is not null || provisionResult["error"] is not null)
                {
                    return Results.Ok(new NetworkTokenProvisionResultDto
                    {
                        Error = new ErrorObject
                        {
                            Error = "NetworkTokenProvisionFailed",
                            Message = provisionResult["Exception"]?.ToString()
                                      ?? provisionResult["errors"]?[0]?["message"]?.ToString()
                                      ?? provisionResult["error"]?.ToString(),
                            CybersourceJson = provisionResult.ToJsonString()
                        }
                    });
                }

                var responsesArray = provisionResult["responses"]?.AsArray();

                string? tokenizedCardId = responsesArray?
                                               .FirstOrDefault(r => r?["resource"]?.ToString() == "tokenizedCard")?["id"]?.ToString()
                                           ?? provisionResult["tokenInformation"]?["tokenizedCard"]?["id"]?.ToString()
                                           ?? provisionResult["id"]?.ToString();

                string? instrumentIdentifierId = responsesArray?
                    .FirstOrDefault(r => r?["resource"]?.ToString() == "instrumentIdentifier")?["id"]?.ToString();

                if (string.IsNullOrWhiteSpace(tokenizedCardId))
                {
                    return Results.Ok(new NetworkTokenProvisionResultDto
                    {
                        Error = new ErrorObject
                        {
                            Error = "NetworkTokenProvisionFailed",
                            Message = "CyberSource response did not include a tokenizedCard id.",
                            CybersourceJson = provisionResult.ToJsonString()
                        }
                    });
                }

                // Step 6
                var retrieveResult = await CallForNetworkTokenRetrieve.RunAsync(tokenizedCardId);
                Console.WriteLine($"\n[TokenEndpoints] Step 6 OUTBOUND:\n{JsonSerializer.Serialize(retrieveResult, options)}");

                var result = new NetworkTokenProvisionResultDto
                {
                    TokenizedCardId = tokenizedCardId,
                    InstrumentIdentifierId = instrumentIdentifierId
                };

                if (retrieveResult["Exception"] is not null || retrieveResult["errors"] is not null || retrieveResult["error"] is not null)
                {
                    result.Error = new ErrorObject
                    {
                        Error = "NetworkTokenRetrieveFailed",
                        Message = retrieveResult["Exception"]?.ToString()
                                  ?? retrieveResult["errors"]?[0]?["message"]?.ToString()
                                  ?? retrieveResult["error"]?.ToString(),
                        CybersourceJson = retrieveResult.ToJsonString()
                    };
                    return Results.Ok(result);
                }

                result.Id = retrieveResult["id"]?.ToString();
                result.EnrollmentId = retrieveResult["enrollmentId"]?.ToString();
                result.State = retrieveResult["state"]?.ToString();
                result.TokenReferenceId = retrieveResult["tokenReferenceId"]?.ToString();
                result.Type = retrieveResult["type"]?.ToString();
                result.Number = retrieveResult["number"]?.ToString();
                result.ExpirationMonth = retrieveResult["expirationMonth"]?.ToString();
                result.ExpirationYear = retrieveResult["expirationYear"]?.ToString();
                result.RequestorId = retrieveResult["requestorId"]?.ToString();

                try
                {
                    await DBNetworkTokenServices.CreateNetworkToken(new NetworkTokenInfoDto
                    {
                        PaymentCardId = null,
                        InstrumentIdentifierId = tokenizedCardId,
                        TokenAccountNumber = result.Number ?? string.Empty,
                        TokenExpMonth = result.ExpirationMonth,
                        TokenExpYear = result.ExpirationYear,
                        EnrollmentId = result.EnrollmentId,
                        TokenState = result.State,
                        TokenizedCardType = result.Type,
                        ResponseTransactionJson = retrieveResult.ToJsonString()
                    });
                }
                catch (Exception pe)
                {
                    Console.WriteLine($"[TokenEndpoints] Network token persistence (non-fatal): {pe.Message}");
                }

                return Results.Ok(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Results.Ok(new NetworkTokenProvisionResultDto
                {
                    Error = new ErrorObject { Error = "Exception", Message = e.Message }
                });
            }
        }).WithName("ProvisionNetworkToken");

        // ── End Network Token Provisioning ───────────────────────────────────────

        // ── Tokenize Transaction History ─────────────────────────────────────────

        group.MapGet("/tokenize-transactions", async (
            [Microsoft.AspNetCore.Mvc.FromQuery] int pageNumber = 1,
            [Microsoft.AspNetCore.Mvc.FromQuery] int pageSize   = 10,
            [Microsoft.AspNetCore.Mvc.FromQuery] string? search = null) =>
        {
            var options = new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            Console.WriteLine($"\n[TokenEndpoints] GET /api/tokens/tokenize-transactions page={pageNumber} size={pageSize} search={search}");
            var result = await DBTokenizeTransactionServices.GetPagedAsync(pageNumber, pageSize, search);
            Console.WriteLine($"\n[TokenEndpoints] OUTBOUND: {result.TotalCount} total, {result.Items.Count} returned");
            return Results.Ok(result);
        }).WithName("GetTokenizeTransactions");

        group.MapGet("/tokenize-transactions/{id:int}", async (int id) =>
        {
            var options = new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            Console.WriteLine($"\n[TokenEndpoints] GET /api/tokens/tokenize-transactions/{id}");
            var result = await DBTokenizeTransactionServices.GetByIdAsync(id);
            if (result is null)
            {
                var notFound = new JsonObject();
                notFound["error"] = new JsonObject { ["message"] = $"TokenizeTransaction {id} not found." };
                return Results.Ok(notFound);
            }
            Console.WriteLine($"\n[TokenEndpoints] OUTBOUND: TokenizeTransactionId={result.TokenizeTransactionId}");
            return Results.Ok(result);
        }).WithName("GetTokenizeTransactionById");

        group.MapPost("/{tokenId}/payment-credentials", async (string tokenId) =>
        {
            var options = new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            Console.WriteLine($"\n[TokenEndpoints] POST /api/tokens/{tokenId}/payment-credentials");

            if (string.IsNullOrWhiteSpace(tokenId))
            {
                var errorObj = new JsonObject();
                errorObj["error"] = new JsonObject { ["message"] = "tokenId is required." };
                return Results.Ok(errorObj);
            }

            try
            {
                var result = await CallForPaymentCredentials.RunAsync(tokenId);
                Console.WriteLine($"\n[TokenEndpoints] OUTBOUND:\n{JsonSerializer.Serialize(result, options)}");
                return Results.Ok(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                var errorObj = new JsonObject();
                errorObj["Exception"] = e.Message;
                return Results.Ok(errorObj);
            }
        }).WithName("GeneratePaymentCredentials");

        // ── End Tokenize Transaction History ─────────────────────────────────────

        return group;
    }
}

