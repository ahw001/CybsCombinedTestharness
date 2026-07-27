using CybsClass.Cybersource.Models.DTOs;
using CybsClass.EntityModels;
using ErrorObject = CybsClass.Cybersource.Models.BaseData.ErrorObject;
using CybsClass.WebApi.Service.Services.DBOperations;
using CybsClass.WebApi.Service.Services.PayByLinkProcessing;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CybsClass.WebApi.Service;

public static class PayByLinkEndpoints
{
    private static readonly JsonSerializerOptions _logOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static RouteGroupBuilder GroupPayByLinkEndpoints(this RouteGroupBuilder group)
    {
        // ── POST /api/paybylink/create ──────────────────────────────────────────
        group.MapPost("/create", async ([FromBody] PayByLinkRequestDto dto) =>
        {
            Console.WriteLine($"\n=== PAY BY LINK /create ===");
            Console.WriteLine($"Inbound: {JsonSerializer.Serialize(dto, _logOptions)}");

            PayByLinkResponseDto response = new();

            try
            {
                JsonObject cybsResponse = await CallForCybsPayByLinkCreate.RunAsyncJsonObject(dto);

                string responseStr = cybsResponse.ToString();

                // Return errors without persisting
                if (responseStr.Contains("Exception", StringComparison.OrdinalIgnoreCase)
                    || responseStr.Contains("INVALID", StringComparison.OrdinalIgnoreCase)
                    || responseStr.Contains("\"error\"", StringComparison.OrdinalIgnoreCase))
                {
                    response.Error = new ErrorObject
                    {
                        Error = "CyberSource error",
                        Message = responseStr,
                        Reason = "CyberSource returned an error response"
                    };
                    Console.WriteLine($"Outbound (error): {JsonSerializer.Serialize(response, _logOptions)}");
                    return Results.Ok(response);
                }

                PayByLinkTransaction entity = await PersistPayByLinkData.CreatePayByLinkAsync(dto, cybsResponse);

                response = MapToDto(entity);

                Console.WriteLine($"Outbound: {JsonSerializer.Serialize(response, _logOptions)}");
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in /create: {ex.Message}");
                response.Error = new ErrorObject { Error = ex.Message, Message = ex.Message };
                return Results.Ok(response);
            }
        }).Produces<PayByLinkResponseDto>().WithName("PayByLinkCreate");

        // ── GET /api/paybylink/all ──────────────────────────────────────────────
        group.MapGet("/all", async () =>
        {
            Console.WriteLine("\n=== PAY BY LINK /all ===");

            try
            {
                List<PayByLinkTransaction> records = await DBPayByLinkServices.GetAllPayByLinkTransactionsAsync();
                List<PayByLinkResponseDto> result = records.Select(MapToDto).ToList();

                Console.WriteLine($"Returning {result.Count} Pay by Link record(s).");
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in /all: {ex.Message}");
                var error = new PayByLinkResponseDto
                {
                    Error = new ErrorObject { Error = ex.Message, Message = ex.Message }
                };
                return Results.Ok(new List<PayByLinkResponseDto> { error });
            }
        }).Produces<List<PayByLinkResponseDto>>().WithName("PayByLinkGetAll");

        // ── POST /api/paybylink/checkstatus/{id} ───────────────────────────────
        group.MapPost("/checkstatus/{id:int}", async ([FromRoute] int id) =>
        {
            Console.WriteLine($"\n=== PAY BY LINK /checkstatus/{id} ===");

            PayByLinkResponseDto response = new();

            try
            {
                PayByLinkTransaction? entity = await DBPayByLinkServices.GetByIdAsync(id);

                if (entity is null)
                {
                    response.Error = new ErrorObject
                    {
                        Error = "Not found",
                        Message = $"Pay by Link record {id} not found in database."
                    };
                    Console.WriteLine($"Outbound (not found): {JsonSerializer.Serialize(response, _logOptions)}");
                    return Results.Ok(response);
                }

                if (string.IsNullOrWhiteSpace(entity.CybersourceLinkId))
                {
                    response.Error = new ErrorObject
                    {
                        Error = "No CyberSource link ID",
                        Message = "This record has no CyberSource link ID — cannot check status."
                    };
                    Console.WriteLine($"Outbound (no link id): {JsonSerializer.Serialize(response, _logOptions)}");
                    return Results.Ok(response);
                }

                JsonObject statusResponse = await CallForCybsPayByLinkStatus.RunAsyncJsonObject(entity.CybersourceLinkId);
                string responseStr = statusResponse.ToString();

                string newStatus = entity.Status;
                if (statusResponse.TryGetPropertyValue("status", out JsonNode? statusNode) && statusNode is not null)
                    newStatus = statusNode.GetValue<string>();

                await PersistPayByLinkData.UpdatePayByLinkStatusAsync(id, newStatus, responseStr);

                // Re-fetch to return fresh data
                PayByLinkTransaction? updated = await DBPayByLinkServices.GetByIdAsync(id);
                response = updated is not null ? MapToDto(updated) : MapToDto(entity);
                response.Status = newStatus;

                Console.WriteLine($"Outbound: {JsonSerializer.Serialize(response, _logOptions)}");
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in /checkstatus/{id}: {ex.Message}");
                response.Error = new ErrorObject { Error = ex.Message, Message = ex.Message };
                return Results.Ok(response);
            }
        }).Produces<PayByLinkResponseDto>().WithName("PayByLinkCheckStatus");

        return group;
    }

    private static PayByLinkResponseDto MapToDto(PayByLinkTransaction entity) => new()
    {
        PayByLinkTransactionId = entity.PayByLinkTransactionId,
        PayByLinkUuid = entity.PayByLinkUuid,
        CybersourceLinkId = entity.CybersourceLinkId,
        PaymentLink = entity.PaymentLink,
        PurchaseNumber = entity.PurchaseNumber,
        CustomerName = entity.CustomerName,
        Email = entity.Email,
        Phone = entity.Phone,
        DeliveryMethod = entity.DeliveryMethod,
        TotalAmount = entity.TotalAmount,
        Currency = entity.Currency,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
        TransactionJson = entity.TransactionJson
    };
}
