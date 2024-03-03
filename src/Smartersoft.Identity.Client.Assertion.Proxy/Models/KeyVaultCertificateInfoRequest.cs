using FluentValidation;

namespace Smartersoft.Identity.Client.Assertion.Proxy.Models
{
    /// <summary>
    /// Request the info for a certificate in a KeyVault
    /// </summary>
    public class KeyVaultCertificateInfoRequest
    {
        /// <summary>
        /// The Uri of your KeyVault hosting the certificate
        /// </summary>
        public Uri? KeyVaultUri { get; set; }
        /// <summary>
        /// Name of the certificate you wish to use
        /// </summary>
        public string? CertificateName { get; set; }
    }

    /// <summary>
    /// KeyVaultCertificateInfoRequestValidator
    /// </summary>
    public class KeyVaultCertificateInfoRequestValidator : AbstractValidator<KeyVaultCertificateInfoRequest>
    {
        /// <summary>
        /// KeyVaultCertificateInfoRequestValidator constructor
        /// </summary>
        public KeyVaultCertificateInfoRequestValidator()
        {
            RuleFor(r => r.CertificateName)
                .NotEmpty()
                .NotEqual("string");
            RuleFor(r => r.KeyVaultUri)
                .NotEmpty()
                .Must(v => v?.IsAbsoluteUri == true)
                .WithMessage("Only absolute Uris permitted");
        }
    }
}
