using CybsClass.Cybersource.Authentication;
using CybsClass.Cybersource.Models.BaseData;
using CybsClass.EntityModels;

namespace CybsClass.WebApi.Service.Services.MerchantBoarding;

// Pre-flight check run before any live submission for an org/merchant that declares
// a BoardingPortfolio. The portfolio's ExpectedSignatureMerchantId is a logical value
// only (not a hard link to the .p12 credential file) — this simply catches the case
// where the running server's currently-loaded credential doesn't match what the
// portfolio expects, instead of silently submitting under the wrong signer.
public static class BoardingPortfolioGuard
{
    public static CybsClass.Cybersource.Models.BaseData.ErrorObject? CheckCredentialMatch(BoardingPortfolio? portfolio)
    {
        if (portfolio is null || string.IsNullOrWhiteSpace(portfolio.ExpectedSignatureMerchantId))
            return null;

        var activeSignatureMerchantId = BoardingCredentials.GetSignatureMerchantId();

        if (string.Equals(portfolio.ExpectedSignatureMerchantId, activeSignatureMerchantId, StringComparison.OrdinalIgnoreCase))
            return null;

        return new CybsClass.Cybersource.Models.BaseData.ErrorObject
        {
            Error = "Portfolio Credential Mismatch",
            Message = $"Organization is configured for portfolio '{portfolio.PortfolioName}' (expects signer '{portfolio.ExpectedSignatureMerchantId}') " +
                      $"but the server is currently running under signer '{activeSignatureMerchantId ?? "(none)"}'.",
            Action = "Restart the server with the matching RestP12JwtCredential/MerchantID in appsettings.json for this portfolio, or re-select the correct portfolio for this organization.",
            Reason = "Boarding submissions must be signed with the credential the organization's portfolio expects."
        };
    }
}
