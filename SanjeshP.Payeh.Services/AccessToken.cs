using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;


namespace SanjeshP.Payeh.Services
{
    public sealed class AccessToken
    {
        [JsonPropertyName("access_token")]
        public string AccessTokenValue { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        public AccessToken()
        {
        }

        public AccessToken(JwtSecurityToken securityToken, string refreshToken = null)
        {
            if (securityToken == null)
                throw new ArgumentNullException(nameof(securityToken));

            AccessTokenValue = new JwtSecurityTokenHandler().WriteToken(securityToken);

            RefreshToken = refreshToken;
            TokenType = "Bearer";

            double remainingSeconds = (securityToken.ValidTo - DateTime.UtcNow).TotalSeconds;

            ExpiresIn = remainingSeconds <= 0 ? 0 : (int)Math.Ceiling(remainingSeconds);
        }
    }
}
