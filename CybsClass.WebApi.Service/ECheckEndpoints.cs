using CybsClass.Cybersource.Models.DTOs;
using CybsClass.EntityModels;
using CybsClass.WebApi.Service.Services.CcTransatcionProcessing;
using CybsClass.WebApi.Service.Services.DBOperations;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CybsClass.WebApi.Service;

// eCheck (ACH) endpoints — the four documented REST flows plus the two reads that feed the
// client's "Use Defaults" and saved-token dropdowns.
public static class ECheckEndpoints
{
    private static readonly JsonSerializerOptions _logOptions =
        new() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public static RouteGroupBuilder GroupECheckEndpoints(this RouteGroupBuilder group)
    {
        // ---- POST /api/echeck/debit --------------------------------------------------------
        // Serves all four flows; which one runs is decided by the DTO flags, not the route.
        group.MapPost("/debit", async ([FromBody] B2cCustomerDto b2cCustomerDto) =>
        {
            Console.WriteLine($"api/echeck/debit INBOUND JSON: {JsonSerializer.Serialize(b2cCustomerDto, _logOptions)}");

            (JsonObject jsonObject, string requestJson, string transactionType) =
                await CallForECheckDebit.RunAsyncJsonObject(b2cCustomerDto);

            Console.WriteLine($"api/echeck/debit OUTBOUND JSON: {JsonSerializer.Serialize(jsonObject, _logOptions)}");

            JsonNode jsonNode = jsonObject;
            string? statusNode = jsonNode["status"]?.GetValue<string>();

            if (statusNode is null)
            {
                // No "status" — either a raw error object from CallCyberSourceApiJson's catch
                // block or an unparseable body. Same 2XX + ErrorObject convention as every
                // other endpoint in this project.
                var err = new CybsClass.Cybersource.Models.BaseData.ErrorObject
                {
                    Error = jsonNode["error"]?.GetValue<string>() ?? "No status returned from CyberSource",
                    Message = "CyberSource did not return a parseable eCheck response.",
                    CybersourceJson = jsonNode["cybersourceJson"]?.GetValue<string>()
                };

                Console.WriteLine($"api/echeck/debit OUTBOUND (error) JSON: {JsonSerializer.Serialize(err, _logOptions)}");
                return Results.Json(err);
            }

            Console.WriteLine($"**** ECHECK STATUS NODE = {statusNode}");

            // PENDING is the eCheck success case — settlement is asynchronous, so there is no
            // AUTHORIZED here and treating a non-AUTHORIZED status as failure would skip
            // persistence on every successful transaction. Only genuine failures are excluded.
            bool failed =
                statusNode.Contains("INVALID", StringComparison.OrdinalIgnoreCase)
                || statusNode.Contains("DECLINED", StringComparison.OrdinalIgnoreCase)
                || statusNode.Contains("ERROR", StringComparison.OrdinalIgnoreCase)
                || statusNode.Contains("REJECT", StringComparison.OrdinalIgnoreCase)
                || statusNode.Contains("FAIL", StringComparison.OrdinalIgnoreCase);

            if (failed)
            {
                Console.WriteLine($"-------------- eCheck {statusNode}: DB FUNCTIONS SKIPPED");
                return Results.Json(jsonNode);
            }

            Dictionary<string, object> dbResults = await PersistECheckTransaction.InsertECheckTransaction(
                b2cCustomerDto, jsonNode, requestJson, transactionType);

            // The debit already succeeded at CyberSource by this point, so a persistence failure
            // must not become a 500 — and must not be silently dropped either. Deliberately NOT
            // named "error": the client's JsonErrorExtractor treats an "error" property as a
            // failed transaction, and this is a successful one that merely failed to persist.
            if (dbResults.TryGetValue(DbErrorHandler.ErrorKey, out object? persistError))
            {
                jsonNode["dbPersistError"] = persistError?.ToString();
                Console.WriteLine($"-------------- DB PERSIST FAILED: {persistError}");
            }
            else
            {
                if (dbResults.TryGetValue("B2cCustomerId", out object? b2cCustomerId))
                    jsonNode["B2cCustomerId"] = Convert.ToInt32(b2cCustomerId);

                if (dbResults.TryGetValue("OrderId", out object? orderId))
                    jsonNode["OrderId"] = orderId?.ToString();

                if (dbResults.TryGetValue("ECheckTransactionId", out object? echeckTransactionId))
                    jsonNode["ECheckTransactionId"] = Convert.ToInt32(echeckTransactionId);

                if (dbResults.TryGetValue("ECheckPaymentInstrumentId", out object? instrumentId))
                    jsonNode["ECheckPaymentInstrumentId"] = Convert.ToInt32(instrumentId);
            }

            Console.WriteLine($"api/echeck/debit FINAL OUTBOUND JSON: {JsonSerializer.Serialize(jsonNode, _logOptions)}");
            return Results.Json(jsonNode);
        })
        .WithName("SubmitECheckDebit");

        // ---- GET /api/echeck/test-accounts --------------------------------------------------
        group.MapGet("/test-accounts", async ([FromServices] CybsDbContext db) =>
        {
            Console.WriteLine("\n[ECheckEndpoints] GET /api/echeck/test-accounts");

            DbResult<List<ECheckTestAccountDto>> result = await DBECheckTestAccountServices.GetAllAsync(db);

            Console.WriteLine($"[ECheckEndpoints] OUTBOUND: {JsonSerializer.Serialize(result.Value, _logOptions)}");
            return result.ToOkOrError();
        })
        .WithName("GetECheckTestAccounts");

        // ---- GET /api/echeck/paymentinstruments ---------------------------------------------
        // Every saved eCheck token, for the token-debit dropdown.
        group.MapGet("/paymentinstruments", async ([FromServices] CybsDbContext db) =>
        {
            Console.WriteLine("\n[ECheckEndpoints] GET /api/echeck/paymentinstruments");

            DbResult<List<ECheckPaymentInstrumentDto>> result = await DBECheckPaymentInstrumentServices.GetAllAsync(db);

            Console.WriteLine($"[ECheckEndpoints] OUTBOUND: {JsonSerializer.Serialize(result.Value, _logOptions)}");
            return result.ToOkOrError();
        })
        .WithName("GetECheckPaymentInstruments");

        // Route placeholder is named for its handler parameter, not "{id}" — mismatched names
        // silently fail to bind from the path and fall back to the query string, which is how
        // 15 endpoints in this project ended up returning 500s.
        group.MapGet("/paymentinstruments/{b2ccustomerid:int}", async (
            int b2ccustomerid,
            [FromServices] CybsDbContext db) =>
        {
            Console.WriteLine($"\n[ECheckEndpoints] GET /api/echeck/paymentinstruments/{b2ccustomerid}");

            DbResult<List<ECheckPaymentInstrumentDto>> result =
                await DBECheckPaymentInstrumentServices.GetByCustomerAsync(db, b2ccustomerid);

            Console.WriteLine($"[ECheckEndpoints] OUTBOUND: {JsonSerializer.Serialize(result.Value, _logOptions)}");
            return result.ToOkOrError();
        })
        .WithName("GetECheckPaymentInstrumentsByCustomer");

        return group;
    }
}
