using Microsoft.AspNetCore.Mvc;
using CybsClass.EntityModels;
using CybsClass.WebApi.Service.Services.DBOperations;

namespace CybsClass.WebApi.Service;

public static class MerchantSampleDatumEndpoints
{
    public static void MapMerchantSampleDatumEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/merchantsampledatum").WithTags(nameof(MerchantSampleDatum));

        group.MapGet("/", async () =>
        {
            return (await DBMerchantSampleDatumServices.GetRandomMerchantSampleDatum())
                .ToOkOrNotFound("No MerchantSampleDatum rows available.");
        })
        .WithName("GetRandomMerchant")
        .Produces<MerchantSampleDatum>(StatusCodes.Status200OK);

        group.MapGet("/{samplemerchantid}", async (int samplemerchantid) =>
        {
            return (await DBMerchantSampleDatumServices.GetMerchantSampleDatumById(samplemerchantid))
                .ToOkOrNotFound($"No MerchantSampleDatum found with id {samplemerchantid}.");
        })
        .WithName("GetMerchantSampleDatumById");

        group.MapPut("/{samplemerchantid}", async (int samplemerchantid, MerchantSampleDatum merchantSampleDatum) =>
        {
            return (await DBMerchantSampleDatumServices.UpdateMerchantSampleDatum(samplemerchantid, merchantSampleDatum))
                .ToOkOrNotFound($"No MerchantSampleDatum found with id {samplemerchantid} to update.");
        })
        .WithName("UpdateMerchantSampleDatum");

        group.MapPost("/", async (MerchantSampleDatum merchantSampleDatum) =>
        {
            return (await DBMerchantSampleDatumServices.CreateMerchantSampleDatum(merchantSampleDatum)).ToOkOrError();
        })
        .WithName("CreateMerchantSampleDatum");

        group.MapDelete("/{samplemerchantid}", async (int samplemerchantid) =>
        {
            return (await DBMerchantSampleDatumServices.DeleteMerchantSampleDatum(samplemerchantid))
                .ToOkOrNotFound($"No MerchantSampleDatum found with id {samplemerchantid} to delete.");
        })
        .WithName("DeleteMerchantSampleDatum");
    }
}
