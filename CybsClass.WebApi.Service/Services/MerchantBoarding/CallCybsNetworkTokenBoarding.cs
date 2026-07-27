using CybsClass.Cybersource.Models.BaseData.Boarding;
using CybsClass.Cybersource.Transactions;
using CybsClass.EntityModels;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CybsClass.WebApi.Service.Services.MerchantBoarding;

// Sole submission path for Token Management / Network Token boarding — fully
// decoupled from the general BRS "Board to Cybersource" merchant flow. Sends
// exactly the PECS NT-enablement shape CyberSource documents, with only
// organizationId, businessInformation.name/doingBusinessAs/websiteUrl, and
// acquirer.acquirerMerchantId configurable per merchant; everything else
// (vault, token formats, address, acquirerId, all service-enable flags) is a
// hardcoded constant matching the required example payload.
public static class CallCybsNetworkTokenBoarding
{
    private static readonly JsonSerializerOptions _opts = new()
    {
        WriteIndented          = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static async Task<JsonObject> RunAsync(BoardingTokenManagementSubscription ntSub, string fallbackOrganizationId)
    {
        JsonObject jsonObject = new();
        string resource = "/products/v1/product-setups";

        try
        {
            var request = BuildRequest(ntSub, fallbackOrganizationId);
            string jsonString = JsonSerializer.Serialize(request, _opts);

            Console.WriteLine("\n************* CALLING PECS NT BOARDING *****\n");
            Console.WriteLine($"\n[NtBoarding] REQUEST JSON: {jsonString}");

            jsonObject = await CallCyberSource.CallCyberSourceApiJson(jsonString, resource, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NtBoarding] ERROR: {ex.Message}");
            jsonObject = new JsonObject();
            jsonObject.Add("error", ex.Message);
        }

        Console.WriteLine($"\n[NtBoarding] RESPONSE JSON: {jsonObject.ToJsonString(_opts)}");
        return jsonObject;
    }

    private static NtPecsBoardingRequestDto BuildRequest(BoardingTokenManagementSubscription s, string fallbackOrganizationId) =>
        new()
        {
            OrganizationId = string.IsNullOrWhiteSpace(s.OrganizationId) ? fallbackOrganizationId : s.OrganizationId,
            CommerceSolutions = new NtPecsCommerceSolutionsDto
            {
                TokenManagement = new NtPecsProductDto
                {
                    SubscriptionInformation = new NtTmsSubscriptionInformationDto { Enabled = true },
                    ConfigurationInformation = new NtPecsConfigurationInformationDto
                    {
                        Configurations = BuildConfigurations(s)
                    }
                }
            }
        };

    private static NtTmsConfigurationsDto BuildConfigurations(BoardingTokenManagementSubscription s) =>
        new()
        {
            Vault = new NtTmsVaultDto
            {
                Location         = "GDC",
                DefaultTokenType = "CUSTOMER",
                TokenFormats = new NtTmsTokenFormatsDto
                {
                    Customer                         = "32_HEX",
                    PaymentInstrument                = "32_HEX",
                    InstrumentIdentifierCard          = "19_DIGIT_LAST_4",
                    InstrumentIdentifierBankAccount   = "32_HEX",
                },
                SensitivePrivileges = new NtTmsSensitivePrivilegesDto
                {
                    CardNumberMaskingFormat = "FIRST_6_LAST_4"
                }
            },
            NetworkTokenEnrollment = new NtTmsNetworkTokenEnrollmentDto
            {
                BusinessInformation = new NtTmsEnrollmentBusinessInfoDto
                {
                    Name            = s.BusinessName,
                    DoingBusinessAs = s.DoingBusinessAs,
                    Address = new NtTmsEnrollmentAddressDto
                    {
                        Country  = "US",
                        Locality = "ORMOND BEACH"
                    },
                    WebsiteUrl = s.WebsiteUrl,
                    Acquirer = new NtTmsEnrollmentAcquirerDto
                    {
                        AcquirerId         = "40010052242",
                        AcquirerMerchantId = s.AcquirerMerchantId
                    }
                },
                NetworkTokenServices = new NtTmsEnrollmentNetworkTokenServicesDto
                {
                    VisaTokenService                   = new NtTmsSchemeEnrollmentDto { Enrollment = true },
                    MastercardDigitalEnablementService  = new NtTmsSchemeEnrollmentDto { Enrollment = true },
                }
            },
            NetworkTokenServices = new NtTmsConfigurationsNetworkTokenServicesDto
            {
                Notifications        = new NtTmsEnabledDto { Enabled = true },
                PaymentCredentials   = new NtTmsEnabledDto { Enabled = true },
                VisaTokenService     = new NtTmsSchemeServiceDto { EnableService = true, EnableTransactionalTokens = true },
                MastercardDigitalEnablementService = new NtTmsSchemeServiceDto { EnableService = true, EnableTransactionalTokens = true },
            }
        };
}
