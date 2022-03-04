using FluentValidation;

namespace Smartersoft.Identity.Client.Assertion.Proxy.Models
{
    /// <summary>
    /// Request an access token with a key in a KeyVault
    /// </summary>
    public class KeyVaultKeyTokenRequest : TokenRequest
    {
        /// <summary>
        /// The Uri of the Key to use, found in KeyId in the KeyVault
        /// </summary>
        public Uri? KeyUri { get; set; }

        /// <summary>
        /// Base64encoded hash of the certificate, use the proxy /api/token/kv-key-info endpoint to find
        /// </summary>
        public string? KeyThumbprint { get; set; }
    }

    /// <summary>
    /// KeyVaultKeyTokenRequestValidator
    /// </summary>
    public class KeyVaultKeyTokenRequestValidator : AbstractValidator<KeyVaultKeyTokenRequest>
    {
        /// <summary>
        /// KeyVaultKeyTokenRequestValidator constructor
        /// </summary>
        public KeyVaultKeyTokenRequestValidator()
        {
            Include(new TokenRequestValidator());
            RuleFor(r => r.KeyUri)
                .NotEmpty()
                .Must(v => v?.IsAbsoluteUri == true)
                .WithMessage("Only absolute Uris permitted");
            RuleFor(r => r.KeyThumbprint)
                .NotEmpty()
                .NotEqual("string");
        }
    }
}
