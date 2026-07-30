using CybsClass.Cybersource.Models.BaseData;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.WebApi.Service.Services;
using System.Text.Json;

namespace CybsClass.WebApi.Service;

public static class CybsLogEndpoints
{
    private static readonly JsonSerializerOptions _logOptions =
        new() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public static void MapCybsLogEndpoints(this IEndpointRouteBuilder routes)
    {
        // Retrieval side of the ApiLogSidebar capture: the middleware in Program.cs stores the
        // CyberSource exchanges made during a request in CybsCallLogStore and emits X-Cybs-Log-Id;
        // the CybsClient ApiLogDelegatingHandler fetches them here. Read-once. Always 2XX —
        // an unknown/evicted id returns an ErrorObject per the server error convention.
        // Deliberately quiet on the happy path (user-approved exception to the every-endpoint
        // logging convention): this is sidebar plumbing that fires during every transaction,
        // and its lines were interleaving into the demo's CyberSource request/response logging.
        // The exchange bodies themselves are already printed in full by CallCyberSource.
        routes.MapGet("/api/cybslog/{logid:guid}", (Guid logid) =>
        {
            CybsCallLogDto dto;
            if (CybsCallLogStore.TryTake(logid, out var exchanges) && exchanges is not null)
            {
                dto = new CybsCallLogDto
                {
                    Exchanges = exchanges.Select(e => new CybsExchangeDto
                    {
                        Url = e.Url,
                        HttpMethod = e.HttpMethod,
                        RequestJson = e.RequestJson,
                        ResponseJson = e.ResponseJson,
                        HttpStatusCode = e.HttpStatusCode,
                        IsError = e.IsError,
                        FaultMessage = e.FaultMessage
                    }).ToList()
                };
            }
            else
            {
                dto = new CybsCallLogDto
                {
                    Error = new ErrorObject
                    {
                        Error = "NOT_FOUND",
                        Message = $"No CyberSource call log found for id {logid} (already fetched or evicted)."
                    }
                };
                Console.WriteLine($"api/cybslog OUTBOUND (error) JSON: {JsonSerializer.Serialize(dto, _logOptions)}");
            }

            return Results.Json(dto);
        });
    }
}
