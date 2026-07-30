using Microsoft.AspNetCore.Mvc;
using CybsClass.Cybersource.Models.BaseData;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.WebApi.Service.Services.DBOperations;
using System.Text.Json;

namespace CybsClass.WebApi.Service;

// Phase 0 of the Unified Checkout store-path plan (UnifiedCheckoutPlanning.md) — CRUD for
// named, saveable Unified Checkout capture-context configurations. Table already exists
// (DDL executed by the user, local + Winhost) before this file was written.
public static class UnifiedCheckoutConfigEndpoints
{
    private static readonly JsonSerializerOptions _logOptions =
        new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private static ErrorObject BuildError(string error, string message, string action) =>
        new ErrorObject { Error = error, Message = message, Action = action, Reason = "Unified Checkout configuration operation failed." };

    public static RouteGroupBuilder GroupUnifiedCheckoutConfigEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async () =>
        {
            Console.WriteLine("\n\n[UnifiedCheckoutConfig] GET /api/uc-config");
            var all = await DBUnifiedCheckoutConfigurationServices.GetAllAsync();
            if (!all.Ok)
            {
                Console.WriteLine($"[UnifiedCheckoutConfig] RESPONSE ERROR: {JsonSerializer.Serialize(all.Error, _logOptions)}");
                return Results.Json(all.Error);
            }
            var dtos = all.Value!.Select(DBUnifiedCheckoutConfigurationServices.ToDto).ToList();
            Console.WriteLine($"[UnifiedCheckoutConfig] RESPONSE count={dtos.Count}\n{JsonSerializer.Serialize(dtos, _logOptions)}");
            return Results.Json(dtos);
        }).WithName("GetAllUnifiedCheckoutConfigs");

        group.MapGet("/{id:int}", async ([FromRoute] int id) =>
        {
            Console.WriteLine($"\n\n[UnifiedCheckoutConfig] GET /api/uc-config/{id}");
            var result = await DBUnifiedCheckoutConfigurationServices.GetByIdAsync(id);

            // A database failure and a missing row are different answers and must not share
            // one "not found" response - the caller can retry the first but not the second.
            if (!result.Ok)
            {
                Console.WriteLine($"[UnifiedCheckoutConfig] RESPONSE ERROR: {JsonSerializer.Serialize(result.Error, _logOptions)}");
                return Results.Json(result.Error);
            }

            if (result.Value is null)
            {
                var err = BuildError("NOT_FOUND", $"UnifiedCheckoutConfiguration {id} not found.", "Check the ID.");
                Console.WriteLine($"[UnifiedCheckoutConfig] RESPONSE ERROR: {JsonSerializer.Serialize(err, _logOptions)}");
                return Results.Json(err);
            }

            var dto = DBUnifiedCheckoutConfigurationServices.ToDto(result.Value);
            Console.WriteLine($"[UnifiedCheckoutConfig] RESPONSE: {JsonSerializer.Serialize(dto, _logOptions)}");
            return Results.Json(dto);
        }).WithName("GetUnifiedCheckoutConfigById");

        group.MapPost("/", async ([FromBody] UnifiedCheckoutConfigurationDto dto) =>
        {
            Console.WriteLine($"\n\n[UnifiedCheckoutConfig] POST /api/uc-config REQUEST: {JsonSerializer.Serialize(dto, _logOptions)}");

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                var err = BuildError("Invalid Request", "Name is required.", "Provide a Name for the configuration.");
                Console.WriteLine($"[UnifiedCheckoutConfig] RESPONSE ERROR: {JsonSerializer.Serialize(err, _logOptions)}");
                return Results.Json(err);
            }

            var saved = await DBUnifiedCheckoutConfigurationServices.UpsertAsync(dto);
            if (!saved.Ok)
            {
                Console.WriteLine($"[UnifiedCheckoutConfig] RESPONSE ERROR: {JsonSerializer.Serialize(saved.Error, _logOptions)}");
                return Results.Json(saved.Error);
            }
            var entity = saved.Value;
            if (entity is null)
            {
                var err = DbErrorHandler.BuildNotFound($"UnifiedCheckoutConfiguration {dto.UnifiedCheckoutConfigurationId} not found to update.");
                Console.WriteLine($"[UnifiedCheckoutConfig] RESPONSE ERROR: {JsonSerializer.Serialize(err, _logOptions)}");
                return Results.Json(err);
            }

            var response = DBUnifiedCheckoutConfigurationServices.ToDto(entity);
            Console.WriteLine($"[UnifiedCheckoutConfig] RESPONSE: {JsonSerializer.Serialize(response, _logOptions)}");
            return Results.Json(response);
        }).WithName("UpsertUnifiedCheckoutConfig");

        group.MapDelete("/{id:int}", async ([FromRoute] int id) =>
        {
            Console.WriteLine($"\n\n[UnifiedCheckoutConfig] DELETE /api/uc-config/{id}");
            var deleted = await DBUnifiedCheckoutConfigurationServices.DeleteAsync(id);
            if (!deleted.Ok)
            {
                Console.WriteLine($"[UnifiedCheckoutConfig] RESPONSE ERROR: {JsonSerializer.Serialize(deleted.Error, _logOptions)}");
                return Results.Json(deleted.Error);
            }
            if (!deleted.Value)
            {
                var err = DbErrorHandler.BuildNotFound($"UnifiedCheckoutConfiguration {id} not found to delete.");
                Console.WriteLine($"[UnifiedCheckoutConfig] RESPONSE ERROR: {JsonSerializer.Serialize(err, _logOptions)}");
                return Results.Json(err);
            }
            Console.WriteLine($"[UnifiedCheckoutConfig] DELETE /api/uc-config/{id} success");
            return Results.Json(new { deleted = true, unifiedCheckoutConfigurationId = id });
        }).WithName("DeleteUnifiedCheckoutConfig");

        return group;
    }
}
