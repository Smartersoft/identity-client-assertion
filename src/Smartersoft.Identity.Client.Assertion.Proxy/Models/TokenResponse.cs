using Microsoft.Identity.Client;
using System.Text.Json.Serialization;

namespace Smartersoft.Identity.Client.Assertion.Proxy.Models
{
    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        public int Lifetime { get; set; }
        [JsonPropertyName("expires_on")]

        public DateTimeOffset ExpiresOn { get; set; }
        public IEnumerable<string> Scopes { get; set; }

        public static TokenResponse FromAuthenticationResult(AuthenticationResult authenticationResult)
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
