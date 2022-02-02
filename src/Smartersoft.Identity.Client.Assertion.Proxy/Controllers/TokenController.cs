using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Security.Cryptography.X509Certificates;

namespace Smartersoft.Identity.Client.Assertion.Proxy.Controllers
{
    [Route("api/[controller]")]
    
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ILogger<TokenController> _logger;

        public TokenController(ILogger<TokenController> logger)
        {
            _logger = logger;
        }

        [HttpPost("kv-key")]
        [ProducesResponseType(typeof(Models.TokenResponse), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<ActionResult<Models.TokenResponse>> KvKeyAccessToken([FromBody] Models.KeyVaultKeyTokenRequest tokenRequest, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Token request with Key Vault key {clientId}, {keyUri}", tokenRequest.ClientId, tokenRequest.KeyUri.ToString());


            var app = ConfidentialClientApplicationBuilder
                .Create(tokenRequest.ClientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, tokenRequest.TenantId)
                .WithKeyVaultKey(tokenRequest.TenantId, tokenRequest.ClientId, tokenRequest.KeyUri, tokenRequest.KeyThumbprint, new DefaultAzureCredential(true))
                .Build();

            var authResult = await app
                .AcquireTokenForClient(tokenRequest.Scopes)
                .ExecuteAsync(cancellationToken);

            return Ok(Models.TokenResponse.FromAuthenticationResult(authResult));
        }

        [HttpPost("kv-key-info")]
        [ProducesResponseType(typeof(CertificateInfo), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<ActionResult<CertificateInfo>> KvKeyInfo([FromBody] Models.KeyVaultCertificateInfoRequest tokenRequest, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Certificate info for Key Vault certificate {certificateName}", tokenRequest.CertificateName);

            var certInfo = await ClientAssertionGenerator.GetCertificateInfoFromKeyVault(tokenRequest.KeyVaultUri, tokenRequest.CertificateName, new DefaultAzureCredential(true), cancellationToken);

            return Ok(certInfo);
        }

        [HttpPost("kv-certificate")]
        [ProducesResponseType(typeof(Models.TokenResponse), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<ActionResult<Models.TokenResponse>> KvCertAccessToken([FromBody] Models.KeyVaultCertificateTokenRequest tokenRequest, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Token request with Key Vault certificate {clientId}, {certificateName}", tokenRequest.ClientId, tokenRequest.CertificateName);

            var app = ConfidentialClientApplicationBuilder
                .Create(tokenRequest.ClientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, tokenRequest.TenantId)
                .WithKeyVaultCertificate(tokenRequest.TenantId, tokenRequest.ClientId, tokenRequest.KeyVaultUri, tokenRequest.CertificateName, new DefaultAzureCredential(true))
                .Build();

            var authResult = await app
                .AcquireTokenForClient(tokenRequest.Scopes)
                .ExecuteAsync(cancellationToken);

            return Ok(Models.TokenResponse.FromAuthenticationResult(authResult));
        }


        [HttpPost("local-certificate")]
        [ProducesResponseType(typeof(Models.TokenResponse), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<ActionResult<Models.TokenResponse>> UserCertAccessToken([FromBody] Models.LocalTokenRequest tokenRequest, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Token request with local certificate @{tokenRequest}", tokenRequest);
            var store = new X509Store(StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            var certificates = store.Certificates.Find(tokenRequest.FindType ?? X509FindType.FindByThumbprint, tokenRequest.FindValue, true);
            if(certificates == null || certificates.Count == 0)
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
