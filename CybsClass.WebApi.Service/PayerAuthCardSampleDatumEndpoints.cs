using CybsClass.EntityModels;
using CybsClass.WebApi.Service.Services.DBOperations;

namespace CybsClass.WebApi.Service;

public static class PayerAuthCardSampleDatumEndpoints
{
    public static void MapPayerAuthCardSampleDatumEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/PayerAuthCardSampleDatum").WithTags(nameof(PayerAuthCardSampleDatum));

        group.MapGet("/", async () =>
        {
            return (await DBPayerAuthCardSampleDatumServices.GetAllPayerAuthCardSampleData()).ToOkOrError();
        })
        .WithName("GetAllPayerAuthCardSampleData");

        group.MapGet("/{samplepayauthpaymentcardid}", async (int samplepayauthpaymentcardid) =>
        {
            return (await DBPayerAuthCardSampleDatumServices.GetPayerAuthCardSampleDatumById(samplepayauthpaymentcardid))
                .ToOkOrNotFound($"No payer auth card sample datum found with id {samplepayauthpaymentcardid}.");
        })
        .WithName("GetPayerAuthCardSampleDatumById");

        group.MapPut("/{samplepayauthpaymentcardid}", async (int samplepayauthpaymentcardid, PayerAuthCardSampleDatum payerAuthCardSampleDatum) =>
        {
            return (await DBPayerAuthCardSampleDatumServices.UpdatePayerAuthCardSampleDatum(samplepayauthpaymentcardid, payerAuthCardSampleDatum))
                .ToOkOrNotFound($"No payer auth card sample datum found with id {samplepayauthpaymentcardid} to update.");
        })
        .WithName("UpdatePayerAuthCardSampleDatum");

        group.MapPost("/", async (PayerAuthCardSampleDatum payerAuthCardSampleDatum) =>
        {
            return (await DBPayerAuthCardSampleDatumServices.CreatePayerAuthCardSampleDatum(payerAuthCardSampleDatum)).ToOkOrError();
        })
        .WithName("CreatePayerAuthCardSampleDatum");

        group.MapDelete("/{samplepayauthpaymentcardid}", async (int samplepayauthpaymentcardid) =>
        {
            return (await DBPayerAuthCardSampleDatumServices.DeletePayerAuthCardSampleDatum(samplepayauthpaymentcardid))
                .ToOkOrNotFound($"No payer auth card sample datum found with id {samplepayauthpaymentcardid} to delete.");
        })
        .WithName("DeletePayerAuthCardSampleDatum");
    }
}
