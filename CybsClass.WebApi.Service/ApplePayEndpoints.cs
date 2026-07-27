using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Models.BaseData;
using CybsClass.Cybersource.Transactions;
using CybsClass.EntityModels;
using CybsClass.WebApi.Service.Services.ApplePay;
using CybsClass.WebApi.Service.Services.DBOperations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CybsClass.WebApi.Service;

public static class ApplePayEndpoints
{
    private static readonly JsonSerializerOptions _logOptions =
        new() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public static void MapApplePayEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/applepay").WithTags(nameof(ApplePayTransaction));

        // Merchant-decryption authorize: decrypt PKPaymentToken.paymentData server-side, submit
        // a tokenizedCard authorization to CyberSource, then persist. Mirrors api/authtransaction
        // in WebApplication.Extensions.cs (CcAuthService -> PersistCustomerData), but as its own
        // dedicated path so nothing in the shared AUTH/SALE flow is touched.
        group.MapPost("/authorize", async ([FromBody] B2cCustomerDto b2cCustomerDto,
            [FromServices] CcApplePayService ccApplePayService) =>
        {
            Console.WriteLine($"api/applepay/authorize INBOUND JSON: {JsonSerializer.Serialize(b2cCustomerDto, _logOptions)}");

            JsonObject jsonObject = await ccApplePayService.CallForAuth(b2cCustomerDto);

            Console.WriteLine($"api/applepay/authorize OUTBOUND JSON: {JsonSerializer.Serialize(jsonObject, _logOptions)}");

            JsonNode jsonNode = jsonObject;
            string? statusNode = jsonNode!["status"]?.GetValue<string>();

            if (statusNode is null)
            {
                // No "status" — either a raw error object from CallCyberSourceApiJson's catch
                // block (decryption failure, missing key, CyberSource unreachable, etc.) or an
                // unparseable body. Same 2XX + ErrorObject convention as every other endpoint.
                var err = new CybsClass.Cybersource.Models.BaseData.ErrorObject
                {
                    Error = jsonNode!["error"]?.GetValue<string>() ?? "No status returned from CyberSource",
                    Message = "Apple Pay authorization did not return a parseable transaction response.",
                    CybersourceJson = jsonNode!["cybersourceJson"]?.GetValue<string>()
                };
                Console.WriteLine($"api/applepay/authorize OUTBOUND (error) JSON: {JsonSerializer.Serialize(err, _logOptions)}");
                return Results.Json(err);
            }

            if (statusNode.Contains("INVALID", StringComparison.OrdinalIgnoreCase)
                || statusNode.Contains("DECLINED", StringComparison.OrdinalIgnoreCase)
                || statusNode.Contains("ERROR", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("api/applepay/authorize -------------- DB PERSIST SKIPPED (status = " + statusNode + ")");
            }
            else
            {
                var dbResults = await PersistApplePayTransaction.InsertApplePayTransaction(
                    b2cCustomerDto,
                    jsonNode,
                    CallForApplePayAuth.LastDecryptedTokenJson,
                    CallCyberSource.LastRequestSent);

                if (dbResults.TryGetValue("OrderId", out var orderId))
                {
                    jsonNode["OrderId"] = orderId.ToString();
                }
                if (dbResults.TryGetValue("B2cCustomerId", out var b2cCustomerId))
                {
                    jsonNode["B2cCustomerId"] = b2cCustomerId.ToString();
                }
                if (dbResults.TryGetValue("ApplePayTransactionsId", out var applePayId))
                {
                    jsonNode["ApplePayTransactionsId"] = applePayId.ToString();
                }
            }

            return Results.Json(jsonNode);
        })
        .WithName("ApplePayAuthorize")
        .Produces<JsonNode>(StatusCodes.Status200OK);

        // onvalidatemerchant proxy: the browser cannot call Apple's validationURL directly
        // (requires mutual TLS with the Merchant Identity Certificate, which must never reach the
        // client). Receives the validationURL from the client JS, POSTs to Apple server-side, and
        // relays Apple's merchant session JSON straight back so the client can call
        // session.completeMerchantValidation(...).
        group.MapPost("/validate-merchant", async ([FromBody] ApplePayMerchantValidationRequestDto dto) =>
        {
            Console.WriteLine($"api/applepay/validate-merchant INBOUND JSON: {JsonSerializer.Serialize(dto, _logOptions)}");

            if (string.IsNullOrWhiteSpace(dto.ValidationUrl))
            {
                var missingUrlErr = new CybsClass.Cybersource.Models.BaseData.ErrorObject { Error = "validationUrl is required." };
                return Results.Json(missingUrlErr);
            }

            JsonObject result = await CallForMerchantValidation.RunAsyncJsonObject(
                dto.ValidationUrl,
                CybsClass.Cybersource.Authentication.ApplePayCredentials.MerchantIdentifier ?? string.Empty,
                CybsClass.Cybersource.Authentication.ApplePayCredentials.InitiativeContext);

            Console.WriteLine($"api/applepay/validate-merchant OUTBOUND JSON: {result.ToJsonString(_logOptions)}");
            return Results.Json(result);
        })
        .WithName("ApplePayValidateMerchant")
        .Produces<JsonNode>(StatusCodes.Status200OK);

        group.MapGet("/count", async () =>
            Results.Ok(await DBApplePayTransactionServices.GetApplePayTransactionCountAsync()))
            .WithName("GetApplePayTransactionCount");

        group.MapGet("/", async (HttpContext httpContext) =>
        {
            var applePayTransactions = await DBApplePayTransactionServices.GetApplePayTransactions();
            var applePayTransaction = applePayTransactions.LastOrDefault();
            return applePayTransaction != null ? Results.Ok(applePayTransactions) : Results.NotFound();
        })
        .WithName("GetAllApplePayTransactions");

        group.MapGet("/{id}", async Task<Results<Ok<ApplePayTransactionDto>, NotFound>> ([FromRoute] int id) =>
        {
            var dto = await DBApplePayTransactionServices.GetApplePayTransactionByUsingId(id);
            return dto == null ? TypedResults.NotFound() : TypedResults.Ok(dto);
        })
        .WithName("GetApplePayTransactionById");
    }
}
