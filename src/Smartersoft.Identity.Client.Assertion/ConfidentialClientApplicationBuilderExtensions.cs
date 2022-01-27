using Azure.Core;
using Azure.Identity;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Smartersoft.Identity.Client.Assertion
{
    /// <summary>
    /// KeyVault extensions for Microsoft.Identity.Client.ConfidentialClientApplicationBuilder
    /// </summary>
    public static class ConfidentialClientApplicationBuilderExtensions
    {
        /// <summary>
        /// Add a client assertion, while they key stays in the KeyVault
        /// </summary>
        /// <param name="applicationBuilder">ConfidentialClientApplicationBuilder</param>
        /// <param name="tenantId">Tenant ID for which you want to use this token</param>
        /// <param name="clientId">Client Identifier</param>
        /// <param name="vaultUri">Uri of the KeyVault</param>
        /// <param name="certificateName">Name of certificate</param>
        public static ConfidentialClientApplicationBuilder WithKeyVaultCertificate(this ConfidentialClientApplicationBuilder applicationBuilder, string tenantId, string clientId, Uri vaultUri, string certificateName)
        {
            return applicationBuilder.WithKeyVaultCertificate(tenantId, clientId, vaultUri, certificateName, new DefaultAzureCredential());
        }

        /// <summary>
        /// Add a client assertion, while they key stays in the KeyVault
        /// </summary>
        /// <param name="applicationBuilder">ConfidentialClientApplicationBuilder</param>
        /// <param name="tenantId">Tenant ID for which you want to use this token</param>
        /// <param name="clientId">Client Identifier</param>
        /// <param name="vaultUri">Uri of the KeyVault</param>
        /// <param name="certificateName">Name of certificate</param>
        /// <param name="tokenCredential">Use any TokenCredential (eg. new DefaultTokenCredential())</param>
        public static ConfidentialClientApplicationBuilder WithKeyVaultCertificate(this ConfidentialClientApplicationBuilder applicationBuilder, string tenantId, string clientId, Uri vaultUri, string certificateName, TokenCredential tokenCredential)
        {
            return applicationBuilder
                //.WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
                .WithClientAssertion((CancellationToken cancellationToken) =>
                    ClientAssertionGenerator.GetSignedTokenWithKeyVaultCertificate(tenantId, clientId, vaultUri, certificateName, tokenCredential, cancellationToken: cancellationToken)
                );
        }

        /// <summary>
        /// Add a client assertion, while they key stays in the KeyVault
        /// </summary>
        /// <param name="applicationBuilder">ConfidentialClientApplicationBuilder</param>
        /// <param name="tenantId">Tenant ID for which you want to use this token</param>
        /// <param name="clientId">Client Identifier</param>
        /// <param name="keyVaultKeyId">KeyId, Uri of the actual key in the KeyVault</param>
        /// <param name="kid">The Base64Url encoded hash of the certificate, use GetCertificateInfoFromKeyVault</param>
        public static ConfidentialClientApplicationBuilder WithKeyVaultKey(this ConfidentialClientApplicationBuilder applicationBuilder, string tenantId, string clientId, Uri keyVaultKeyId, string kid)
        {
            return applicationBuilder.WithKeyVaultKey(tenantId, clientId, keyVaultKeyId, kid, new DefaultAzureCredential());
        }


        /// <summary>
        /// Add a client assertion, while they key stays in the KeyVault
        /// </summary>
        /// <param name="applicationBuilder">ConfidentialClientApplicationBuilder</param>
        /// <param name="tenantId">Tenant ID for which you want to use this token</param>
        /// <param name="clientId">Client Identifier</param>
        /// <param name="keyVaultKeyId">KeyId, Uri of the actual key in the KeyVault</param>
        /// <param name="kid">The Base64Url encoded hash of the certificate, use GetCertificateInfoFromKeyVault</param>
        /// <param name="tokenCredential">Use any TokenCredential (eg. new DefaultTokenCredential())</param>
        public static ConfidentialClientApplicationBuilder WithKeyVaultKey(this ConfidentialClientApplicationBuilder applicationBuilder, string tenantId, string clientId, Uri keyVaultKeyId, string kid, TokenCredential tokenCredential)
        {
            return applicationBuilder
                //.WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
                .WithClientAssertion((CancellationToken cancellationToken) =>
                    ClientAssertionGenerator.GetSignedTokenWithKeyVaultKey(tenantId, clientId, keyVaultKeyId, kid, tokenCredential, cancellationToken: cancellationToken)
                );
        }
    }
}
