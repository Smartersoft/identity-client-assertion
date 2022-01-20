# Smartersoft.Identity.Client.Assertion

This package allows you to use [Managed Identities](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview)
with a multi tenant application. Your certificates used for getting access tokens with the **Client Credential** flow will be completely protected and can **NEVER** be extracted, not even by yourself.

Managed Identities are great but they [don't support multi-tenant use cases](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/managed-identities-faq#can-i-use-a-managed-identity-to-access-a-resource-in-a-different-directorytenant), until now.

This library is created by [Smartersoft B.V.](https://smartersoft.nl) and [licensed](./LICENSE) as **GPL-3.0-only**.

## Packages

- [Smartersoft.Identity.Client.Assertion](docs/Smartersoft.Identity.Client.Assertion.md)
