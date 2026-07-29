using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using CybsClass.EntityModels;
using CybsClass.WebApi.Service.Services.DBOperations;
using CybsClass.Cybersource.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
namespace CybsClass.WebApi.Service;

public static class AuthTransResponseEndpoints
{
    public static void MapAuthTransResponseEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/AuthTransResponse").WithTags(nameof(AuthTransResponse));

        group.MapGet("/count", async () =>
        {
            return (await DBAuthTransResponseServices.GetAuthTransResponseCountAsync()).ToOkOrError();
        })
        .WithName("GetAuthTransResponseCount");

        group.MapGet("/", async (HttpContext httpContext) =>
        {
            var authTransResponses = await DBAuthTransResponseServices.GetAuthTransResponses();
            var authTransResponse = authTransResponses.LastOrDefault();
            if (authTransResponse != null)
            {
                return Results.Ok(authTransResponses);
            }

            return Results.Json(DbErrorHandler.BuildNotFound("No Auth Trans Responses found."));
        })
        .WithName("GetAllAuthTransResponses");

        group.MapGet("/{id}", async ([FromRoute] int id) =>
        {
            var authTransResponseDto = await DBAuthTransResponseServices.GetAuthTransResponseByUsingId(id);
            if (authTransResponseDto == null)
            {
                return Results.Json(DbErrorHandler.BuildNotFound($"No Auth Trans Response found with id {id}."));
            }
            return Results.Ok(authTransResponseDto);
        })
        .WithName("GetAuthTransResponseById");

    }
}
