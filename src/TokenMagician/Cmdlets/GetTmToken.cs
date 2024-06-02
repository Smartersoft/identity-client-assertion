using Azure.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Svrooij.PowerShell.DependencyInjection;
using Smartersoft.Identity.Client.Assertion;
using System.Management.Automation;

namespace TokenMagician.Cmdlets;

[Cmdlet(VerbsCommon.Get, "TmToken", DefaultParameterSetName = SetCertificate)]
[OutputType(typeof(Models.TokenResult))]
public class GetTmToken : DependencyCmdlet<Startup>
{
    private const string SetCertificate = "Certificate";
    private const string SetKeyId = "KeyId";
    private const string SetManagedIdentity = "ManagedIdentity";

    const string ClientIdHelp = "The client id of the application to authenticate as.";
    [Parameter(Position = 0, Mandatory = true, ParameterSetName = SetCertificate, HelpMessage = ClientIdHelp)]
    [Parameter(Position = 0, Mandatory = true, ParameterSetName = SetKeyId, HelpMessage = ClientIdHelp)]
    public string? ClientId { get; set; }

    const string TenantIdHelp = "The tenant id where you want to authenticate to.";
    [Parameter(Position = 1, Mandatory = true, ParameterSetName = SetCertificate, HelpMessage = TenantIdHelp)]
    [Parameter(Position = 1, Mandatory = true, ParameterSetName = SetKeyId, HelpMessage = TenantIdHelp)]
    public string? TenantId { get; set; }

    const string ScopeHelp = "The scope to request a token for. (eg. https://graph.microsoft.com/.default)";
    [Parameter(Position = 2, Mandatory = true, ParameterSetName = SetCertificate, HelpMessage = ScopeHelp)]
    [Parameter(Position = 2, Mandatory = true, ParameterSetName = SetKeyId, HelpMessage = ScopeHelp)]
    [Parameter(Position = 0, Mandatory = true, ParameterSetName = SetManagedIdentity, HelpMessage = ScopeHelp)]
    public string? Scope { get; set; }

    const string KeyVaultUriHelp = "The URI of the key vault where the certificate is stored.";
    [Parameter(Position = 3, Mandatory = true, ParameterSetName = SetCertificate, HelpMessage = KeyVaultUriHelp)]
    public Uri? KeyVaultUri { get; set; }

    const string CertificateHelp = "The name of the certificate in the key vault.";
    [Parameter(Position = 4, Mandatory = true, ParameterSetName = SetCertificate, HelpMessage = CertificateHelp)]
    public string? Certificate { get; set; }

    const string KeyIdHelp = "The Key Uri (use `Get-TmCertificate` to get)";
    [Parameter(Position = 3, Mandatory = true, ParameterSetName = SetKeyId, HelpMessage = KeyIdHelp)]
    public Uri? KeyId { get; set; }

    const string KeyHashHelp = "The Key Hash (use `Get-TmCertificate` to get)";
    [Parameter(Position = 4, Mandatory = true, ParameterSetName = SetKeyId, HelpMessage = KeyHashHelp)]
    public string? KeyHash { get; set; }

    [Parameter(Position = 1, Mandatory = true, ParameterSetName = SetManagedIdentity, HelpMessage = "Get token using managed identity (DefaultAzureCredential)")]
    public SwitchParameter ManagedIdentity { get; set; }

    [ServiceDependency]
    private ILogger<GetTmToken>? _logger;

    [ServiceDependency]
    private TokenCredential? _tokenCredential;

    public override async Task ProcessRecordAsync(CancellationToken cancellationToken)
    {
        IConfidentialClientApplication? app = null;
        _logger?.LogInformation("Get token with {ClientId} and scope {Scope}, using {parameterSetName}", ClientId, Scope, ParameterSetName);
        if (ParameterSetName == SetCertificate)
        {
            app = ConfidentialClientApplicationBuilder.Create(ClientId)
                .WithTenantId(TenantId)
                .WithKeyVaultCertificate(KeyVaultUri!, Certificate!, _tokenCredential!)
                .Build();
        }
        else if (ParameterSetName == SetKeyId)
        {
            app = ConfidentialClientApplicationBuilder.Create(ClientId)
                .WithTenantId(TenantId)
                .WithKeyVaultKey(KeyId!, KeyHash!, _tokenCredential!)
                .Build();
        }
        else if (ParameterSetName == SetManagedIdentity)
        {
            var azTokenResult = await _tokenCredential!.GetTokenAsync(new TokenRequestContext(new[] { Scope! }), cancellationToken);
            WriteObject(new Models.TokenResult { AccessToken = azTokenResult.Token, ExpiresOn = azTokenResult.ExpiresOn, ClientId = ClientId, TenantId = TenantId, Scope = Scope });
            return;
        }


        var tokenResult = await app!.AcquireTokenForClient(new[] { Scope! })
            .ExecuteAsync(cancellationToken);

        _logger?.LogInformation("Token acquired for {ClientId} and scope {Scope}", ClientId, Scope);
        WriteObject(new Models.TokenResult(tokenResult, ClientId, Scope, TenantId));
    }
}
