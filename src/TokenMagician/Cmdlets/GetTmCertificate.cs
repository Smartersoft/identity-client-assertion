using Azure.Core;
using Microsoft.Extensions.Logging;
using Svrooij.PowerShell.DependencyInjection;
using Smartersoft.Identity.Client.Assertion;
using System.Management.Automation;

namespace TokenMagician.Cmdlets;

[Cmdlet(VerbsCommon.Get, "TmCertificate", DefaultParameterSetName = SetCertificate)]
[OutputType(typeof(Models.CertificateInfo))]
public class GetTmCertificate : DependencyCmdlet<Startup>
{
    private const string SetCertificate = "Certificate";

    const string KeyVaultUriHelp = "The URI of the key vault where the certificate is stored.";
    [Parameter(Position = 0, Mandatory = true, ParameterSetName = SetCertificate, HelpMessage = KeyVaultUriHelp)]
    public Uri? KeyVaultUri { get; set; }

    const string CertificateHelp = "The name of the certificate in the key vault.";
    [Parameter(Position = 1, Mandatory = true, ParameterSetName = SetCertificate, HelpMessage = CertificateHelp)]
    public string? Certificate { get; set; }

    [ServiceDependency]
    private ILogger<GetTmCertificate>? _logger;

    [ServiceDependency]
    private TokenCredential? _tokenCredential;

    public override async Task ProcessRecordAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("Get certificate info from {keyVaultUri} for {certificateName}", KeyVaultUri, Certificate);

        var certInfo = await ClientAssertionGenerator.GetCertificateInfoFromKeyVault(KeyVaultUri!, Certificate!, _tokenCredential!, cancellationToken);
        if (certInfo != null)
        {
            WriteObject(new Models.CertificateInfo(certInfo));
        }
    }
}
