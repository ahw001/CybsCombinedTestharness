using Microsoft.AspNetCore.Http.HttpResults;
using CybsClass.EntityModels;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.WebApi.Service.Services.DBOperations;
using Microsoft.AspNetCore.Mvc;
namespace CybsClass.WebApi.Service;

public static class FollowOnTransactionsEndpoints
{
    public static void MapFollowOnTransResponseEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/FollowOnTransResponse").WithTags(nameof(FollowOnTransResponse));

        group.MapGet("/count", async () =>
        {
            return (await DBFollowOnTransResponseServices.GetFollowOnTransResponseCountAsync()).ToOkOrError();
        })
        .WithName("GetFollowOnTransCount");

        group.MapGet("/", async (HttpContext httpContext) =>
        {
            var followOnTransResponseDtos = await DBFollowOnTransResponseServices.GetFollowOnTransResponses();
            var followOnTransResponseDto = followOnTransResponseDtos.LastOrDefault();
            if (followOnTransResponseDto != null)
            {
                return Results.Ok(followOnTransResponseDto);
            }

            return Results.Json(DbErrorHandler.BuildNotFound("No Follow On Transactions found."));
        })
        .WithName("GetAllFollowOnTransactions");

        group.MapGet("/{id}", async ([FromRoute] int id) =>
        {
            var followOnTransResponseDto = await DBFollowOnTransResponseServices.GetFollowOnTransResponseByUsingId(id);
            if (followOnTransResponseDto == null)
            {
                return Results.Json(DbErrorHandler.BuildNotFound($"No Follow On Transaction found with id {id}."));
            }
            return Results.Ok(followOnTransResponseDto);
        })
        .WithName("GetFollowOnTransResponseById");

    }
}
