using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Data;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Entities.User;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SanjeshP.Payeh.Services.JwtService
{
    public class JwtService : IJwtService, IScopedDependency
    {
        private readonly SiteSettings _siteSettings;
        private readonly IUserTokenRepository _userTokenRepository;
        private readonly ApplicationDbContext _dbContext;

        public JwtService(IOptions<SiteSettings> siteSettings, IUserTokenRepository userTokenRepository, ApplicationDbContext dbContext)
        {
            _siteSettings = siteSettings.Value;
            _userTokenRepository = userTokenRepository;
            _dbContext = dbContext;
        }

        public async Task<AccessToken> GenerateAsync(User user, string deviceName, string ipAddress, CancellationToken cancellationToken)
        {
            var jti = Guid.NewGuid().ToString();
            var expires = DateTime.UtcNow.AddMinutes(_siteSettings.JwtSettings.ExpirationMinutes);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, jti)
            };

            var secretKey = Encoding.UTF8.GetBytes(_siteSettings.JwtSettings.SecretKey);
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature);

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                SigningCredentials = signingCredentials,
                Issuer = _siteSettings.JwtSettings.Issuer,
                Audience = _siteSettings.JwtSettings.Audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            // استفاده از CreateJwtSecurityToken برای تولید خروجی از نوع JwtSecurityToken سازگار با کلاس AccessToken
            var securityToken = tokenHandler.CreateJwtSecurityToken(descriptor);

            var userToken = new UserToken
            {
                UserId = user.Id,
                Jti = jti,
                DeviceName = deviceName,
                IpAddress = ipAddress,
                TokenExpireDate = expires,
                RefreshTokenExpireDate = expires.AddDays(7)
            };

            await _userTokenRepository.AddAsync(userToken, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // ساخت شیء AccessToken با استفاده از سازنده‌ای که در فایل AccessToken.cs خودتان تعریف کرده‌اید
            return new AccessToken(securityToken);
        }

        public async Task RevokeAsync(string jti, CancellationToken cancellationToken)
        {
            var userToken = await _userTokenRepository.GetByJtiAsync(jti, cancellationToken);
            if (userToken != null)
            {
                userToken.RevocationDate = DateTime.UtcNow;
                _userTokenRepository.Update(userToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken)
        {
            var tokens = await _userTokenRepository.GetActiveTokensByUserIdAsync(userId, cancellationToken);
            foreach (var token in tokens)
            {
                token.RevocationDate = DateTime.UtcNow;
                _userTokenRepository.Update(token);
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> ValidateTokenAsync(string jti, Guid userId, CancellationToken cancellationToken)
        {
            var userToken = await _userTokenRepository.GetByJtiAsync(jti, cancellationToken);
            return userToken != null && userToken.UserId == userId && userToken.RevocationDate == null && userToken.TokenExpireDate > DateTime.UtcNow;
        }
    }
}