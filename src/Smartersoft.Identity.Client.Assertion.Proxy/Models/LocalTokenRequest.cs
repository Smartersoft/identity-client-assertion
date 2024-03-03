using FluentValidation;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
namespace Smartersoft.Identity.Client.Assertion.Proxy.Models
{
    /// <summary>
    /// Request an access token with a certificate in the local certificate store
    /// </summary>
    public class LocalTokenRequest : TokenRequest
    {
        /// <summary>
        /// How do you want to search for the certificate
        /// </summary>
        public X509FindType? FindType { get; set; } = X509FindType.FindByThumbprint;
        /// <summary>
        /// With what value do you wish to search for the certificate?
        /// </summary>
        [Required]
        public required object FindValue { get; set; }
    }

    /// <summary>
    /// LocalTokenRequestValidator
    /// </summary>
    public class LocalTokenRequestValidator : AbstractValidator<LocalTokenRequest>
    {
        /// <summary>
        /// LocalTokenRequestValidator constructor
        /// </summary>
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
