using CybsClass.EntityModels;
using CybsClass.WebApi.Service.Services.DBOperations;

namespace CybsClass.WebApi.Service;

public static class ElectronicProductEndpoints
{
    public static void MapElectronicProductEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/ElectronicProduct").WithTags(nameof(ElectronicProduct));

        group.MapGet("/", async () =>
        {
            return (await DBElectronicProductServices.GetAllElectronicProducts()).ToOkOrError();
        })
        .WithName("GetAllElectronicProducts");

        group.MapGet("/{id}", async (int id) =>
        {
            return (await DBElectronicProductServices.GetElectronicProductById(id))
                .ToOkOrNotFound($"No ElectronicProduct found with id {id}.");
        })
        .WithName("GetElectronicProductById");

        group.MapPut("/{id}", async (int id, ElectronicProduct electronicProduct) =>
        {
            return (await DBElectronicProductServices.UpdateElectronicProduct(id, electronicProduct))
                .ToOkOrNotFound($"No ElectronicProduct found with id {id} to update.");
        })
        .WithName("UpdateElectronicProduct");

        group.MapPost("/", async (ElectronicProduct electronicProduct) =>
        {
            return (await DBElectronicProductServices.CreateElectronicProduct(electronicProduct)).ToOkOrError();
        })
        .WithName("CreateElectronicProduct");

        group.MapDelete("/{id}", async (int id) =>
        {
            return (await DBElectronicProductServices.DeleteElectronicProduct(id))
                .ToOkOrNotFound($"No ElectronicProduct found with id {id} to delete.");
        })
        .WithName("DeleteElectronicProduct");
    }
}
