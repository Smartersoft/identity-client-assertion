# Smartersoft.Identity.Client.Assertion.Proxy

If you are only allowed to use certificates as client credentials, and you're storing those in an Azure Key Vault.
Your stuck when you want to use postman to debug your api.

This small api allows you to requests access tokens with those secrets securely stored in the Key Vault. See [this post](https://svrooij.io/2022/01/20/secure-multi-tenant-app/) for more details. Or check the [live demo][link_twitch].

[![Github source][badge_source]][link_source]
[![Nuget package][badge_nuget_proxy]][link_nuget_proxy]
[![GitHub License][badge_license]][link_license]
[![GitHub issues][badge_issues]][link_issues]
[![GitHub Sponsors][badge_sponsor]][link_sponsor]

## Development only!

**DON'T** use this proxy anywhere in production! Having an endpoint where every app can just request tokens without authentication with your developer credentials is a bad idea.
This api is meant to be used during development only! For production check out [our extensions to ConfidentialClientApplicationBuilder](link_nuget).

## Using this proxy

1. Install the proxy with `dotnet tool install --global Smartersoft.Identity.Client.Assertion.Proxy`
2. Run the proxy with `az-kv-proxy` or `az-kv-proxy --urls http://localhost:5616` if you wish to use another port
3. Open de browser and go to [/swagger/index.html](http://localhost:5616/swagger/index.html)
4. Try the endpoints

### Usage with Insomnia

1. Create request called `GetToken` to one of the three endpoints.
2. Edit the original request, change authentication to Bearer.
3. Select `TOKEN` field and press `CTRL` + `SPACE`, and select `Response: Body attribute`.
4. Request: Select `GetToken`, Filter: `$.access_token`, Trigger Behavior: `When Expired` and Max Age: `3000` (any number between 300 and 3599)

I like [Insomnia](https://insomnia.rest/) over postman, but your millage may vary.

### Usage with postman

1. Create an environment variable called `token`.
2. Create a request to one of the 3 endpoints to get a token.
3. In the **Tests** tab, save the `access_token` to the environment variable `token`
4. Change other requests to use environment variable `token` as the token.

See [this post](https://blog.postman.com/extracting-data-from-responses-and-chaining-requests/) for more details.

## Available endpoints

This api had several endpoints all requiring different parameters.

They all respond with the same data (provided it succeeded to get a token).

```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIx___0IjoxNTE2MjM5MDIyfQ.SflKxwR___6yJV_adQssw5c",
  "lifetime": 3600,
  "expires_on": "2022-01-27T11:26:21.0424181+00:00",
  "scopes": [
    "https://graph.microsoft.com/.default"
  ]
}
```

And there also is a Swagger UI running to try it out in the browser, `/swagger/index.html`.

### Using Key Vault Key

This is the most efficient way to use the Key Vault with your secret securely saved. It requires to get info about the key.

- **URL** `/api/Token/kv-key`
- **Method** `POST`

```json
{
  "clientId": "7e36ca13-5d1e-4c62-95f1-66570bfcec47",
  "tenantId": "8cd0791b-341e-40d5-a6de-9a0249c447f2",
  "scopes": [
    "https://graph.microsoft.com/.default"
  ],
  "keyUri": "https://{kv-domain}.vault.azure.net/keys/{some-certificate-name}/{cert-version}",
  "keyThumbprint": "{base64Url-encoded-certificate-hash}"
}
```

### Using Key Vault Certificate

This endpoint still keeps the certificate in the Key Store, but it does requests information about the certificate on each call.
Depending on usage, you're better of using the endpoint above this one.

- **URL** `/api/Token/kv-certificate`
- **Method** `POST`

```json
{
  "clientId": "7e36ca13-5d1e-4c62-95f1-66570bfcec47",
  "tenantId": "8cd0791b-341e-40d5-a6de-9a0249c447f2",
  "scopes": [
    "https://graph.microsoft.com/.default"
  ],
  "keyVaultUri": "https://{kv-domain}.vault.azure.net/",
  "certificateName": "{some-certificate-name}"
}
```

### Using Certificate from current user certificate store

This endpoint requires you to generate the certificate in the current user certificate store, but is at least safer than using a plain password as a secret.

- **URL** `/api/Token/local-certificate`
- **Method** `POST`

```json
{
  "clientId": "7e36ca13-5d1e-4c62-95f1-66570bfcec47",
  "tenantId": "8cd0791b-341e-40d5-a6de-9a0249c447f2",
  "scopes": [
    "https://graph.microsoft.com/.default"
  ],
  "findType": "FindByThumbprint",
  "findValue": "{value-to-find-certificate-Thumbprint-in-this-case}"
}
```

### Using Certificate from local computer certificate store

This endpoint requires you to generate the certificate in the current user certificate store, but is at least safer than using a plain password as a secret.

- **URL** `/api/Token/computer-certificate`
- **Method** `POST`

```json
{
  "clientId": "7e36ca13-5d1e-4c62-95f1-66570bfcec47",
  "tenantId": "8cd0791b-341e-40d5-a6de-9a0249c447f2",
  "scopes": [
    "https://graph.microsoft.com/.default"
  ],
  "findType": "FindByThumbprint",
  "findValue": "{value-to-find-certificate-Thumbprint-in-this-case}"
}
```

## Mananged Identity Credential Emulator

This api has a special endpoint to emulate the Managed Identity Credentials endpoint, (as used by the CloudShell). This is useful for local development and testing.
If you want to know how the ManagedIdentityCredential works, check out [this blog post](https://svrooij.io/2021/07/20/managed-identity-without-azure/#managedidentitycredential-explained).

For now you just have to know how to set it up:

1. Start the proxy
2. Pick your endpoint
3. Set the `MSI_ENDPOINT` environment variable to one of the MSI endpoints (see below).
   You can do this either in de debug settings of your IDE, in your user profile or in the launchSettings.json in the project.
4. Start your app and use `ManagedIdentityCredential` to get your tokens, as if you were running in the cloud.

Setting the `MSI_ENDPOINT` environment variable tricks the `ManagedIdentityCredential` into thinking it's running as it would in the CloudShell.
Specifically, it will trick the [ManagedIdentitySource](https://github.com/Azure/azure-sdk-for-net/blob/13bc415e43a92354af7019063718d54f10488c7e/sdk/identity/Azure.Identity/src/ManagedIdentityClient.cs#L80-L90),
to pick the [CloudShellManagedIdentitySource](https://github.com/Azure/azure-sdk-for-net/blob/13bc415e43a92354af7019063718d54f10488c7e/sdk/identity/Azure.Identity/src/CloudShellManagedIdentitySource.cs),
which happens to only need a `MSI_ENDPOINT` to work. 



### MSI - Forward

MSI Endpoint: `http://localhost:5616/api/msi/forward`

Your request is forwarded to the Microsoft Token Endpoint using DefaultAzureCredential, this might be useful in a situation where you want to test your app using MSI inside a docker container.
If you want to use this in docker, make sure the docker container can reach the host machine on port 5616 and set the `MSI_ENDPOINT` to `http://host.docker.internal:5616/api/msi/forward`.

### MSI - Local certificate

MSI Endpoint: `http://localhost:5616/api/msi/{tenant}/{clientId}/{machinecert_or_usercert}/{thumbprint}`

Do a token request with a pre-registered application and a certificate in the local machine certificate store or the current user certificate store.

### MSI - Key Vault certificate

MSI Endpoint: `http://localhost:5616/api/msi/{tenant}/{clientId}/kv/{keyvaultSubdomain}/{certificateName}`

Do a token request with a pre-registered application and a certificate in the Key Vault, this uses signing inside the keyvault, the private key of the certificate is not downloaded! You should generate it inside the KeyVault and mark it as not exportable. See (#using-key-vault-certificate) for more info.

## License

These packages are [licensed](https://github.com/Smartersoft/identity-client-assertion/blob/main/LICENSE.txt) under `GPL-3.0`, if you wish to use this software under a different license. Or you feel that this really helped in your commercial application and wish to support us? You can [get in touch](https://smartersoft.nl/#contact) and we can talk terms. We are available as consultants.

[badge_issues]: https://img.shields.io/github/issues/Smartersoft/identity-client-assertion?style=for-the-badge
[badge_license]: https://img.shields.io/github/license/Smartersoft/identity-client-assertion?style=for-the-badge
[badge_nuget_proxy]: https://img.shields.io/nuget/v/Smartersoft.Identity.Client.Assertion.Proxy?logoColor=00a880&style=for-the-badge
[badge_nuget]: https://img.shields.io/nuget/v/Smartersoft.Identity.Client.Assertion?logoColor=00a880&style=for-the-badge
[badge_source]: https://img.shields.io/badge/Source-Github-green?style=for-the-badge
[badge_sponsor]: https://img.shields.io/github/sponsors/svrooij?label=Github%20Sponsors&style=for-the-badge

[link_issues]: https://github.com/Smartersoft/identity-client-assertion/issues
[link_license]: https://github.com/Smartersoft/identity-client-assertion/blob/main/LICENSE.txt
[link_nuget_proxy]: https://www.nuget.org/packages/Smartersoft.Identity.Client.Assertion.Proxy/
[link_nuget]: https://www.nuget.org/packages/Smartersoft.Identity.Client.Assertion/
[link_source]: https://github.com/Smartersoft/identity-client-assertion/
[link_sponsor]: https://github.com/sponsors/svrooij/
[link_twitch]: https://www.twitch.tv/videos/1414084395