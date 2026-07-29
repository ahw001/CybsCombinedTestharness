using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using CybsClass.EntityModels;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.WebApi.Service.Services.DBOperations;
namespace CybsClass.WebApi.Service;

public static class IndividualTransactionEndpoints
{
    public static void MapIndividualTransactionEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/IndividualTransaction").WithTags(nameof(IndividualTransaction));

        group.MapGet("/count", async () =>
        {
            return (await DBIndividualTransactionServices.GetIndividualTransactionCountAsync()).ToOkOrError();
        })
        .WithName("GetIndividualTransactionCount");

        group.MapGet("/", async (HttpContext httpContext) =>
        {
            var individualTransactionDtos = await DBIndividualTransactionServices.GetIndividualTransactions();
            var individualTransactionDto = individualTransactionDtos.LastOrDefault();
            if (individualTransactionDto != null)
            {
                return Results.Ok(individualTransactionDtos);
            }

            return Results.Json(DbErrorHandler.BuildNotFound("No Individual Transactions found."));
        })
        .WithName("GetAllIndividualTransactions");

        group.MapGet("/{id}", async (int id) =>
        {
            var individualTransactionDto = await DBIndividualTransactionServices.GetIndividualTransactionByUsingId(id);
            if (individualTransactionDto == null)
            {
                return Results.Json(DbErrorHandler.BuildNotFound($"No Individual Transaction found with id {id}."));
            }
            return Results.Ok(individualTransactionDto);
        })
        .WithName("GetIndividualTransactionById");

    }
}
