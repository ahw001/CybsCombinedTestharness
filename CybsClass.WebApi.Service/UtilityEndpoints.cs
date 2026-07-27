using Microsoft.AspNetCore.Mvc;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.WebApi.Service.Services.Utilities;
using System.Text.Json.Nodes;

namespace CybsClass.WebApi.Service;

public static class UtilityEndpoints
{
    public static RouteGroupBuilder UtilityGroupEndPoints(this RouteGroupBuilder group)
    {
        group.MapPost("/processor", async ([FromBody] SimpleJsonProcessorDto simpleJsonProcessorDto) =>
        {
            JsonObject jsonObject = new JsonObject();

            var inboundJson = System.Text.Json.JsonSerializer.Serialize(simpleJsonProcessorDto);
            Console.WriteLine($"\n[/api/json/processor] INBOUND: {inboundJson}");

            if (simpleJsonProcessorDto == null || simpleJsonProcessorDto.Value == null)
            {
                return Results.NotFound();
            }

            jsonObject = await SimpleJsonProcessor.CallForSimpleJson(simpleJsonProcessorDto);

            // Diagnostic-only: surface the actual outbound request headers and the CyberSource
            // response headers alongside the body, so header/TLS-level differences between
            // origins (e.g. local vs Winhost) are visible directly in the Raw JSON Processor tool.
            // CallCyberSource.LastRequestHeaders/LastResponseHeaders are populated by whichever
            // CallCyberSourceApi* method SimpleJsonProcessor routed to for this call.
            jsonObject["requestHeaders"] = System.Text.Json.JsonSerializer.SerializeToNode(CybsClass.Cybersource.Transactions.CallCyberSource.LastRequestHeaders);
            jsonObject["responseHeaders"] = System.Text.Json.JsonSerializer.SerializeToNode(CybsClass.Cybersource.Transactions.CallCyberSource.LastResponseHeaders);

            var outboundJson = System.Text.Json.JsonSerializer.Serialize(jsonObject, new System.Text.Json.JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            Console.WriteLine($"\n[/api/json/processor] OUTBOUND: {outboundJson}");

            if (jsonObject == null)
            {
                return Results.NotFound();
            }
            else
            {
                return Results.Ok(jsonObject);
            }

        }).Produces<JsonObject>().WithName("JsonProcessor");

        return group;
    }
}


