using Azure.Core;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Keys.Cryptography;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Smartersoft.Identity.Client.Assertion
{
    /// <summary>
    /// Generator for Signed client assertions
    /// </summary>
    /// <remarks>Inspired by https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-net-client-assertions </remarks>
    public static class ClientAssertionGenerator
    {
        /// <summary>
        /// Encoded a byte array to a Base64Url encoded string.
        /// </summary>
        /// <param name="input">byte array</param>
        /// <returns>string</returns>
        public static string Base64UrlEncode(byte[] input)
        {
            char Base64PadCharacter = '=';
            char Base64Character62 = '+';
            char Base64Character63 = '/';
            char Base64UrlCharacter62 = '-';
            char Base64UrlCharacter63 = '_';

            string s = Convert.ToBase64String(input);
            s = s.Split(Base64PadCharacter)[0]; // Remove any trailing padding
            s = s.Replace(Base64Character62, Base64UrlCharacter62); // 62nd char of encoding
            s = s.Replace(Base64Character63, Base64UrlCharacter63); // 63rd char of encoding

            return s;
        }

        /// <summary>
        /// Generate the required claims for a client assertion
        /// </summary>
        /// <param name="audience">Audience token is used for eg `https://login.microsoftonline.com/{tenantId}/v2.0` </param>
        /// <param name="clientId">Client ID of the calling application</param>
        /// <param name="lifetime">optional lifetime</param>
        /// <returns></returns>
        public static IDictionary<string, object> GenerateClaimsForAudience(string audience, string clientId, int lifetime = 300)
        {

            DateTimeOffset validFrom = DateTimeOffset.UtcNow;
            DateTimeOffset validUntil = validFrom.AddSeconds(lifetime);
            return new Dictionary<string, object>()
            {
                { "aud", audience },
                { "exp", validUntil.ToUnixTimeSeconds() },
                { "iss", clientId },
                { "jti", Guid.NewGuid().ToString() },
                { "nbf", validFrom.ToUnixTimeSeconds() },
                { "sub", clientId }
            };
        }

        /// <summary>
        /// Generate the required claims for a client assertion
        /// </summary>
        /// <param name="tenantId">Tenant ID for which this token will be used</param>
        /// <param name="clientId">Client ID of the calling application</param>
        /// <param name="lifetime">optional lifetime</param>
        /// <returns></returns>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("Use version with audience")]
        public static IDictionary<string, object> GenerateClaimsForTenant(string tenantId, string clientId, int lifetime = 300)
        {
            string aud = $"https://login.microsoftonline.com/{tenantId}/v2.0";
            return GenerateClaimsForAudience(aud, clientId, lifetime);
        }


        /// <summary>
        /// Generate the JWT header for the client assertion
        /// </summary>
        /// <param name="kid">Base64Url encoded hash of the certificate</param>
        /// <returns></returns>
        public static IDictionary<string, string> GenerateHeader(string kid)
        {
            return new Dictionary<string, string>()
            {
                { "alg", "RS256"},
                { "typ", "JWT"},
                { "x5t", kid }
            };
        }

        /// <summary>
        /// Generate the first two parts of the client assertion (no signature)
        /// </summary>
        /// <param name="kid">Base64Url encoded hash of the certificate</param>
        /// <param name="assertionClaims">Client assertion claims</param>
        /// <returns></returns>
        public static string GetUnsignedToken(string kid, IDictionary<string, object> assertionClaims)
        {
            var header = GenerateHeader(kid);

            var headerBytes = JsonSerializer.SerializeToUtf8Bytes(header);
            var claimsBytes = JsonSerializer.SerializeToUtf8Bytes(assertionClaims);
            return Base64UrlEncode(headerBytes) + "." + Base64UrlEncode(claimsBytes);
        }

        /// <summary>
        /// Generate the first two parts of the client assertion (no signature)
        /// </summary>
        /// <param name="kid">Base64Url encoded hash of the certificate</param>
        /// <param name="tenantId">Tenant ID for which this token will be used</param>
        /// <param name="clientId">Client ID of the calling application</param>
        /// <returns></returns>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("Use version with audience")]
        public static string GetUnsignedToken(string kid, string tenantId, string clientId)
        {
            return GetUnsignedToken(kid, GenerateClaimsForTenant(tenantId, clientId));
        }

        /// <summary>
        /// Creates a signed client assertion, with a provided certificate.
        /// </summary>
        /// <param name="certificate">X509Certificate2, with private key included!</param>
        /// <param name="tenantId">Tenant ID for which this token will be used</param>
        /// <param name="clientId">Client ID of the calling application</param>
        /// <remarks>Provided only as a reference, use WithClientCertificate on the ConfidentialAppBuilder.</remarks>
        /// <returns></returns>
        public static string GetSignedToken(X509Certificate2 certificate, string tenantId, string clientId)
        {
            var kid = Base64UrlEncode(certificate.GetCertHash());
            var unsignedToken = GetUnsignedToken(kid, tenantId, clientId);
            var rsa = certificate.GetRSAPrivateKey();

            if (rsa == null)
            {
                throw new ArgumentNullException(nameof(certificate));
            }

            string signature = Base64UrlEncode(rsa.SignData(Encoding.UTF8.GetBytes(unsignedToken), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
            return unsignedToken + "." + signature;
        }

        /// <summary>
        /// Create a signed client assertion with a Key in the KeyVault
        /// </summary>
        /// <param name="assertionClaims">Claims in client assertion, use `GenerateClaimsForAudience` or `GenerateClaimsForTenant`</param>
        /// <param name="keyId">KeyId, Uri of the actual key in the KeyVault</param>
        /// <param name="kid">The Base64Url encoded hash of the certificate, use GetCertificateInfoFromKeyVault</param>
        /// <param name="tokenCredential">Use any TokenCredential (eg. new DefaultTokenCredential())</param>
        /// <param name="cancellationToken">Use cancellation token if preferred</param>
        /// <remarks>Needs Key => Sign permission, the client assertion is signed in the KeyVault</remarks>
        /// <returns>Signed client assertion</returns>
        public static async Task<string> GetSignedTokenWithKeyVaultKey(IDictionary<string, object> assertionClaims, Uri keyId, string kid, TokenCredential tokenCredential, CancellationToken cancellationToken = default)
        {
            var unsignedToken = GetUnsignedToken(kid, assertionClaims);
            var cryptographyClient = new CryptographyClient(keyId, tokenCredential);

            // The signing takes place at the KeyVault, the private key never reaches the client.
            // This needs the `Key => Sign` permission, and counts as a KeyVault operation.
            var signResult = await cryptographyClient.SignDataAsync(SignatureAlgorithm.RS256, Encoding.UTF8.GetBytes(unsignedToken), cancellationToken);

            return unsignedToken + "." + Base64UrlEncode(signResult.Signature);
        }

        /// <summary>
        /// Create a signed client assertion with a Key in the KeyVault
        /// </summary>
        /// <param name="tenantId">Tenant ID for which you want to use this token</param>
        /// <param name="clientId">Client Identifier</param>
        /// <param name="keyId">KeyId, Uri of the actual key in the KeyVault</param>
        /// <param name="kid">The Base64Url encoded hash of the certificate, use GetCertificateInfoFromKeyVault</param>
        /// <param name="tokenCredential">Use any TokenCredential (eg. new DefaultTokenCredential())</param>
        /// <param name="cancellationToken">Use cancellation token if preferred</param>
        /// <remarks>Needs Key => Sign permission, the client assertion is signed in the KeyVault</remarks>
        /// <returns>Signed client assertion</returns>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("Use version with audience")]
        public static Task<string> GetSignedTokenWithKeyVaultKey(string tenantId, string clientId, Uri keyId, string kid, TokenCredential tokenCredential, CancellationToken cancellationToken = default)
        {
            return GetSignedTokenWithKeyVaultKey(GenerateClaimsForTenant(tenantId, clientId), keyId, kid, tokenCredential, cancellationToken);
        }

        /// <summary>
        /// Create a signed client assertion with a Key in the KeyVault
        /// </summary>
        /// <param name="keyId">KeyId, Uri of the actual key in the KeyVault</param>
        /// <param name="kid">The Base64Url encoded hash of the certificate, use GetCertificateInfoFromKeyVault</param>
        /// <param name="audience">audience to use in the assertion</param>
        /// <param name="clientId">Client Identifier</param>
        /// <param name="tokenCredential">Use any TokenCredential (eg. new DefaultTokenCredential())</param>
        /// <param name="cancellationToken">Use cancellation token if preferred</param>
        /// <remarks>Needs Key => Sign permission, the client assertion is signed in the KeyVault</remarks>
        /// <returns>Signed client assertion</returns>
        public static Task<string> GetSignedTokenWithKeyVaultKey(Uri keyId, string kid, string audience, string clientId, TokenCredential tokenCredential, CancellationToken cancellationToken = default)
        {
            return GetSignedTokenWithKeyVaultKey(GenerateClaimsForAudience(audience, clientId), keyId, kid, tokenCredential, cancellationToken);
        }

        /// <summary>
        /// Get the KeyId and the kid from the KeyVault, this info should be cached. It will hardly ever change.
        /// </summary>
        /// <param name="vaultUri">Uri of your KeyVault</param>
        /// <param name="certificateName">Name of the certificate</param>
        /// <param name="tokenCredential">Use any TokenCredential (eg. new DefaultTokenCredential())</param>
        /// <param name="cancellationToken">Use cancellation token if preferred</param>
        /// <remarks>Calls GetCertificate, which will download the public information about the certificate</remarks>
        /// <returns>CertificateInfo</returns>
        public static async Task<CertificateInfo> GetCertificateInfoFromKeyVault(Uri vaultUri, string certificateName, TokenCredential tokenCredential, CancellationToken cancellationToken = default)
        {
            var certClient = new CertificateClient(vaultUri, tokenCredential);

            // This call needs the `GetCertificate` permission, but it only downlods the public info.
            // Be sure to mark the certificates' private key as NOT EXPORTABLE upon generation. That means no one can download the private key EVER!
            var certificateResult = await certClient.GetCertificateAsync(certificateName, cancellationToken);

            var certificate = new X509Certificate2(certificateResult.Value.Cer);

            return new CertificateInfo
            {
                CertificateName = certificateResult.Value.Name,
                KeyId = certificateResult.Value.KeyId,
                Kid = Base64UrlEncode(certificate.GetCertHash())
            };
        }

        /// <summary>
        /// Fetches information about the certificate (should be cached!), and then signs a token with the info from the KeyVault
        /// </summary>
        /// <param name="assertionClaims">Claims in client assertion, use `GenerateClaimsForAudience` or `GenerateClaimsForTenant`</param>
        /// <param name="vaultUri">Uri of the KeyVault</param>
        /// <param name="certificateName">Name of certificate</param>
        /// <param name="tokenCredential">Use any TokenCredential (eg. new DefaultTokenCredential())</param>
        /// <param name="cancellationToken">Use cancellation token if preferred</param>
        /// <returns>Signed client assertion</returns>
        /// <remarks>`GetSignedTokenWithKeyVaultKey` is perferred over this method</remarks>
        public static async Task<string> GetSignedTokenWithKeyVaultCertificate(IDictionary<string, object> assertionClaims, Uri vaultUri, string certificateName, TokenCredential tokenCredential, CancellationToken cancellationToken = default)
        {
            var certInfo = await GetCertificateInfoFromKeyVault(vaultUri, certificateName, tokenCredential, cancellationToken);

            if (certInfo.Kid == null || certInfo.KeyId == null)
            {
                throw new ArgumentNullException(nameof(certInfo));
            }

            return await GetSignedTokenWithKeyVaultKey(assertionClaims, certInfo.KeyId, certInfo.Kid, tokenCredential, cancellationToken);
        }

        /// <summary>
        /// Fetches information about the certificate (should be cached!), and then signs a token with the info from the KeyVault
        /// </summary>
        /// <param name="tenantId">Tenant ID for which you want to use this token</param>
        /// <param name="clientId">Client Identifier</param>
        /// <param name="vaultUri">Uri of the KeyVault</param>
        /// <param name="certificateName">Name of certificate</param>
        /// <param name="tokenCredential">Use any TokenCredential (eg. new DefaultTokenCredential())</param>
        /// <param name="cancellationToken">Use cancellation token if preferred</param>
        /// <returns>Signed client assertion</returns>
        /// <remarks>`GetSignedTokenWithKeyVaultKey` is perferred over this method</remarks>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("Use version with audience")]
        public static Task<string> GetSignedTokenWithKeyVaultCertificate(string tenantId, string clientId, Uri vaultUri, string certificateName, TokenCredential tokenCredential, CancellationToken cancellationToken = default)
        {
            return GetSignedTokenWithKeyVaultCertificate(GenerateClaimsForTenant(tenantId, clientId), vaultUri, certificateName, tokenCredential, cancellationToken);
        }

        /// <summary>
        /// Fetches information about the certificate (should be cached!), and then signs a token with the info from the KeyVault
        /// </summary>

        /// <param name="vaultUri">Uri of the KeyVault</param>
        /// <param name="certificateName">Name of certificate</param>
        /// <param name="audience">Assertion audience</param>
        /// <param name="clientId">Client Identifier</param>
        /// <param name="tokenCredential">Use any TokenCredential (eg. new DefaultTokenCredential())</param>
        /// <param name="cancellationToken">Use cancellation token if preferred</param>
        /// <returns>Signed client assertion</returns>
        /// <remarks>`GetSignedTokenWithKeyVaultKey` is perferred over this method</remarks>
        public static Task<string> GetSignedTokenWithKeyVaultCertificate(Uri vaultUri, string certificateName, string audience, string clientId, TokenCredential tokenCredential, CancellationToken cancellationToken = default)
        {
            return GetSignedTokenWithKeyVaultCertificate(GenerateClaimsForAudience(audience, clientId), vaultUri, certificateName, tokenCredential, cancellationToken);
        }
    }
}
