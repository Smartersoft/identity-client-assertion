using FluentValidation;

namespace Smartersoft.Identity.Client.Assertion.Proxy.Models
{
    /// <summary>
    /// Request an access token with a certificate in a KeyVault
    /// </summary>
    public class KeyVaultCertificateTokenRequest : TokenRequest
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
    /// KeyVaultCertificateTokenRequestValidator
    /// </summary>
    public class KeyVaultCertificateTokenRequestValidator : AbstractValidator<KeyVaultCertificateTokenRequest>
    {
        /// <summary>
        /// KeyVaultCertificateTokenRequestValidator constuctor
        /// </summary>
        public KeyVaultCertificateTokenRequestValidator()
        {
            Include(new TokenRequestValidator());
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
