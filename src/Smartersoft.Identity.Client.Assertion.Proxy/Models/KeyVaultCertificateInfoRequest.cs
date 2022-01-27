using FluentValidation;

namespace Smartersoft.Identity.Client.Assertion.Proxy.Models
{
    public class KeyVaultCertificateInfoRequest
    {
        public Uri KeyVaultUri { get; set; }
        public string CertificateName { get; set; }
    }

    public class KeyVaultCertificateInfoRequestValidator : AbstractValidator<KeyVaultCertificateInfoRequest>
    {
        public KeyVaultCertificateInfoRequestValidator()
        {
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
