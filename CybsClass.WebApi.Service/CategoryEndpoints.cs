using CybsClass.EntityModels;
using CybsClass.WebApi.Service.Services.DBOperations;

namespace CybsClass.WebApi.Service;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Category").WithTags(nameof(Category));

        group.MapGet("/", async () =>
        {
            return (await DBCategoryServices.GetAllCategories()).ToOkOrError();
        })
        .WithName("GetAllCategories");

        group.MapGet("/{categoryid}", async (int categoryid) =>
        {
            return (await DBCategoryServices.GetCategoryById(categoryid))
                .ToOkOrNotFound($"No Category found with id {categoryid}.");
        })
        .WithName("GetCategoryById");

        group.MapPut("/{categoryid}", async (int categoryid, Category category) =>
        {
            return (await DBCategoryServices.UpdateCategory(categoryid, category))
                .ToOkOrNotFound($"No Category found with id {categoryid} to update.");
        })
        .WithName("UpdateCategory");

        group.MapPost("/", async (Category category) =>
        {
            return (await DBCategoryServices.CreateCategory(category)).ToOkOrError();
        })
        .WithName("CreateCategory");

        group.MapDelete("/{categoryid}", async (int categoryid) =>
        {
            return (await DBCategoryServices.DeleteCategory(categoryid))
                .ToOkOrNotFound($"No Category found with id {categoryid} to delete.");
        })
        .WithName("DeleteCategory");
    }
}
