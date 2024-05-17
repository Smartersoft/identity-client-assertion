using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using System.Security.Cryptography.X509Certificates;

namespace Smartersoft.Identity.Client.Assertion.Proxy.Controllers
{
    /// <summary>
    /// TokenController hosting all the KeyVault proxy endpoints
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ILogger<TokenController> _logger;
        private readonly TokenCredential _tokenCredential;
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        /// TokenController constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="tokenCredential"></param>
        public TokenController(ILogger<TokenController> logger, TokenCredential tokenCredential, IMemoryCache memoryCache)
        {
            _logger = logger;
            _tokenCredential = tokenCredential;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Get access token with KeyVault key
        /// </summary>
        [HttpPost("kv-key")]
        [ProducesResponseType(typeof(Models.TokenResponse), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<ActionResult<Models.TokenResponse>> KvKeyAccessToken([FromBody] Models.KeyVaultKeyTokenRequest tokenRequest, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Token request with Key Vault key {clientId}, {keyUri}", tokenRequest.ClientId, tokenRequest.KeyUri?.ToString());


            var app = ConfidentialClientApplicationBuilder
                .Create(tokenRequest.ClientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, tokenRequest.TenantId)
                .WithKeyVaultKey(tokenRequest.KeyUri!, tokenRequest.KeyThumbprint!, _tokenCredential)
                .Build();

            var authResult = await app
                .AcquireTokenForClient(tokenRequest.Scopes)
                .ExecuteAsync(cancellationToken);

            return Ok(Models.TokenResponse.FromAuthenticationResult(authResult));
        }

        /// <summary>
        /// Get key info about a certificate in the KeyVault
        /// </summary>
        [HttpPost("kv-key-info")]
        [ProducesResponseType(typeof(CertificateInfo), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<ActionResult<CertificateInfo>> KvKeyInfo([FromBody] Models.KeyVaultCertificateInfoRequest tokenRequest, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Certificate info for Key Vault certificate {certificateName}", tokenRequest.CertificateName);
            var cacheKey = $"{tokenRequest.KeyVaultUri}/{tokenRequest.CertificateName!}";
            if (!tokenRequest.SkipCache && _memoryCache.TryGetValue(cacheKey, out CertificateInfo? cachedCertInfo))
            {
                return Ok(cachedCertInfo);
            }

            var certInfo = await ClientAssertionGenerator.GetCertificateInfoFromKeyVault(tokenRequest.KeyVaultUri!, tokenRequest.CertificateName!, _tokenCredential, cancellationToken);

            if (!tokenRequest.SkipCache)
            {
                _memoryCache.Set(cacheKey, certInfo, certInfo.ExpiresOn ?? DateTimeOffset.UtcNow.AddDays(1));
            }

            return Ok(certInfo);
        }

        /// <summary>
        /// Get access token with certificate hosted in KeyVault
        /// </summary>
        /// <remarks>Fetching the key info with /api/Token/kv-key-info and using it with /api/Token/kv-key is way more efficient.</remarks>
        [HttpPost("kv-certificate")]
        [ProducesResponseType(typeof(Models.TokenResponse), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<ActionResult<Models.TokenResponse>> KvCertAccessToken([FromBody] Models.KeyVaultCertificateTokenRequest tokenRequest, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Token request with Key Vault certificate {clientId}, {certificateName}", tokenRequest.ClientId, tokenRequest.CertificateName);

            var app = ConfidentialClientApplicationBuilder
                .Create(tokenRequest.ClientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, tokenRequest.TenantId)
                .WithKeyVaultCertificate(tokenRequest.KeyVaultUri!, tokenRequest.CertificateName!, _tokenCredential, tokenRequest.SkipCache ? null : _memoryCache)
                .Build();

            var authResult = await app
                .AcquireTokenForClient(tokenRequest.Scopes)
                .ExecuteAsync(cancellationToken);

            return Ok(Models.TokenResponse.FromAuthenticationResult(authResult));
        }

        /// <summary>
        /// Get access token with certificate from the local user certificate store
        /// </summary>
        [HttpPost("local-certificate")]
        [ProducesResponseType(typeof(Models.TokenResponse), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<ActionResult<Models.TokenResponse>> UserCertAccessToken([FromBody] Models.LocalTokenRequest tokenRequest, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Token request with local certificate @{tokenRequest}", tokenRequest);
            var store = new X509Store(StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            var certificates = store.Certificates.Find(tokenRequest.FindType ?? X509FindType.FindByThumbprint, tokenRequest.FindValue, true);
            if (certificates == null || certificates.Count == 0)
            {
                return NotFound();
            }

            var app = ConfidentialClientApplicationBuilder
                .Create(tokenRequest.ClientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, tokenRequest.TenantId)
                .WithCertificate(certificates.First())
                .Build();

            var authResult = await app.AcquireTokenForClient(tokenRequest.Scopes).ExecuteAsync(cancellationToken);

            return Ok(Models.TokenResponse.FromAuthenticationResult(authResult));
        }

        /// <summary>
        /// Get access token with certificate hosted in local computer certificate store
        /// </summary>
        [HttpPost("computer-certificate")]
        [ProducesResponseType(typeof(Models.TokenResponse), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<ActionResult<Models.TokenResponse>> ComputerCertAccessToken([FromBody] Models.LocalTokenRequest tokenRequest, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Token request with local certificate @{tokenRequest}", tokenRequest);
            var store = new X509Store(StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            var certificates = store.Certificates.Find(tokenRequest.FindType ?? X509FindType.FindByThumbprint, tokenRequest.FindValue, true);
            if (certificates == null || certificates.Count == 0)
            {
                return NotFound();
            }

            var app = ConfidentialClientApplicationBuilder
                .Create(tokenRequest.ClientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, tokenRequest.TenantId)
                .WithCertificate(certificates.First())
                .Build();

            var authResult = await app
                .AcquireTokenForClient(tokenRequest.Scopes)
                .ExecuteAsync(cancellationToken);

            return Ok(Models.TokenResponse.FromAuthenticationResult(authResult));
        }
    }
}
