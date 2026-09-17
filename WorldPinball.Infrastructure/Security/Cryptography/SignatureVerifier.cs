using System.Security.Cryptography;

namespace WorldPinball.Infrastructure.Security
{
    public sealed class EcdsaP256SignatureVerifier : ISignatureVerifier
    {

        private const string AllowedAlg = "ecdsa-p256-sha256";

        public bool Verify(string publicKeyPem, byte[] data, string signatureBase64, string? algorithm = null)
     => VerifyEcdsaP256Sha256(publicKeyPem, data, signatureBase64, algorithm);

        /// <summary>
        /// Verify an ECDSA P-256 signature over the provided data (canonical bytes).
        /// - publicKeyPem: PEM "BEGIN PUBLIC KEY" (SubjectPublicKeyInfo)
        /// - signatureBase64: signature bytes encoded in base64. Can be DER or RAW (r||s) 64 bytes.
        /// </summary>
        private bool VerifyEcdsaP256Sha256(string publicKeyPem, byte[] data, string signatureBase64, string? algorithm = null)
        {


            if (!string.IsNullOrWhiteSpace(algorithm) &&
                !algorithm.Equals(AllowedAlg, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException($"Algorithm not supported: {algorithm}");
            if (string.IsNullOrWhiteSpace(publicKeyPem))
                throw new ArgumentException("Public key PEM is missing.", nameof(publicKeyPem));

            if (data is null || data.Length == 0)
                throw new ArgumentException("Data is missing.", nameof(data));

            if (string.IsNullOrWhiteSpace(signatureBase64))
                throw new ArgumentException("Signature is missing.", nameof(signatureBase64));

            byte[] sigBytes;
            try
            {
                sigBytes = Convert.FromBase64String(signatureBase64);
            }
            catch (FormatException)
            {
                throw new UnauthorizedAccessException("invalid X-Signature  (base64).");
            }

            // If signature looks like RAW r||s (64 bytes for P-256), convert to DER.
            // DER signatures are variable length (usually ~70-72 bytes for P-256).
            if (sigBytes.Length == 64)
            {
                sigBytes = RawEcdsaSignatureToDer(sigBytes);
            }

            try
            {
                using var ecdsa = ECDsa.Create();

                // Import public key. Expected PEM: -----BEGIN PUBLIC KEY----- ... -----END PUBLIC KEY-----
                ecdsa.ImportFromPem(publicKeyPem);

                // Verify signature over the data using SHA-256 (ECDSA P-256).
                return ecdsa.VerifyData(data, sigBytes, HashAlgorithmName.SHA256);
            }
            catch (CryptographicException ex)
            {
                // Wrong key format, wrong curve, invalid DER, etc.
                throw new UnauthorizedAccessException($"Vérification signature impossible: {ex.Message}");
            }
        }

        /// <summary>
        /// Convert a RAW ECDSA signature (r||s, 64 bytes for P-256) to DER ASN.1 SEQUENCE of two INTEGERs.
        /// </summary>
        private byte[] RawEcdsaSignatureToDer(byte[] rawSig)
        {
            if (rawSig.Length != 64)
                throw new ArgumentException("RAW ECDSA signature must be 64 bytes for P-256.");

            var r = new byte[32];
            var s = new byte[32];
            Buffer.BlockCopy(rawSig, 0, r, 0, 32);
            Buffer.BlockCopy(rawSig, 32, s, 0, 32);

            // ASN.1 INTEGER must be positive; if MSB is set, prepend 0x00.
            r = Asn1IntegerNormalize(r);
            s = Asn1IntegerNormalize(s);

            // DER: 0x30 len 0x02 lenR rBytes 0x02 lenS sBytes
            var totalLen = 2 + r.Length + 2 + s.Length;

            var der = new byte[2 + totalLen];
            int offset = 0;

            der[offset++] = 0x30;              // SEQUENCE
            der[offset++] = (byte)totalLen;

            der[offset++] = 0x02;              // INTEGER
            der[offset++] = (byte)r.Length;
            Buffer.BlockCopy(r, 0, der, offset, r.Length);
            offset += r.Length;

            der[offset++] = 0x02;              // INTEGER
            der[offset++] = (byte)s.Length;
            Buffer.BlockCopy(s, 0, der, offset, s.Length);

            return der;
        }


        private byte[] Asn1IntegerNormalize(byte[] x)
        {
            // Strip leading zeros
            int i = 0;
            while (i < x.Length - 1 && x[i] == 0x00) i++;
            var trimmed = new byte[x.Length - i];
            Buffer.BlockCopy(x, i, trimmed, 0, trimmed.Length);

            // If highest bit is set, prepend 0x00 to keep INTEGER positive
            if ((trimmed[0] & 0x80) != 0)
            {
                var prefixed = new byte[trimmed.Length + 1];
                prefixed[0] = 0x00;
                Buffer.BlockCopy(trimmed, 0, prefixed, 1, trimmed.Length);
                return prefixed;
            }

            return trimmed;
        }

    }
}
