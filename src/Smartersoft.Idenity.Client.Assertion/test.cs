using Azure.Identity;
using Microsoft.Identity.Client;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace Smartersoft.Idenity.Client.Assertion
{
    internal class test
    {
        public async Task<string> GetToken (CancellationToken cancellationToken)
        {
            // Create a token credential that suits your needs, used to access the KeyVault
            var tokenCredential = new DefaultAzureCredential();

            const string clientId = "d294e746-425b-44fa-896c-dacf2c7938b8";
            const string tenantId = "42a26c5d-b8ed-4f1b-8760-655f98154373";
            const string KeyVaultUri = "https://{kv-domain}.vault.azure.net/";
            const string certificateName = "some-certificate";

            // Use the ConfidentialClientApplicationBuilder as usual
            // but call `.WithKeyVaultCertificate(...)` instead of `.WithCertificate(...)`
            var app = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithKeyVaultCertificate(tenantId, clientId, new Uri(KeyVaultUri), certificateName, tokenCredential)
                .Build();

            // Use the app, just like before
            var tokenResult = await app.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" })
                .ExecuteAsync(cancellationToken);

            return tokenResult.AccessToken;
        }

    public async Task<string> GetTokenEfficiently(CancellationToken cancellationToken)
    {
        // Create a token credential that suits your needs, used to access the KeyVault
        var tokenCredential = new DefaultAzureCredential();

        const string KeyVaultUri = "https://{kv-domain}.vault.azure.net/";
        const string certificateName = "some-certificate";

        Uri? keyId = null;
        string? kid = null;

        // Load once and save in Cache/Config/...
        var certificateInfo = await ClientAssertionGenerator.GetCertificateInfoFromKeyVault(new Uri(KeyVaultUri), certificateName, tokenCredential, cancellationToken);
        if (certificateInfo.Kid == null || certificateInfo.KeyId == null)
        {
            throw new Exception();
        }
        keyId = certificateInfo.KeyId;
        kid = certificateInfo.Kid;


        const string clientId = "d294e746-425b-44fa-896c-dacf2c7938b8";
        const string tenantId = "42a26c5d-b8ed-4f1b-8760-655f98154373";

        // Use the ConfidentialClientApplicationBuilder as usual
        // but call `.WithKeyVaultKey(...)` instead of `.WithCertificate(...)`
        var app = ConfidentialClientApplicationBuilder
            .Create(clientId)
            .WithKeyVaultKey(tenantId, clientId, keyId, kid, tokenCredential)
            .Build();

        // Use the app, just like before
        var tokenResult = await app.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" })
            .ExecuteAsync(cancellationToken);

        return tokenResult.AccessToken;
    }
    }
}
