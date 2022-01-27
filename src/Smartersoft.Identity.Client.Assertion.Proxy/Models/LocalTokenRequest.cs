using FluentValidation;
using System.Security.Cryptography.X509Certificates;
namespace Smartersoft.Identity.Client.Assertion.Proxy.Models
{
    public class LocalTokenRequest : TokenRequest
    {
        public X509FindType? FindType { get; set; } = X509FindType.FindByThumbprint;
        public object FindValue { get; set; }
    }

    public class LocalTokenRequestValidator : AbstractValidator<LocalTokenRequest>
    {
        public LocalTokenRequestValidator()
        {
            Include(new TokenRequestValidator());
            RuleFor(r => r.FindType)
                .NotEmpty()
                .IsInEnum()
                .WithMessage("Just use the default `FindByThumbprint`");
            RuleFor(r => r.FindValue)
                .NotEmpty()
                .NotEqual("string");
        }
    }
}
