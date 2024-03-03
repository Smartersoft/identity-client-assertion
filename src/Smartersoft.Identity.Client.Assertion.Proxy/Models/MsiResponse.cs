using System.Text.Json.Serialization;

namespace Smartersoft.Identity.Client.Assertion.Proxy.Models
{
    /// <summary>
    /// MsiResponse that apparantly all managed identity endpoints return
    /// </summary>
    public record MsiResponse
    {
        /// <summary>
        /// Access token for the resource
        /// </summary>
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; init; }

        /// <summary>
        /// Refresh token
        /// </summary>
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; init; } = string.Empty;

        /// <summary>
        /// Time in seconds until the token expires
        /// </summary>
        [JsonPropertyName("expires_in")]
        public required int ExpiresIn { get; init; }

        /// <summary>
        /// Unix timestamp when the token expires
        /// </summary>
        [JsonPropertyName("expires_on")]

        public required int ExpiresOn { get; init; }

        /// <summary>
        /// Unix timestamp when the token is valid from
        /// </summary>
        [JsonPropertyName("not_before")]

        public required int NotBefore { get; init; }

        /// <summary>
        /// Resource for which the token is valid
        /// </summary>
        [JsonPropertyName("resource")]
        public required string Resource { get; init; }

        /// <summary>
        /// Token type
        /// </summary>
        /// <remarks>Will always return `Bearer`?</remarks>
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
