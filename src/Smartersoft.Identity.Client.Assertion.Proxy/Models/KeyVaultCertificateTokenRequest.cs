using FluentValidation;

namespace Smartersoft.Identity.Client.Assertion.Proxy.Models
{
    public class KeyVaultCertificateTokenRequest : TokenRequest
    {
        public Uri KeyVaultUri { get; set; }
        public string CertificateName { get; set; }
    }

    public class KeyVaultCertificateTokenRequestValidator : AbstractValidator<KeyVaultCertificateTokenRequest>
    {
        public KeyVaultCertificateTokenRequestValidator()
        {
            Include(new TokenRequestValidator());
            RuleFor(r => r.CertificateName)
                .NotEmpty()
                .NotEqual("string");
            RuleFor(r => r.KeyVaultUri)
                .NotEmpty()
                .Must(v => v.IsAbsoluteUri)
                .WithMessage("Only absolute Uris permitted");
        }
    }
}
