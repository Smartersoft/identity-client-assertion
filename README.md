# Smartersoft.Identity.Client.Assertion

This package allows you to use [Managed Identities](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview)
with a multi tenant application. Your certificates used for getting access tokens with the **Client Credential** flow will be completely protected and can **NEVER** be extracted, not even by yourself.

Managed Identities are great but they [don't support multi-tenant use cases](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/managed-identities-faq#can-i-use-a-managed-identity-to-access-a-resource-in-a-different-directorytenant), until now.

This library is created by [Smartersoft B.V.](https://smartersoft.nl) and [licensed](./LICENSE.txt) as **GPL-3.0-only**.

## Smartersoft.Identity.Client.Assertion

[![Nuget package][badge_nuget]][link_nuget]
[![GitHub issues][badge_issues]][link_issues]
[![GitHub Sponsors][badge_sponsor]][link_sponsor]

[Smartersoft.Identity.Client.Assertion](docs/Smartersoft.Identity.Client.Assertion.md) has some useful extensions for the [ConfidentialClientApplicationBuilder](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-net-initializing-client-applications#initializing-a-confidential-client-application-from-code)

## Smartersoft.Identity.Client.Assertion.Proxy

[![Nuget package][badge_nuget_proxy]][link_nuget_proxy]
[![GitHub issues][badge_issues]][link_issues]
[![GitHub Sponsors][badge_sponsor]][link_sponsor]

[Smartersoft.Identity.Client.Assertion.Proxy](docs/Smartersoft.Identity.Client.Assertion.Proxy.md) is a small api you can run on your local machine to use certificates stored in the KeyVault to secure your client credentials during development.

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