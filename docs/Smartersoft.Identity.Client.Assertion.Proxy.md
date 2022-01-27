# Smartersoft.Identity.Client.Assertion.Proxy

If you are only allowed to use certificates as client credentials, and you're storing those in an Azure Key Vault.
Your stuck when you want to use postman to debug your api.

This small api allows you to requests access tokens with those secrets securely stored in the Key Vault. See [this post](https://svrooij.io/2022/01/20/secure-multi-tenant-app/) for more details.

## Development only!

**DON'T** use this proxy anywhere in production! Having an endpoint where every app can just request tokens without authentication with your deverloper credentials is a bad idea.
This api is meant to be used during development only!

## Using this proxy

1. Install the proxy with `dotnet tool install --global Smartersoft.Identity.Client.Assertion.Proxy`
2. Run the proxy with `az-kv-proxy` or `az-kv-proxy --urls http://localhost:55000` if you wish to use another port
3. Open de browser and go to `http://localhost:5000/swagger/index.html`
4. Try the endpoints

### Usage with Insomnia

1. Create request called `GetToken` to one of the three endpoints.
2. Edit the original request, change authentication to Bearer.
3. Select `TOKEN` field and press `CTRL` + `SPACE`, and select `Response Body attribute`.
4. Request: Select `GetToken`, Filter: `$.accessToken`, Trigger Behavior: `When Expired` and Max Age: `3000` (any number between 300 and 3599)

I like [Insomnia](https://insomnia.rest/) over postman, but your millage may vary.

### Usage with postman

1. Create an enviroment variable called `token`.
2. Create a request to one of the 3 endpoints to get a token.
3. In the **Tests** tab, save the `accessToken` to the environment variable `token`
4. Change other requests to use enviroment variable `token` as the token.

See [this post](https://blog.postman.com/extracting-data-from-responses-and-chaining-requests/) for more details.

## Available endpoints

This api had several endpoints all requiring different parameters.

They all respond with the same data (provided it succeded to get a token).

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIx___0IjoxNTE2MjM5MDIyfQ.SflKxwR___6yJV_adQssw5c",
  "lifetime": 3600,
  "expiresOn": "2022-01-27T11:26:21.0424181+00:00",
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

### Using Certificate from current user store

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
