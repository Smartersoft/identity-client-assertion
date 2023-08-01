using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using System.Security.Cryptography.X509Certificates;

namespace Smartersoft.Identity.Client.Assertion.Proxy.Controllers
{
    /// <summary>
    /// MSI controller to respond to ManagedIdentityCredential calls without a real MSI
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MsiController : ControllerBase
    {
        private readonly ILogger<MsiController> _logger;
        private readonly IMemoryCache? _cache;

        public MsiController(ILogger<MsiController> logger, IMemoryCache? cache)
        {
            _logger = logger;
            _cache = cache;
        }

        /// <summary>
        /// Forward the call to the Microsoft Token Service using DefaultAzureCredential
        /// </summary>
        /// <param name="msiRequest">MSI request as if this would come from CloudShell</param>
        [HttpPost("forward")]
        [ProducesResponseType(typeof(Models.MsiResponse), 200)]
        public async Task<IActionResult> Forward([FromForm] Models.MsiRequest msiRequest, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("MSI request for {resource}", msiRequest.Resource);
            var credential = new DefaultAzureCredential();
            var tokenResult = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { msiRequest.Resource }), cancellationToken);

            return Ok(Models.MsiResponse.FromAzureAccessToken(tokenResult, msiRequest.Resource));
        }

        /// <summary>
        /// Forward the call to the Microsoft Token Service using DefaultAzureCredential and a specific tenant
        /// </summary>
        /// <param name="tenant">Pre-set the tenant so other tenants are skipped when setting the token</param>
        /// <param name="msiRequest">MSI request as if this would come from CloudShell</param>
        [HttpPost("{tenant}/forward")]
        [ProducesResponseType(typeof(Models.MsiResponse), 200)]
        public async Task<IActionResult> TenantForward([FromRoute] string tenant, [FromForm] Models.MsiRequest msiRequest, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("MSI request for {resource}", msiRequest.Resource);

            var credential = new DefaultAzureCredential();
            var tokenResult = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { msiRequest.Resource }, tenantId: tenant), cancellationToken);

            return Ok(Models.MsiResponse.FromAzureAccessToken(tokenResult, msiRequest.Resource));
        }

        /// <summary>
        /// Forward ManagedIdentityCredential to the Microsoft Token Service using a pre-registered client and local user certificate
        /// </summary>
        /// <param name="tenant">Tenant</param>
        /// <param name="clientId">Client ID (Application ID)</param>
        /// <param name="thumbprint">Certificate Thumbprint</param>
        /// <param name="msiRequest">MSI request as if this would come from CloudShell</param>
        [HttpPost("{tenant}/{clientId}/usercert/{thumbprint}")]
        [ProducesResponseType(typeof(Models.MsiResponse), 200)]
        [ProducesResponseType(404)]
        public Task<IActionResult> UserCert(
            [FromRoute] string tenant,
            [FromRoute] string clientId,
            [FromRoute] string thumbprint,
            [FromForm] Models.MsiRequest msiRequest,
            CancellationToken cancellationToken = default) => Cert(StoreLocation.CurrentUser, tenant, clientId, thumbprint, msiRequest, cancellationToken);

        /// <summary>
        /// Forward ManagedIdentityCredential to the Microsoft Token Service using a pre-registered client and local machine certificate
        /// </summary>
        /// <param name="tenant">Tenant</param>
        /// <param name="clientId">Client ID (Application ID)</param>
        /// <param name="thumbprint">Certificate Thumbprint</param>
        /// <param name="msiRequest">MSI request as if this would come from CloudShell</param>
        [HttpPost("{tenant}/{clientId}/machinecert/{thumbprint}")]
        [ProducesResponseType(typeof(Models.MsiResponse), 200)]
        [ProducesResponseType(404)]
        public Task<IActionResult> MachineCert(
            [FromRoute] string tenant,
            [FromRoute] string clientId,
            [FromRoute] string thumbprint,
            [FromForm] Models.MsiRequest msiRequest,
            CancellationToken cancellationToken = default) => Cert(StoreLocation.LocalMachine, tenant, clientId, thumbprint, msiRequest, cancellationToken);

        private async Task<IActionResult> Cert(StoreLocation storeLocation, string tenant, string clientId, string thumbprint, Models.MsiRequest msiRequest, CancellationToken cancellationToken)
        {
            _logger.LogInformation("MSI request for {resource}, clientId: {clientId} cert: {thumbprint} store: {store} ", msiRequest.Resource, clientId, thumbprint, storeLocation);
            var store = new X509Store(storeLocation);
            store.Open(OpenFlags.ReadOnly);
            var cert = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, true).FirstOrDefault();
            if (cert == null)
            {
                _logger.LogError("Cert not found");
                return NotFound();
            }

            var app = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, tenant)
                .WithCertificate(cert)
                .Build();
            
            var authResult = await app.AcquireTokenForClient(new[] { msiRequest.Resource }).ExecuteAsync(cancellationToken);

            return Ok(Models.MsiResponse.FromAuthenticationResult(authResult, msiRequest.Resource));
        }

        /// <summary>
        /// Forward ManagedIdentityCredential to the Microsoft Token Service using a pre-registered client and a certificate stored in Azure Key Vault
        /// </summary>
        /// <param name="tenant">Tenant</param>
        /// <param name="clientId">Client ID (Application ID)</param>
        /// <param name="subdomain">KeyVault subdomain</param>
        /// <param name="certificateName">Certificate name of certificate in KeyVault</param>
        /// <param name="msiRequest">MSI request as if this would come from CloudShell</param>
        [HttpPost("{tenant}/{clientId}/kv/{subdomain}/{certificateName}")]
        [ProducesResponseType(typeof(Models.MsiResponse), 200)]
        public async Task<IActionResult> KeyVault(
                       [FromRoute] string tenant,
                       [FromRoute] string clientId,
                       [FromRoute] string subdomain,
                       [FromRoute] string certificateName,
                       [FromForm] Models.MsiRequest msiRequest,
                       CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("MSI request for {resource}, clientId: {clientId} kv: {subdomain} cert: {certificateName}", msiRequest.Resource, clientId, subdomain, certificateName);

            var certInfo = await GetCertificateInfoAsync(subdomain, certificateName, cancellationToken);
            if (certInfo == null)
            {
                _logger.LogError("Cert not found");
                return NotFound();
            }
            var app = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, tenant)
                .WithKeyVaultKey(certInfo.KeyId!, certInfo.Kid!, TokenController.GetTokenCredential())
                .Build();

            var authResult = await app.AcquireTokenForClient(new[] { msiRequest.Resource }).ExecuteAsync(cancellationToken);

            return Ok(Models.MsiResponse.FromAuthenticationResult(authResult, msiRequest.Resource));
        }

        private Task<CertificateInfo> GetCertificateInfoAsync(string subdomain, string certificateName, CancellationToken cancellationToken)
        {
            if(_cache is not null)
            {
                return _cache.GetOrCreateAsync($"cert-info-{subdomain}-{certificateName}", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                    return await ClientAssertionGenerator.GetCertificateInfoFromKeyVault(GenerateKeyVaultUri(subdomain), certificateName, TokenController.GetTokenCredential(), cancellationToken);
                })!;
            }

            return ClientAssertionGenerator.GetCertificateInfoFromKeyVault(GenerateKeyVaultUri(subdomain), certificateName, TokenController.GetTokenCredential(), cancellationToken);
        }

        private Uri GenerateKeyVaultUri(string subdomain)
        {
            return new Uri($"https://{subdomain}.vault.azure.net/");
        }
    }
}