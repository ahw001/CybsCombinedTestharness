using CybsClass.Cybersource.Models.BaseData;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Models.OutboundTransObjects;
using CybsClass.Cybersource.Transactions;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CybsClass.WebApi.Service.Services.PayByLinkProcessing;

public static class CallForCybsPayByLinkCreate
{
    private const string Resource = "/ipl/v2/payment-links/";

    public static async Task<JsonObject> RunAsyncJsonObject(PayByLinkRequestDto dto)
    {
        JsonObject? jsonObject = new JsonObject();

        try
        {
            string purchaseNumber = Guid.NewGuid().ToString("N");

            string totalAmountStr = (dto.TotalAmount ?? 0m).ToString("0.00");

            var lineItem = new LineItems
            {
                ProductName = dto.ProductName ?? "Pay by Link Payment",
                UnitPrice = dto.TotalAmount ?? 0m,
                Quantity = 1
            };

            var payByLinkData = new PayByLinkData
            {
                ProcessingInformation = new PayByLinkProcessingInformation
                {
                    LinkType = "PURCHASE"
                },
                PurchaseInformation = new PurchaseInformation
                {
                    PurchaseNumber = purchaseNumber
                },
                OrderInformation = new OrderInformation
                {
                    AmountDetails = new AmountDetails
                    {
                        Currency = dto.Currency ?? "USD",
                        TotalAmount = totalAmountStr
                    },
                    LineItems = new List<LineItems> { lineItem }
                }
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            string jsonString = JsonSerializer.Serialize(payByLinkData, options);

            Console.WriteLine("\n************* CALLING FOR PAY BY LINK CREATE *****\n");
            Console.WriteLine($"Inbound DTO: {JsonSerializer.Serialize(dto, options)}");
            Console.WriteLine($"CyberSource Request: {jsonString}");

            jsonObject = await CallCyberSource.CallCyberSourceApiJson(jsonString, Resource, false) ?? new JsonObject();

            // Embed the purchase number so the persist layer can retrieve it
            if (jsonObject is not null && !jsonObject.ContainsKey("purchaseNumber"))
            {
                jsonObject["purchaseNumber"] = purchaseNumber;
            }

            Console.WriteLine($"CyberSource Response: {JsonSerializer.Serialize(jsonObject, options)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR PAY BY LINK CREATE: {ex.Message}");
            jsonObject["Exception"] = "ERROR PAY BY LINK CREATE: " + ex.Message;
        }

        return jsonObject ?? new JsonObject();
    }
}
