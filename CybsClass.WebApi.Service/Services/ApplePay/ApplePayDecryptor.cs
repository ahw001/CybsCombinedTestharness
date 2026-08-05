using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace CybsClass.WebApi.Service.Services.ApplePay
{
    // Decrypts an Apple Pay "EC_v1" PKPaymentToken.paymentData payload — this is the MERCHANT
    // DECRYPTION step: we hold the Payment Processing Certificate private key and decrypt the
    // token locally instead of asking CyberSource to do it. Mirrors the static-helper shape of
    // CybsClass.WebApi.Service.Services.GooglePay.GooglePayDecryptor, but Apple's envelope/KDF
    // differ from Google's (Concatenation KDF w/ SHA-256 vs HKDF, all-zero 16-byte IV vs 12-byte).
    //
    // Apple Pay "Payment Token Format Reference" EC_v1 algorithm:
    //   1. sharedSecret = ECDH(merchant Payment Processing private key, ephemeralPublicKey)
    //   2. info = "id-aes256-GCM" (ASCII) + SHA256(UTF8 bytes of the merchant identifier)
    //   3. symmetricKey = SHA256( 0x00000001 || sharedSecret || info )   (NIST SP 800-56A Concat KDF, 1 round)
    //   4. plaintext = AES-256-GCM decrypt(data) with a 16-byte all-zero IV; the last 16 bytes of
    //      "data" are the GCM auth tag appended to the ciphertext.
    public static class ApplePayDecryptor
    {
        private class ApplePayHeader
        {
            [JsonPropertyName("ephemeralPublicKey")]
            public string? EphemeralPublicKey { get; set; }

            [JsonPropertyName("publicKeyHash")]
            public string? PublicKeyHash { get; set; }

            [JsonPropertyName("transactionId")]
            public string? TransactionId { get; set; }
        }

        private class ApplePayPaymentData
        {
            [JsonPropertyName("data")]
            public string? Data { get; set; }

            [JsonPropertyName("header")]
            public ApplePayHeader? Header { get; set; }

            [JsonPropertyName("signature")]
            public string? Signature { get; set; }

            [JsonPropertyName("version")]
            public string? Version { get; set; }
        }

        /// <summary>
        /// Decrypts the raw JSON of PKPaymentToken.paymentData (the "EC_v1" envelope) and
        /// returns the decrypted token JSON string, containing the DPAN, expiry, and
        /// onlinePaymentCryptogram — mirroring Apple's "Payment Data Dictionary" shape.
        /// Signature verification against Apple's root CA is intentionally NOT performed here
        /// (test-environment scope) — only decryption.
        /// </summary>
        public static string Decrypt(string paymentDataJson, ECDiffieHellman merchantPrivateKey, string merchantIdentifier)
        {
            if (merchantPrivateKey is null)
                throw new InvalidOperationException("Apple Pay Payment Processing private key is not loaded — check ApplePaySettings.");

            var paymentData = JsonSerializer.Deserialize<ApplePayPaymentData>(paymentDataJson)
                ?? throw new ArgumentException("paymentData JSON could not be parsed.", nameof(paymentDataJson));

            if (!string.Equals(paymentData.Version, "EC_v1", StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException($"Unsupported Apple Pay payment data version: {paymentData.Version}. Only EC_v1 is implemented.");

            if (string.IsNullOrEmpty(paymentData.Header?.EphemeralPublicKey) || string.IsNullOrEmpty(paymentData.Data))
                throw new ArgumentException("paymentData is missing header.ephemeralPublicKey or data.");

            // 1. ECDH shared secret — ephemeralPublicKey is a base64 DER SubjectPublicKeyInfo (P-256).
            var ephemeralPubKeyBytes = Convert.FromBase64String(paymentData.Header.EphemeralPublicKey);
            using var ephemeralEcdh = ECDiffieHellman.Create();
            ephemeralEcdh.ImportSubjectPublicKeyInfo(ephemeralPubKeyBytes, out _);

            byte[] sharedSecret = merchantPrivateKey.DeriveRawSecretAgreement(ephemeralEcdh.PublicKey);

            // 2 & 3. NIST SP 800-56A Concatenation KDF (single round) with SHA-256.
            byte[] symmetricKey = DeriveSymmetricKey(sharedSecret, merchantIdentifier);

            // 4. AES-256-GCM decrypt. "data" = ciphertext || 16-byte tag. IV is 16 zero bytes.
            //
            // .NET's built-in System.Security.Cryptography.AesGcm only supports a 12-byte nonce
            // (confirmed via AesGcm.NonceByteSizes on this platform: MinSize=MaxSize=12) — it has
            // no implementation of GCM's GHASH-based J0 derivation required for non-96-bit IVs.
            // Apple's real EC_v1 payloads use a 16-byte IV, so decrypting them requires a GCM
            // implementation that supports that per-spec — hence BouncyCastle here (the
            // "necessary for cryptography" exception in CLAUDE.md's hard constraints).
            byte[] cipherAndTag = Convert.FromBase64String(paymentData.Data);
            if (cipherAndTag.Length < 16)
                throw new ArgumentException("paymentData.data is too short to contain a GCM tag.");

            const int tagLengthBits = 128;
            byte[] iv = new byte[16]; // Apple Pay EC_v1 uses an all-zero 16-byte IV.

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(symmetricKey), tagLengthBits, iv);
            cipher.Init(forEncryption: false, parameters);

            byte[] plaintext = new byte[cipher.GetOutputSize(cipherAndTag.Length)];
            int len = cipher.ProcessBytes(cipherAndTag, 0, cipherAndTag.Length, plaintext, 0);
            len += cipher.DoFinal(plaintext, len); // throws InvalidCipherTextException on tag mismatch

            return Encoding.UTF8.GetString(plaintext, 0, len);
        }

        /// <summary>
        /// NIST SP 800-56A §5.8.1 Concatenation KDF, single round, SHA-256:
        ///
        ///     SHA256( counter(0x00000001) || Z || OtherInfo )
        ///     OtherInfo = AlgorithmID || PartyUInfo || PartyVInfo
        ///
        /// Apple specifies those three fields exactly (Payment Token Format Reference,
        /// "Restoring the Symmetric Key"):
        ///   AlgorithmID — the LENGTH BYTE 0x0D followed by ASCII "id-aes256-GCM"
        ///   PartyUInfo  — the ASCII string "Apple"
        ///   PartyVInfo  — SHA-256 of the merchant identifier
        ///
        /// Both the 0x0D length prefix and the "Apple" PartyUInfo were missing here until
        /// 2026-08-05, so every real Apple token failed AES-GCM tag verification with
        /// "mac check in GCM failed". Verified against a genuine EC_v1 token: omitting EITHER
        /// field still fails, so both are load-bearing.
        ///
        /// Note the original round-trip unit test could not catch this — it encrypted its test
        /// payload with this same method before decrypting it, so any systematic deviation from
        /// Apple's format cancelled out. Only a real Apple token discriminates. Do not treat a
        /// self-generated round-trip as validation of this function.
        /// </summary>
        private static byte[] DeriveSymmetricKey(byte[] sharedSecret, string merchantIdentifier)
        {
            byte[] merchantIdHash = SHA256.HashData(Encoding.UTF8.GetBytes(merchantIdentifier ?? string.Empty));
            byte[] algorithmId = Encoding.ASCII.GetBytes("id-aes256-GCM");
            byte[] partyUInfo = Encoding.ASCII.GetBytes("Apple");
            byte[] counter = { 0x00, 0x00, 0x00, 0x01 };

            using var buffer = new MemoryStream();
            buffer.Write(counter, 0, counter.Length);
            buffer.Write(sharedSecret, 0, sharedSecret.Length);
            buffer.WriteByte((byte)algorithmId.Length);          // 0x0D — AlgorithmID length prefix
            buffer.Write(algorithmId, 0, algorithmId.Length);
            buffer.Write(partyUInfo, 0, partyUInfo.Length);       // PartyUInfo
            buffer.Write(merchantIdHash, 0, merchantIdHash.Length); // PartyVInfo

            return SHA256.HashData(buffer.ToArray());
        }
    }
}
