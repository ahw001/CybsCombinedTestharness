using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using CybsClass.EntityModels;
using CybsClass.WebApi.Service.Services.DBOperations;
using Microsoft.AspNetCore.Mvc;
using CybsClass.Cybersource.Models.DTOs;
namespace CybsClass.WebApi.Service;

public static class OrderDetailEndpoints
{
    public static void MapOrderDetailEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/OrderDetail").WithTags(nameof(OrderDetail));

        group.MapGet("/count", async () =>
        {
            return (await DBOrderDetailServices.GetPaymentCardCountAsync()).ToOkOrError();
        })
        .WithName("GetOrderDetailCount");
 
        group.MapGet("/", async (HttpContext httpContext) =>
        {
            var orderDetailDtos = await DBOrderDetailServices.GetOrderDetails();
            var orderDetailDto = orderDetailDtos.LastOrDefault();
            if (orderDetailDto != null)
            {
                return Results.Ok(orderDetailDto);
            }

            return Results.Json(DbErrorHandler.BuildNotFound("No Order Details found."));
        })
        .WithName("GetAllOrderDetails");


        group.MapGet("/{id}", async ([FromRoute] int id) =>
        {
            var orderDetailDto = await DBOrderDetailServices.GetOrderDetailByUsingId(id);
            if (orderDetailDto == null)
            {
                return Results.Json(DbErrorHandler.BuildNotFound($"No Order Detail found with id {id}."));
            }
            return Results.Ok(orderDetailDto);
        })
        .WithName("GetOrderDetailById");
    }
}
