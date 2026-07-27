using CybsClass.Cybersource.Models.DTOs;
using System.Text.Json.Nodes;

namespace CybsClass.WebApi.Service.Services.ApplePay
{
    // Mirrors CcAuthService — a thin orchestrator the endpoint calls. Persistence is handled by
    // the endpoint itself (PersistApplePayTransaction), exactly like api/authtransaction calls
    // PersistCustomerData.InsertCustomers after CcAuthService.CallForAuth returns.
    public class CcApplePayService
    {
        public async Task<JsonObject> CallForAuth(B2cCustomerDto b2cCustomerDto)
        {
            return await CallForApplePayAuth.RunAsyncJsonObject(b2cCustomerDto);
        }
    }
}
