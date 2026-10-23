
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;

namespace SanjeshP.Payeh.Services
{
    public sealed class AccessToken
    {
        [JsonPropertyName("access_token")]
        public string AccessTokenValue { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = "Bearer";

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        public AccessToken()
        {
        }

        public AccessToken(
            JwtSecurityToken securityToken,
            string? refreshToken = null)
        {
            ArgumentNullException.ThrowIfNull(securityToken);

            AccessTokenValue =
                new JwtSecurityTokenHandler().WriteToken(securityToken);

            RefreshToken = refreshToken ?? string.Empty;
            TokenType = "Bearer";

            var remainingSeconds =
                (securityToken.ValidTo - DateTime.UtcNow).TotalSeconds;

            ExpiresIn = remainingSeconds <= 0
                ? 0
                : (int)Math.Ceiling(remainingSeconds);
        }
    }
}
