using CybsClass.EntityModels;
using CybsClass.WebApi.Service.Services.DBOperations;

namespace CybsClass.WebApi.Service;

public static class B2cCustomers
{
    public static void MapB2cCustomerEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/B2cCustomer").WithTags(nameof(B2cCustomer));

        group.MapGet("/", async () =>
        {
            return (await DBCustomerServices.GetB2CCustomers()).ToOkOrError();
        })
        .WithName("GetAllB2cCustomers");

        group.MapGet("/paging/{pageIndex}/{pageSize}", async (int pageIndex, int pageSize) =>
        {
            return (await DBCustomerServices.GetB2cCustomerPagedAsync(pageIndex, pageSize)).ToOkOrError();
        })
        .WithName("GetPagingCustomers");

        group.MapGet("/{b2ccustomerid}", async (int b2ccustomerid) =>
        {
            return (await DBCustomerServices.GetB2cCustomerByIdAsync(b2ccustomerid))
                .ToOkOrNotFound($"No B2cCustomer found with id {b2ccustomerid}.");
        })
        .WithName("GetB2cCustomerById");

        group.MapPut("/{b2ccustomerid}", async (int b2ccustomerid, B2cCustomer b2cCustomer) =>
        {
            return (await DBCustomerServices.UpdateB2cCustomer(b2ccustomerid, b2cCustomer))
                .ToOkOrNotFound($"No B2cCustomer found with id {b2ccustomerid} to update.");
        })
        .WithName("UpdateB2cCustomer");

        group.MapPost("/", async (B2cCustomer b2cCustomer) =>
        {
            return (await DBCustomerServices.CreateB2cCustomerSimple(b2cCustomer)).ToOkOrError();
        })
        .WithName("CreateB2cCustomer");

        group.MapDelete("/{b2ccustomerid}", async (int b2ccustomerid) =>
        {
            return (await DBCustomerServices.DeleteB2cCustomer(b2ccustomerid))
                .ToOkOrNotFound($"No B2cCustomer found with id {b2ccustomerid} to delete.");
        })
        .WithName("DeleteB2cCustomer");
    }
}
