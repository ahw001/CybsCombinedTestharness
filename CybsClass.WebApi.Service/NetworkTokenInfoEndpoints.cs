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
    }
}
