using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using CybsClass.EntityModels;
using CybsClass.WebApi.Service.Services.DBOperations;
using Microsoft.AspNetCore.Mvc;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Models.Mappers;
namespace CybsClass.WebApi.Service;

public static class NetworkTokenInfoEndpoints
{
    public static void MapNetworkTokenInfoEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/NetworkTokenInfo").WithTags(nameof(NetworkTokenInfo));

        group.MapGet("/count", async () =>
        {
            return (await DBNetworkTokenServices.GetNetworkTokenInfosCountAsync()).ToOkOrError();
        })
        .WithName("GetNetowrkTokenCount");

        group.MapGet("/", async (HttpContext httpContext) =>
        {
            var networkTokenInfoDtos = await DBNetworkTokenServices.GetNetworkTokens();
            var networkTokenInfoDto = networkTokenInfoDtos.LastOrDefault();
            if (networkTokenInfoDto != null)
            {
                return Results.Ok(networkTokenInfoDto);
            }

            return Results.Json(DbErrorHandler.BuildNotFound("No Network Tokens found."));
        })
        .WithName("GetAllNetworkTokenInfos");

        group.MapGet("/{id}", async ([FromRoute] int id) =>
        {
            var networkTokenInfoDtos = await DBNetworkTokenServices.GetNetworkTokenByUsingId(id);
            if (networkTokenInfoDtos == null || networkTokenInfoDtos.Count == 0)
            {
                return Results.Json(DbErrorHandler.BuildNotFound($"No Network Tokens found for payment card {id}."));
            }
            return Results.Ok(networkTokenInfoDtos);
        })
        .WithName("GetNetworkTokenInfoById");

        // NOTE the key here is PaymentTokenId - the NetworkTokenInfo primary key - which is
        // NOT what GET /{id} above takes; that one looks up by PaymentCardId and can return
        // several tokens. The placeholder is named for the parameter deliberately: ASP.NET
        // Core binds route values by name, and a mismatch silently falls back to query-string
        // binding instead of failing (that is what produced 15 broken routes in this service).
        group.MapPut("/{paymenttokenid}", async ([FromRoute] int paymenttokenid, NetworkTokenInfoDto networkTokenInfoDto) =>
        {
            return (await DBNetworkTokenServices.UpdateNetworkToken(paymenttokenid, networkTokenInfoDto))
                .ToOkOrNotFound($"No NetworkTokenInfo found with PaymentTokenId {paymenttokenid} to update.");
        })
        .WithName("UpdateNetworkToken");
    }
}
