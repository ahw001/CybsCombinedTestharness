using CybsClass.Cybersource.Transactions;
using System.Text.Json.Nodes;

namespace CybsClass.WebApi.Service.Services.PayByLinkProcessing;

public static class CallForCybsPayByLinkStatus
{
    public static async Task<JsonObject> RunAsyncJsonObject(string cybersourceLinkId)
    {
        JsonObject jsonObject = new JsonObject();

        try
        {
            string resource = $"/ipl/v2/payment-links/{cybersourceLinkId}";

            Console.WriteLine($"\n************* CALLING FOR PAY BY LINK STATUS CHECK *****\n");
            Console.WriteLine($"CyberSource Link ID: {cybersourceLinkId}");

            jsonObject = await CallCyberSource.CallCyberSourceApiGet(resource, false);

            Console.WriteLine($"CyberSource Status Response: {jsonObject}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR PAY BY LINK STATUS CHECK: {ex.Message}");
            jsonObject["Exception"] = "ERROR PAY BY LINK STATUS CHECK: " + ex.Message;
        }

        return jsonObject;
    }
}
