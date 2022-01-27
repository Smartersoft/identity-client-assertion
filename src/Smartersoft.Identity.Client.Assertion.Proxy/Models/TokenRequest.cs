using FluentValidation;

namespace Smartersoft.Identity.Client.Assertion.Proxy.Models
{
    public class TokenRequest
    {
        public string ClientId { get; set; }
        public string TenantId { get; set; }
        public IEnumerable<string> Scopes { get; set; }
    }

    public class TokenRequestValidator : AbstractValidator<TokenRequest>
    {
        public TokenRequestValidator()
        {
            RuleFor(r => r.ClientId)
                .NotEmpty()
                .Must(v => Guid.TryParse(v, out _))
                .WithMessage("'{PropertyName}' must be a guid.");
            RuleFor(r => r.TenantId)
                .NotEmpty()
                .Must(v => Guid.TryParse(v, out _))
                .WithMessage("'{PropertyName}' must be a guid.");
            RuleFor(r => r.Scopes)
                .NotEmpty()
                .Must(v => !v.Any(s => s == "string"))
                .WithMessage("'string' is not a valid scope.");
        }
    }
}
