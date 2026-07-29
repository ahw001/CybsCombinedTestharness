using CybsClass.EntityModels;
using CybsClass.WebApi.Service.Services.DBOperations;

namespace CybsClass.WebApi.Service;

public static class SampleInvoiceDetailEndpoints
{
    public static void MapSampleInvoiceDetailEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/SampleInvoiceDetail").WithTags(nameof(SampleInvoiceDetail));

        group.MapGet("/", async () =>
        {
            return (await DBSampleInvoiceDetailServices.GetAllSampleInvoiceDetails()).ToOkOrError();
        })
        .WithName("GetAllSampleInvoiceDetails");

        group.MapGet("/{sampleinvoiceid}", async (int sampleinvoiceid) =>
        {
            return (await DBSampleInvoiceDetailServices.GetSampleInvoiceDetailById(sampleinvoiceid))
                .ToOkOrNotFound($"No sample invoice detail found with id {sampleinvoiceid}.");
        })
        .WithName("GetSampleInvoiceDetailById");

        group.MapPut("/{sampleinvoiceid}", async (int sampleinvoiceid, SampleInvoiceDetail sampleInvoiceDetail) =>
        {
            return (await DBSampleInvoiceDetailServices.UpdateSampleInvoiceDetail(sampleinvoiceid, sampleInvoiceDetail))
                .ToOkOrNotFound($"No sample invoice detail found with id {sampleinvoiceid} to update.");
        })
        .WithName("UpdateSampleInvoiceDetail");

        group.MapPost("/", async (SampleInvoiceDetail sampleInvoiceDetail) =>
        {
            return (await DBSampleInvoiceDetailServices.CreateSampleInvoiceDetail(sampleInvoiceDetail)).ToOkOrError();
        })
        .WithName("CreateSampleInvoiceDetail");

        group.MapDelete("/{sampleinvoiceid}", async (int sampleinvoiceid) =>
        {
            return (await DBSampleInvoiceDetailServices.DeleteSampleInvoiceDetail(sampleinvoiceid))
                .ToOkOrNotFound($"No sample invoice detail found with id {sampleinvoiceid} to delete.");
        })
        .WithName("DeleteSampleInvoiceDetail");
    }
}
