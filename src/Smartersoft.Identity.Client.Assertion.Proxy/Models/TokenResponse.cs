using Microsoft.Identity.Client;

namespace Smartersoft.Identity.Client.Assertion.Proxy.Models
{
    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public int Lifetime { get; set; }
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
