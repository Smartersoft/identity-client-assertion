# Smartersoft.Identity.Client.Assertion

This package allows you to use [Managed Identities](https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/overview?wt.mc_id=SEC-MVP-5004985)
with a multi tenant application. Your certificates used for getting access tokens with the **Client Credential** flow will be completely protected and can **NEVER** be extracted, not even by yourself.

Managed Identities are great but they [don't support multi-tenant use cases](https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/managed-identities-faq?wt.mc_id=SEC-MVP-5004985#can-i-use-a-managed-identity-to-access-a-resource-in-a-different-directorytenant), until now.

This library is created by [Smartersoft B.V.](https://smartersoft.nl) and [licensed](https://github.com/Smartersoft/identity-client-assertion/blob/main/LICENSE.txt) as **GPL-3.0-only**.

More details on this library in [this post](https://svrooij.io/2022/01/20/secure-multi-tenant-app/#post)

## Prerequisites

- Azure resource with support for managed identities (Azure Functions, App Service, ...)
- KeyVault
- `Key Sign` (and optionally `Get Certificate`) permission on the KeyVault with the managed identity
- Multi-tenant app registration
- Self-signed certificate in KeyVault, see below

## Creating a certificate in KeyVault

When using a certificate for client assertions a self-signed certificate will suffice. It will only be used for digital signatures, so it doesn't matter if it's not from some known CA.

1. Go to the KeyVault in Azure Portal
2. Click certificates
3. Click Generate/Import
4. Enter any name (needed to get the certificate info later on)
5. Pick a subject, I always use `CN={app-name}.{company}.internal`
6. Set a Validity period (`12 months` is the default, which is fine)
7. Leave Content Type to `PKCS #12`
8. Set Lifetime action Type to `E-mail all contacts...` instead of auto-renew. You'll need to know when you'll have to take action.
9. **Configure** Advanced Policy Configuration! Set **X.509 Key Usage Flags** to `Digital Signature` only and **Exportable Private Key** to `No`. Leave the rest to their default setting.
10. Click Create
11. Click the new certificate, click the version, download in CER format (needed in app registration).

When creating a certificate in the KeyVault, it's **IMPORTANT** to configure the **Advanced Policy Configuration**.
This allows you to mark the private key as **NOT EXPORTABLE**, which means that private key will **NEVER** leave that KeyVault.

## Required usings

```csharp
using Azure.Identity;
using Microsoft.Identity.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using Smartersoft.Identity.Client.Assertion;
```

## Get access token using certificate in KeyVault

```csharp
    private readonly IMemoryCache? _injectedCache;
    public async Task<string> GetToken (CancellationToken cancellationToken)
    {
        // Create a token credential that suits your needs, used to access the KeyVault
        // You should get this from dependency injection as a singleton, because it will cache the token internally.
        var tokenCredential = new DefaultAzureCredential();

        const string clientId = "d294e746-425b-44fa-896c-dacf2c7938b8";
        const string tenantId = "42a26c5d-b8ed-4f1b-8760-655f98154373";
        const string KeyVaultUri = "https://{kv-domain}.vault.azure.net/";
        const string certificateName = "some-certificate";

        // Use the ConfidentialClientApplicationBuilder as usual
        // but call `.WithKeyVaultCertificate(...)` instead of `.WithCertificate(...)`
        var app = ConfidentialClientApplicationBuilder
            .Create(clientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
            .WithKeyVaultCertificate(new Uri(KeyVaultUri), certificateName, tokenCredential, _injectedCache)
            .Build();

        // Use the app, just like before
        var tokenResult = await app.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" })
            .ExecuteAsync(cancellationToken);

        return tokenResult.AccessToken;
    }
```

## Get access token using certificate in KeyVault, more efficiently

To use the [Client Assertion](https://docs.microsoft.com/azure/active-directory/develop/msal-net-client-assertions?wt.mc_id=SEC-MVP-5004985)
you'll need the Base64Url encoded certificate hash. This information about the certificate will almost never change, only after certificate renewal.

It can be loaded only once and saved in a config file to reduce the calls to the KeyVault, the code above calls the KeyVault twice for each call to get a client assertion.

```csharp
    public async Task<string> GetTokenEfficiently(CancellationToken cancellationToken)
    {
        // Create a token credential that suits your needs, used to access the KeyVault
        // You should get this from dependency injection as a singleton, because it will cache the token internally.
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
            .WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
            .WithKeyVaultKey(keyId, kid, tokenCredential)
            .Build();

        // Use the app, just like before
        var tokenResult = await app.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" })
            .ExecuteAsync(cancellationToken);

        return tokenResult.AccessToken;
    }
```

## Security

Why is this solution more secure that others?
This solution will **prevent** attackers getting **persistent access** in case of a breach.

All other samples I've seen use the [CertificateClient.DownloadCertificateAsync](https://learn.microsoft.com/en-us/dotnet/api/azure.security.keyvault.certificates.certificateclient.downloadcertificateasync?view=azure-dotnet&wt.mc_id=SEC-MVP-5004985) method to **Get the certificate information** and **Download the private key**.
If the app can Get the secret, an attacker can do the same.

This way the seemingly secure certificate can be extracted by some malicious actor, and if the breach goes undetected they now have a certificate that can possibly access data in several tenants. Without getting noticed.

This solution does the signing in the KeyVault instead of on the client. The application doesn't need the private key.
It just needs the **Sign** permission.

Off course this solution still needs a secure way to access the Key Vault, like a managed identity. But if you need to implement [KeyVault access without managed identities](https://svrooij.io/2021/07/20/managed-identity-without-azure/#post), the attacker can only sign token requests during the breach. This way you'll always have a log file of the sign-in attempts, in your Azure AD. If they would succeed in extracting the certificate, the only logs would be in the client Azure AD.

### How does this work?

1. Generate an unsigned client assertion (just some json, Base64Url encoded)
2. Converts the unsigned client assertion to bytes
3. Asks the KeyVault to [Sign the data](https://docs.microsoft.com/dotnet/api/azure.security.keyvault.keys.cryptography.cryptographyclient.signdata?view=azure-dotnet&wt.mc_id=SEC-MVP-5004985).
4. Encodes the signature Base64Url
5. Appends the signature to the token

## License

These packages are [licensed](https://github.com/Smartersoft/identity-client-assertion/blob/main/LICENSE.txt) under `GPL-3.0`, if you wish to use this software under a different license. Or you feel that this really helped in your commercial application and wish to support us? You can [get in touch](https://smartersoft.nl/#contact) and we can talk terms. We are available as consultants.

## Open-source

This package is open-source for a reason. It's developed by [Stephan van Rooij](https://svrooij.io/), people make mistakes. Always check out what's doing and make sure it doesn't do anything strange with the tokens.

