using System.Text.Json.Serialization;

namespace Smartersoft.Identity.Client.Assertion.Proxy.Models
{
    public record MsiResponse
    {
        [JsonPropertyName("access_token")]

        public string AccessToken { get; init; }
        [JsonPropertyName("refresh_token")]

        public string RefreshToken { get; init; } = string.Empty;
        [JsonPropertyName("expires_in")]

        public int ExpiresIn { get; init; }
        [JsonPropertyName("expires_on")]

        public int ExpiresOn { get; init; }
        [JsonPropertyName("not_before")]

        public int NotBefore { get; init; }
        [JsonPropertyName("resource")]
        public string Resource { get; init; }
        [JsonPropertyName("token_type")]
        public string TokenType { get; init; } = "Bearer";

        internal static MsiResponse FromAzureAccessToken(Azure.Core.AccessToken token, string resource) => new MsiResponse
        {
            AccessToken = token.Token,
            ExpiresIn = (int)token.ExpiresOn.Subtract(DateTimeOffset.UtcNow).TotalSeconds,
            ExpiresOn = (int)token.ExpiresOn.ToUnixTimeSeconds(),
            NotBefore = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Resource = resource,
        };

        internal static MsiResponse FromAuthenticationResult(Microsoft.Identity.Client.AuthenticationResult authenticationResult, string resource) => new MsiResponse
        {
            AccessToken = authenticationResult.AccessToken,
            ExpiresIn = (int)authenticationResult.ExpiresOn.Subtract(DateTimeOffset.UtcNow).TotalSeconds,
            ExpiresOn = (int)authenticationResult.ExpiresOn.ToUnixTimeSeconds(),
            NotBefore = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Resource = resource,
        };
    }
}
