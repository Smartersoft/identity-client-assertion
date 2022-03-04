using Microsoft.Identity.Client;
using System.Text.Json.Serialization;

namespace Smartersoft.Identity.Client.Assertion.Proxy.Models
{
    /// <summary>
    /// Response containing the access token
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// Access token (this will always be a JWT)
        /// </summary>
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
        /// <summary>
        /// Lifetime is not returned from Azure, use expires_on
        /// </summary>
        public int Lifetime { get; set; }

        /// <summary>
        /// When will the token expire, according to Azure
        /// </summary>
        [JsonPropertyName("expires_on")]
        public DateTimeOffset ExpiresOn { get; set; }

        /// <summary>
        /// Which scopes where requested in the token
        /// </summary>
        public IEnumerable<string>? Scopes { get; set; }

        internal static TokenResponse FromAuthenticationResult(AuthenticationResult authenticationResult)
        {
            return new TokenResponse
            {
                AccessToken = authenticationResult.AccessToken,
                Lifetime = 3600,
                ExpiresOn = authenticationResult.ExpiresOn,
                Scopes = authenticationResult.Scopes,
            };
        }
    }
}
