using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using CybsClass.EntityModels;
using CybsClass.WebApi.Service.Services.DBOperations;
using Microsoft.AspNetCore.Mvc;
using CybsClass.Cybersource.Models.DTOs;
namespace CybsClass.WebApi.Service;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Order").WithTags(nameof(Order));

        group.MapGet("/count", async () =>
        {
            return (await DBOrdersServices.GetOrdersCountAsync()).ToOkOrError();
        })
        .WithName("GetOrderCount");

        group.MapGet("/", async (HttpContext httpContext) =>
        {
            var orderDtos = await DBOrdersServices.GetOrders();
            var orderDto = orderDtos.LastOrDefault();
            if (orderDto != null)
            {
                return Results.Ok(orderDto);
            }

            return Results.Json(DbErrorHandler.BuildNotFound("No Orders found."));
        })
        .WithName("GetAllOrders");

        group.MapGet("/{id}", async ([FromRoute] int id) =>
        {
            var orderDto = await DBOrdersServices.GetOrdersByUsingId(id);
            if (orderDto == null)
            {
                return Results.Json(DbErrorHandler.BuildNotFound($"No Order found with id {id}."));
            }
            return Results.Ok(orderDto);
        })
        .WithName("GetOrderById");

    }
}
