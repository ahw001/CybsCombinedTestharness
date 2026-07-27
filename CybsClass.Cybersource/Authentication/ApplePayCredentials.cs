using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CybsClass.Cybersource.Authentication
{
    // Loads the Apple Pay Payment Processing Certificate private key — the MERCHANT DECRYPTION
    // key used to decrypt PKPaymentToken.paymentData (EC_v1). This is a completely separate
    // credential from the CyberSource MLE keys in MleCredentials — do not conflate the two.
    public static class ApplePayCredentials
    {
        // EC (P-256) private key used for ECDH against the ephemeral public key in paymentData.header.
        public static ECDiffieHellman? PaymentProcessingPrivateKey { get; private set; }
        public static string? MerchantIdentifier { get; private set; }

        // Merchant Identity Certificate (cert + private key) — presented as a TLS client
        // certificate for mutual TLS to Apple's onvalidatemerchant validationURL. Kept as a full
        // X509Certificate2 (not just key material) since HttpClientHandler.ClientCertificates
        // needs the certificate itself, not a bare key.
        public static X509Certificate2? MerchantIdentityCertificate { get; private set; }
        public static string? InitiativeContext { get; private set; }

        public static void Initialize(string paymentProcessingKeyPath, string? keyPass, string? merchantIdentifier,
            string? merchantIdentityCertPath = null, string? merchantIdentityCertPass = null, string? initiativeContext = null)
        {
            MerchantIdentifier = merchantIdentifier;
            InitiativeContext = initiativeContext;

            InitializeMerchantIdentityCertificate(merchantIdentityCertPath, merchantIdentityCertPass);

            if (string.IsNullOrWhiteSpace(paymentProcessingKeyPath))
            {
                Console.WriteLine("[ApplePay] PaymentProcessingKeyPath not configured — Apple Pay decryption unavailable.");
                return;
            }

            var baseRes = Path.Combine(AppContext.BaseDirectory, "Resource");
            var resolvedKeyPath = Path.IsPathRooted(paymentProcessingKeyPath)
                ? paymentProcessingKeyPath
                : Path.Combine(baseRes, paymentProcessingKeyPath);

            Console.WriteLine($"[ApplePay] PaymentProcessingKeyPath={resolvedKeyPath} Exists={File.Exists(resolvedKeyPath)}");
            Console.WriteLine($"[ApplePay] MerchantIdentifier={merchantIdentifier}");

            if (!File.Exists(resolvedKeyPath))
            {
                Console.WriteLine("[ApplePay] Payment Processing private key file not found — decryption will fail until it is provided.");
                return;
            }

            try
            {
                if (resolvedKeyPath.EndsWith(".p12", StringComparison.OrdinalIgnoreCase))
                {
                    var cert = new X509Certificate2(
                        resolvedKeyPath, keyPass ?? string.Empty,
                        X509KeyStorageFlags.EphemeralKeySet);
                    var certEcdsa = cert.GetECDsaPrivateKey();
                    if (certEcdsa is null)
                    {
                        Console.WriteLine("[ApplePay] ERROR: P12 does not contain an EC private key.");
                        return;
                    }
                    var ecdh = ECDiffieHellman.Create();
                    ecdh.ImportParameters(certEcdsa.ExportParameters(true));
                    PaymentProcessingPrivateKey = ecdh;
                }
                else
                {
                    string pem = File.ReadAllText(resolvedKeyPath);
                    using var ecdsa = ECDsa.Create();
                    ecdsa.ImportFromPem(pem);
                    var ecdh = ECDiffieHellman.Create();
                    ecdh.ImportParameters(ecdsa.ExportParameters(true));
                    PaymentProcessingPrivateKey = ecdh;
                }

                Console.WriteLine("[ApplePay] Payment Processing private key loaded.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApplePay] ERROR loading Payment Processing private key: {ex.Message}");
            }
        }

        private static void InitializeMerchantIdentityCertificate(string? merchantIdentityCertPath, string? merchantIdentityCertPass)
        {
            if (string.IsNullOrWhiteSpace(merchantIdentityCertPath))
            {
                Console.WriteLine("[ApplePay] MerchantIdentityCertPath not configured — onvalidatemerchant proxy unavailable.");
                return;
            }

            var baseRes = Path.Combine(AppContext.BaseDirectory, "Resource");
            var resolvedPath = Path.IsPathRooted(merchantIdentityCertPath)
                ? merchantIdentityCertPath
                : Path.Combine(baseRes, merchantIdentityCertPath);

            Console.WriteLine($"[ApplePay] MerchantIdentityCertPath={resolvedPath} Exists={File.Exists(resolvedPath)}");

            if (!File.Exists(resolvedPath))
            {
                Console.WriteLine("[ApplePay] Merchant Identity certificate file not found — onvalidatemerchant proxy will fail until it is provided.");
                return;
            }

            try
            {
                var cert = new X509Certificate2(resolvedPath, merchantIdentityCertPass ?? string.Empty,
                    X509KeyStorageFlags.EphemeralKeySet);
                if (!cert.HasPrivateKey)
                {
                    Console.WriteLine("[ApplePay] ERROR: Merchant Identity P12 has no private key.");
                    cert.Dispose();
                    return;
                }
                MerchantIdentityCertificate = cert;
                Console.WriteLine($"[ApplePay] Merchant Identity certificate loaded: {cert.Subject}, valid until {cert.NotAfter:yyyy-MM-dd}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApplePay] ERROR loading Merchant Identity certificate: {ex.Message}");
            }
        }
    }
}
