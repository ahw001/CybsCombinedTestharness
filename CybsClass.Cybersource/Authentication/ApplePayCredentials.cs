using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CybsClass.Cybersource.Authentication
{
    // Loads the Apple Pay Payment Processing Certificate private key — the MERCHANT DECRYPTION
    // key used to decrypt PKPaymentToken.paymentData (EC_v1). This is a completely separate
    // credential from the CyberSource MLE keys in MleCredentials — do not conflate the two.
    public static class ApplePayCredentials
    {
        // EC (P-256) private keys used for ECDH against the ephemeral public key in
        // paymentData.header. This is a LIST because a merchant may have more than one Apple Pay
        // Payment Processing Certificate registered at once — Apple encrypts each token to exactly
        // one of them and names it via header.publicKeyHash, so during a certificate rotation the
        // server must hold every key that Apple might still be encrypting to. Order is
        // preference order: the configured primary key first, then any additional keys.
        public static IReadOnlyList<ECDiffieHellman> PaymentProcessingPrivateKeys { get; private set; }
            = Array.Empty<ECDiffieHellman>();

        // Convenience accessor for the primary key — retained so existing callers keep working.
        public static ECDiffieHellman? PaymentProcessingPrivateKey
            => PaymentProcessingPrivateKeys.Count > 0 ? PaymentProcessingPrivateKeys[0] : null;
        public static string? MerchantIdentifier { get; private set; }

        // Merchant Identity Certificate (cert + private key) — presented as a TLS client
        // certificate for mutual TLS to Apple's onvalidatemerchant validationURL. Kept as a full
        // X509Certificate2 (not just key material) since HttpClientHandler.ClientCertificates
        // needs the certificate itself, not a bare key.
        public static X509Certificate2? MerchantIdentityCertificate { get; private set; }
        public static string? InitiativeContext { get; private set; }

        public static void Initialize(string paymentProcessingKeyPath, string? keyPass, string? merchantIdentifier,
            string? merchantIdentityCertPath = null, string? merchantIdentityCertPass = null, string? initiativeContext = null,
            IEnumerable<string>? additionalPaymentProcessingKeyPaths = null)
        {
            MerchantIdentifier = merchantIdentifier;
            InitiativeContext = initiativeContext;

            InitializeMerchantIdentityCertificate(merchantIdentityCertPath, merchantIdentityCertPass);

            var keyPaths = new List<string>();
            if (!string.IsNullOrWhiteSpace(paymentProcessingKeyPath))
                keyPaths.Add(paymentProcessingKeyPath);
            if (additionalPaymentProcessingKeyPaths is not null)
                keyPaths.AddRange(additionalPaymentProcessingKeyPaths.Where(p => !string.IsNullOrWhiteSpace(p)));

            if (keyPaths.Count == 0)
            {
                Console.WriteLine("[ApplePay] PaymentProcessingKeyPath not configured — Apple Pay decryption unavailable.");
                return;
            }

            Console.WriteLine($"[ApplePay] MerchantIdentifier={merchantIdentifier}");

            var loadedKeys = new List<ECDiffieHellman>();
            foreach (var keyPath in keyPaths)
            {
                var key = LoadPaymentProcessingKey(keyPath, keyPass);
                if (key is not null)
                    loadedKeys.Add(key);
            }

            PaymentProcessingPrivateKeys = loadedKeys;

            if (loadedKeys.Count == 0)
                Console.WriteLine("[ApplePay] ERROR: no Payment Processing private key could be loaded — decryption will fail.");
            else
                Console.WriteLine($"[ApplePay] Payment Processing private keys loaded: {loadedKeys.Count} of {keyPaths.Count} configured.");
        }

        // Loads one EC private key (PEM or P12). Returns null and logs on any failure — a single
        // bad path must not prevent the remaining keys from loading, since during a certificate
        // rotation the surviving key is what keeps decryption working.
        private static ECDiffieHellman? LoadPaymentProcessingKey(string paymentProcessingKeyPath, string? keyPass)
        {
            var baseRes = Path.Combine(AppContext.BaseDirectory, "Resource");
            var resolvedKeyPath = Path.IsPathRooted(paymentProcessingKeyPath)
                ? paymentProcessingKeyPath
                : Path.Combine(baseRes, paymentProcessingKeyPath);

            Console.WriteLine($"[ApplePay] PaymentProcessingKeyPath={resolvedKeyPath} Exists={File.Exists(resolvedKeyPath)}");

            if (!File.Exists(resolvedKeyPath))
            {
                Console.WriteLine("[ApplePay] Payment Processing private key file not found — skipping this key.");
                return null;
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
                        return null;
                    }
                    var ecdh = ECDiffieHellman.Create();
                    ecdh.ImportParameters(certEcdsa.ExportParameters(true));
                    return ecdh;
                }
                else
                {
                    string pem = File.ReadAllText(resolvedKeyPath);
                    using var ecdsa = ECDsa.Create();
                    ecdsa.ImportFromPem(pem);
                    var ecdh = ECDiffieHellman.Create();
                    ecdh.ImportParameters(ecdsa.ExportParameters(true));
                    return ecdh;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApplePay] ERROR loading Payment Processing private key '{resolvedKeyPath}': {ex.Message}");
                return null;
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
                // Key storage flags MUST be platform-dependent here — this is not cosmetic.
                //
                //   Linux (Azure App Service): EphemeralKeySet is required. There is no user key
                //   store to import into, so a persisted-key load fails outright. This is the
                //   original reason the flag was introduced.
                //
                //   Windows: EphemeralKeySet certificates CANNOT be used for TLS client
                //   authentication. SslStream rejects them at the transport layer with the opaque
                //   Win32Exception "The credentials supplied to the package were not recognized",
                //   so merchant validation fails before ever reaching Apple. UserKeySet imports a
                //   non-persisted key that IS usable for client auth and is cleaned up with the
                //   certificate object.
                //
                // Note the HasPrivateKey guard below does NOT catch this — it returns true for an
                // ephemeral key that SslStream will still refuse. Verified empirically 2026-08-04
                // by POSTing to Apple's gateway with each flag set.
                var keyStorageFlags = OperatingSystem.IsWindows()
                    ? X509KeyStorageFlags.UserKeySet
                    : X509KeyStorageFlags.EphemeralKeySet;

                var cert = new X509Certificate2(resolvedPath, merchantIdentityCertPass ?? string.Empty,
                    keyStorageFlags);
                if (!cert.HasPrivateKey)
                {
                    Console.WriteLine("[ApplePay] ERROR: Merchant Identity P12 has no private key.");
                    cert.Dispose();
                    return;
                }
                MerchantIdentityCertificate = cert;
                Console.WriteLine($"[ApplePay] Merchant Identity certificate loaded: {cert.Subject}, valid until {cert.NotAfter:yyyy-MM-dd} (keyStorageFlags={keyStorageFlags}).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApplePay] ERROR loading Merchant Identity certificate: {ex.Message}");
            }
        }
    }
}
