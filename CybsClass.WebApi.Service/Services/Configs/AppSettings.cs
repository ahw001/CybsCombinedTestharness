namespace CybsClass.WebApi.Service.Services.Configs
{
    public class AppSettings
    {
        public LoggingSettings Logging { get; set; } = new();
        public string? AllowedHosts { get; set; }
        public string? BaseUrlAddress { get; set; }
        public string? BasePosUrlAddress { get; set; }
        public AuthSecretKeySettings AuthSecretKey { get; set; } = new();
        public AcceptanceDeviceInfoSettings AcceptanceDeviceInfo { get; set; } = new();
        public AuthCredentialFileSettings AuthCredentialFile { get; set; } = new();
        public CorsSettings Cors { get; set; } = new();
        public MleSettings MleSettings { get; set; } = new();
        public ApplePaySettings ApplePaySettings { get; set; } = new();
    }

    public class LoggingSettings
    {
        public LogLevelSettings LogLevel { get; set; } = new();
    }

    public class LogLevelSettings
    {
        public string? Default { get; set; }
        public string? MicrosoftAspNetCore { get; set; }
    }

    public class AuthSecretKeySettings
    {
        public string? KeyId { get; set; }
        public string? SharedSecret { get; set; }
    }

    public class AcceptanceDeviceInfoSettings
    {
        public string? AcceptanceMerchantId { get; set; }
        public string? AcceptanceSecret { get; set; }
        public string? AcceptanceDeviceSerialNumber { get; set; }
    }

    public class AuthCredentialFileSettings
    {
        public string? RestP12JwtCredential { get; set; }
        public string? IsPortfolioCredential { get; set; }
        public string? MerchantID { get; set; }
        public string? KeyPass { get; set; }
    }

    public class CorsSettings
    {
        public List<string> AllowedOrigins { get; set; } = new();
    }

    public class MleSettings
    {
        public string? SjcCertificatePath { get; set; }
        public string? ResponseMleKeyPath { get; set; }
        public string? ResponseMleKeyPass { get; set; }
        public string? ResponseMleKid { get; set; }
        public string? LegacyMlePrivateKeyPath { get; set; }
        public string? LegacyMleKid { get; set; }
    }

    public class ApplePaySettings
    {
        // Apple Pay Payment Processing Certificate — this is the MERCHANT DECRYPTION key pair.
        // Not the same certificate/purpose as MleSettings (CyberSource System 1/2 MLE).
        public string? PaymentProcessingCertPath { get; set; }
        public string? PaymentProcessingKeyPath { get; set; }
        public string? KeyPass { get; set; }
        public string? MerchantIdentifier { get; set; }

        // Merchant Identity Certificate — used for mutual-TLS to Apple's onvalidatemerchant
        // validationURL. Distinct from the Payment Processing Certificate above (that one
        // decrypts the payment token; this one authenticates merchant-validation requests).
        public string? MerchantIdentityCertPath { get; set; }
        public string? MerchantIdentityCertPass { get; set; }
        public string? InitiativeContext { get; set; }
    }
}
