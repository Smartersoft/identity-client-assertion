using FluentValidation;

namespace Smartersoft.Identity.Client.Assertion.Proxy.Models
{
    /// <summary>
    /// Base class for all token requests
    /// </summary>
    public class TokenRequest
    {
        /// <summary>
        /// Azure Client ID for you application (also called application id)
        /// </summary>
        public string? ClientId { get; set; }
        /// <summary>
        /// The ID of the Azure Tenant, needed in all requests with client credentials
        /// </summary>
        public string? TenantId { get; set; }
        /// <summary>
        /// For what scopes are you requesting a token
        /// </summary>
        /// <remarks>Even though this is an array, you should only specify just one!</remarks>
        public IEnumerable<string>? Scopes { get; set; }
    }

    /// <summary>
    /// TokenRequestValidator
    /// </summary>
    public class TokenRequestValidator : AbstractValidator<TokenRequest>
    {
        /// <summary>
        /// TokenRequestValidator constructor
        /// </summary>
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
                .Must(v => !v!.Any(s => s == "string"))
                .WithMessage("'string' is not a valid scope.");
        }
    }
}
