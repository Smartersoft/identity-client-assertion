using Azure.Core;
using Azure.Identity;
using Microsoft.Identity.Client;
using System;
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
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("Replaced by method without Tenant ID and Client ID")]
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
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("Replaced by method without Tenant ID and Client ID")]
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
        /// <param name="vaultUri">Uri of the KeyVault</param>
        /// <param name="certificateName">Name of certificate</param>
        public static ConfidentialClientApplicationBuilder WithKeyVaultCertificate(this ConfidentialClientApplicationBuilder applicationBuilder, Uri vaultUri, string certificateName)
        {
            return applicationBuilder.WithKeyVaultCertificate(vaultUri, certificateName, new DefaultAzureCredential());
        }

        /// <summary>
        /// Add a client assertion, while they key stays in the KeyVault
        /// </summary>
        /// <param name="applicationBuilder">ConfidentialClientApplicationBuilder</param>
        /// <param name="vaultUri">Uri of the KeyVault</param>
        /// <param name="certificateName">Name of certificate</param>
        /// <param name="tokenCredential">Use any TokenCredential (eg. new DefaultTokenCredential())</param>
        public static ConfidentialClientApplicationBuilder WithKeyVaultCertificate(this ConfidentialClientApplicationBuilder applicationBuilder, Uri vaultUri, string certificateName, TokenCredential tokenCredential)
        {
            return applicationBuilder
                //.WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
                .WithClientAssertion((AssertionRequestOptions options) =>
                    ClientAssertionGenerator.GetSignedTokenWithKeyVaultCertificate(vaultUri, certificateName, options.TokenEndpoint, options.ClientID, tokenCredential, cancellationToken: options.CancellationToken)
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
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("Replaced by method without Tenant ID and Client ID")]
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
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("Replaced by method without Tenant ID and Client ID")]
        public static ConfidentialClientApplicationBuilder WithKeyVaultKey(this ConfidentialClientApplicationBuilder applicationBuilder, string tenantId, string clientId, Uri keyVaultKeyId, string kid, TokenCredential tokenCredential)
        {
            return applicationBuilder
                //.WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
                .WithClientAssertion((CancellationToken cancellationToken) =>
                    ClientAssertionGenerator.GetSignedTokenWithKeyVaultKey(tenantId, clientId, keyVaultKeyId, kid, tokenCredential, cancellationToken: cancellationToken)
                );
        }

        /// <summary>
        /// Add a client assertion, while they key stays in the KeyVault
        /// </summary>
        /// <param name="applicationBuilder">ConfidentialClientApplicationBuilder</param>
        /// <param name="keyVaultKeyId">KeyId, Uri of the actual key in the KeyVault</param>
        /// <param name="kid">The Base64Url encoded hash of the certificate, use GetCertificateInfoFromKeyVault</param>
        public static ConfidentialClientApplicationBuilder WithKeyVaultKey(this ConfidentialClientApplicationBuilder applicationBuilder, Uri keyVaultKeyId, string kid)
        {
            return applicationBuilder.WithKeyVaultKey(keyVaultKeyId, kid, new DefaultAzureCredential());
        }

        /// <summary>
        /// Add a client assertion, while they key stays in the KeyVault
        /// </summary>
        /// <param name="applicationBuilder">ConfidentialClientApplicationBuilder</param>
        /// <param name="keyVaultKeyId">KeyId, Uri of the actual key in the KeyVault</param>
        /// <param name="kid">The Base64Url encoded hash of the certificate, use GetCertificateInfoFromKeyVault</param>
        /// <param name="tokenCredential">Use any TokenCredential (eg. new DefaultTokenCredential())</param>
        public static ConfidentialClientApplicationBuilder WithKeyVaultKey(this ConfidentialClientApplicationBuilder applicationBuilder, Uri keyVaultKeyId, string kid, TokenCredential tokenCredential)
        {
            return applicationBuilder
                //.WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
                .WithClientAssertion((AssertionRequestOptions options) =>
                    ClientAssertionGenerator.GetSignedTokenWithKeyVaultKey(keyVaultKeyId, kid, options.TokenEndpoint, options.ClientID, tokenCredential, cancellationToken: options.CancellationToken)
                );
        }

        /// <summary>
        /// Add a client assertion using a Managed Identity, configured as Federated Credential.
        /// </summary>
        /// <param name="applicationBuilder">ConfidentialClientApplicationBuilder</param>
        /// <param name="managedIdentityScope">The scope used for the federated credential api</param>
        /// <see href="https://svrooij.io/2022/06/21/managed-identity-multi-tenant-app/">Blog post</see>
        /// <remarks>This is experimental, since federated credentials are still in preview.</remarks>
        public static ConfidentialClientApplicationBuilder WithManagedIdentity(this ConfidentialClientApplicationBuilder applicationBuilder, string managedIdentityScope) => applicationBuilder.WithManagedIdentity(managedIdentityScope, new ManagedIdentityCredential());

        /// <summary>
        /// Add a client assertion using a Managed Identity, configured as Federated Credential.
        /// </summary>
        /// <param name="applicationBuilder">ConfidentialClientApplicationBuilder</param>
        /// <param name="managedIdentityScope">The scope used for the federated credential api, eg. `{app-uri}/.default`</param>
        /// <param name="managedIdentityCredential">Use any TokenCredential (eg. new ManagedIdentityCredential())</param>
        /// <see href="https://svrooij.io/2022/06/21/managed-identity-multi-tenant-app/">Blog post</see>
        /// <remarks>This is experimental, since federated credentials are still in preview.</remarks>
        public static ConfidentialClientApplicationBuilder WithManagedIdentity(this ConfidentialClientApplicationBuilder applicationBuilder, string managedIdentityScope, TokenCredential managedIdentityCredential)
        {
            return applicationBuilder.WithClientAssertion(async (AssertionRequestOptions options) =>
            {
                var tokenResult = await managedIdentityCredential.GetTokenAsync(new TokenRequestContext(new[] { managedIdentityScope }), options.CancellationToken);
                return tokenResult.Token;
            });
        }
    }
}