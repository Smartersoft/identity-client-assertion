using FluentValidation;

namespace Smartersoft.Identity.Client.Assertion.Proxy.Models
{
    public class KeyVaultKeyTokenRequest : TokenRequest
    {
        public Uri KeyUri { get; set; }
        public string KeyThumbprint { get; set; }
    }

    public class KeyVaultKeyTokenRequestValidator : AbstractValidator<KeyVaultKeyTokenRequest>
    {
        public KeyVaultKeyTokenRequestValidator()
        {
            Include(new TokenRequestValidator());
            RuleFor(r => r.KeyUri)
                .NotEmpty()
                .Must(v => v.IsAbsoluteUri)
                .WithMessage("Only absolute Uris permitted");
            RuleFor(r => r.KeyThumbprint)
                .NotEmpty()
                .NotEqual("string");
        }
    }
}
