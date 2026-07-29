using CybsClass.EntityModels;
using Microsoft.AspNetCore.Mvc;
using CybsClass.WebApi.Service.Services.DBOperations;
using ErrorObject = CybsClass.Cybersource.Models.BaseData.ErrorObject;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Models.Mappers;
namespace CybsClass.WebApi.Service;

public static class PaymentCardInfoEndpoints
{

    public static void MapPaymentCardInfoEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/PaymentCardInfo").WithTags(nameof(PaymentCardInfo));

        group.MapGet("/count", async () =>
        {
            return (await DBPaymentCardServices.GetPaymentCardCountAsync()).ToOkOrError();
        })
        .WithName("GetPaymentCardCount");

        group.MapGet("/", async () =>
        {
            return (await DBPaymentCardServices.GetAllPaymentCardInfoEntities()).ToOkOrError();
        })
        .WithName("GetAllPaymentCardInfos");

        group.MapGet("/{paymentcardid:int}", async ([FromRoute] int paymentcardid) =>
        {
            var paymentCardDto = await DBPaymentCardServices.GetPaymentCardInfoByUsingId(paymentcardid);
            if (paymentCardDto == null)
            {
                return Results.Json(DbErrorHandler.BuildNotFound($"No PaymentCardInfo found with id {paymentcardid}."));
            }
            return Results.Ok(paymentCardDto);
        })
        .WithName("GetPaymentCardInfoByUsingId");


        group.MapGet("/customer/{b2ccustomerid:int}", async ([FromRoute] int b2ccustomerid) =>
        {
            var result = await DBCustomerServices.GetB2CCustomerPaymentCards(b2ccustomerid);

            if (!result.Ok)
            {
                return Results.Json(result.Error);
            }

            if (result.Value is null || result.Value.Count == 0)
            {
                return Results.Json(DbErrorHandler.BuildNotFound($"No payment cards found for customer {b2ccustomerid}."));
            }

            List<PaymentCardDto> paymentCardDtos = PaymentCardMapper.Map(result.Value);
            return Results.Ok(paymentCardDtos);
        })
        .WithName("GetPaymentCardsByCustomer");


        group.MapPut("/{id}", async ([FromRoute] int id, PaymentCardDto paymentCardDto) =>
        {
            return (await DBPaymentCardServices.UpdatePaymentCardInfo(id, paymentCardDto))
                .ToOkOrNotFound($"No PaymentCardInfo found with id {id} to update.");
        })
        .WithName("UpdatePaymentCardInfo");

        group.MapPost("/", async (PaymentCardDto paymentCardDto) =>
        {
            var dbResults = await DBPaymentCardServices.CreatePaymentCardInfo(paymentCardDto);

            if (dbResults.TryGetValue(DbErrorHandler.ErrorKey, out string? error))
            {
                return Results.Json(new ErrorObject
                {
                    Error = "DATABASE_ERROR",
                    Message = error,
                    Reason = "Database Error",
                    Action = "Payment card was not created. See server logs."
                });
            }

            dbResults.TryGetValue("PaymentCardId", out string? newId);
            return Results.Ok(newId);
        })
        .WithName("CreatePaymentCardInfo");

        group.MapDelete("/{paymentcardid}", async (int paymentcardid) =>
        {
            return (await DBPaymentCardServices.DeletePaymentCardInfo(paymentcardid))
                .ToOkOrNotFound($"No PaymentCardInfo found with id {paymentcardid} to delete.");
        })
        .WithName("DeletePaymentCardInfo");
    }
}
