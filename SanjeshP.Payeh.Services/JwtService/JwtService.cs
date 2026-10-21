using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.Common.Utilities;
using SanjeshP.Payeh.Data;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Entities.User;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SanjeshP.Payeh.Services.JwtService
{
    /// <summary>
    /// صدور JWT کوتاه‌مدت + Refresh Token چرخشی به‌ازای هر نشست.
    /// نکات امنیتی:
    /// - Refresh Token تصادفی ۶۴ بایتی است و فقط هش آن ذخیره می‌شود.
    /// - در هر Refresh، توکن قبلی در PreviousRefreshTokenHash نگه داشته می‌شود؛
    ///   استفاده‌ی مجدد از توکن چرخش‌یافته ⇒ نشانه‌ی سرقت ⇒ ابطال کل نشست.
    /// - claim نقش‌ها از UserRoles خوانده می‌شود تا Authorize(Roles=...) کار کند.
    /// </summary>
    public class JwtService : IJwtService, IScopedDependency
    {
        private readonly SiteSettings _siteSettings;
        private readonly IUserSessionRepository _sessionRepository;
        private readonly ApplicationDbContext _dbContext;

        public JwtService(
            IOptions<SiteSettings> siteSettings,
            IUserSessionRepository sessionRepository,
            ApplicationDbContext dbContext)
        {
            _siteSettings = siteSettings.Value;
            _sessionRepository = sessionRepository;
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public Task<AccessToken> GenerateAsync(User user, string deviceName, string ipAddress, CancellationToken cancellationToken)
            => GenerateAsync(user, deviceName, ipAddress, userAgent: null, cancellationToken);

        /// <inheritdoc />
        public async Task<AccessToken> GenerateAsync(User user, string deviceName, string ipAddress, string? userAgent, CancellationToken cancellationToken)
        {
            // اگر ورود چنددستگاهی مجاز نیست، نشست‌های قبلی باطل می‌شوند.
            if (!_siteSettings.AllowMultipleSessions)
                await RevokeAllAsync(user.Id, cancellationToken);

            var now = DateTime.UtcNow;
            var jti = Guid.NewGuid().ToString("N");
            var refreshToken = GenerateRefreshToken();

            var session = new UserSession
            {
                UserId = user.Id,
                Jti = jti,
                RefreshTokenHash = SecurityHelper.GetSha256Hash(refreshToken),
                DeviceName = string.IsNullOrWhiteSpace(deviceName) ? "دستگاه ناشناس" : deviceName.Trim(),
                UserAgent = userAgent,
                IpAddress = ipAddress,
                TokenExpireDate = now.AddMinutes(_siteSettings.JwtSettings.ExpirationMinutes),
                RefreshTokenExpireDate = now.AddDays(_siteSettings.RefreshTokenSettings.ExpirationDays),
                LastUsedAt = now,
                CreatedBy = user.Id
            };

            await _sessionRepository.AddAsync(session, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return BuildAccessToken(user, session, refreshToken);
        }

        /// <inheritdoc />
        public async Task<AccessToken> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new UnauthorizedException("توکن تازه‌سازی ارسال نشده است.");

            var hash = SecurityHelper.GetSha256Hash(refreshToken);
            var now = DateTime.UtcNow;

            var session = await _sessionRepository.GetByRefreshTokenHashAsync(hash, cancellationToken);

            if (session is null)
            {
                // تشخیص استفاده‌ی مجدد: توکن قبلاً چرخش خورده ⇒ احتمال سرقت ⇒ ابطال کل نشست.
                var reused = await _sessionRepository.GetByPreviousRefreshTokenHashAsync(hash, cancellationToken);
                if (reused is not null && reused.RevocationDate is null)
                {
                    reused.RevocationDate = now;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                throw new UnauthorizedException("توکن تازه‌سازی نامعتبر است. لطفاً دوباره وارد شوید.");
            }

            if (session.RevocationDate is not null || session.IsDeleted || !session.IsActive)
                throw new UnauthorizedException("این نشست باطل شده است. لطفاً دوباره وارد شوید.");

            if (session.RefreshTokenExpireDate <= now)
                throw new UnauthorizedException("نشست منقضی شده است. لطفاً دوباره وارد شوید.");

            if (session.User is null || session.User.IsDeleted || !session.User.IsActive)
                throw new UnauthorizedException("حساب کاربری غیرفعال است.");

            // چرخش توکن: هش قبلی نگه داشته می‌شود و توکن جدید صادر می‌گردد.
            var newRefreshToken = GenerateRefreshToken();
            session.PreviousRefreshTokenHash = session.RefreshTokenHash;
            session.RefreshTokenHash = SecurityHelper.GetSha256Hash(newRefreshToken);
            session.Jti = Guid.NewGuid().ToString("N");
            session.TokenExpireDate = now.AddMinutes(_siteSettings.JwtSettings.ExpirationMinutes);
            session.RefreshTokenExpireDate = now.AddDays(_siteSettings.RefreshTokenSettings.ExpirationDays);
            session.LastUsedAt = now;

            _sessionRepository.Update(session, saveNow: false);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return BuildAccessToken(session.User, session, newRefreshToken);
        }

        /// <inheritdoc />
        public async Task RevokeAsync(string jti, CancellationToken cancellationToken)
        {
            var session = await _sessionRepository.GetByJtiAsync(jti, cancellationToken);
            if (session is not null && session.RevocationDate is null)
            {
                session.RevocationDate = DateTime.UtcNow;
                _sessionRepository.Update(session, saveNow: false);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        /// <inheritdoc />
        public async Task RevokeSessionAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken)
        {
            var session = await _sessionRepository.GetActiveByIdAsync(sessionId, userId, cancellationToken)
                ?? throw new NotFoundException("نشست مورد نظر یافت نشد.");

            session.RevocationDate = DateTime.UtcNow;
            _sessionRepository.Update(session, saveNow: false);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken)
        {
            var sessions = await _sessionRepository.GetActiveSessionsByUserIdAsync(userId, cancellationToken);
            var now = DateTime.UtcNow;
            foreach (var session in sessions)
            {
                session.RevocationDate = now;
                _sessionRepository.Update(session, saveNow: false);
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> ValidateSessionAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken)
        {
            var session = await _sessionRepository.GetActiveByIdAsync(sessionId, userId, cancellationToken);
            return session is not null && session.IsActive && !session.IsDeleted;
        }

        /// <inheritdoc />
        public async Task<bool> ValidateTokenAsync(string jti, Guid userId, CancellationToken cancellationToken)
        {
            var session = await _sessionRepository.GetByJtiAsync(jti, cancellationToken);
            return session is not null
                && session.UserId == userId
                && session.RevocationDate is null
                && session.TokenExpireDate > DateTime.UtcNow;
        }

        /// <inheritdoc />
        public Task<List<UserSession>> GetActiveSessionsAsync(Guid userId, CancellationToken cancellationToken)
            => _sessionRepository.GetActiveSessionsByUserIdAsync(userId, cancellationToken);

        /// <summary>ساخت AccessToken با claimهای کاربر، نقش‌ها و شناسه‌ی نشست.</summary>
        private AccessToken BuildAccessToken(User user, UserSession session, string refreshToken)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, session.Jti),
                new(IJwtService.SessionIdClaimType, session.Id.ToString())
            };

            // نقش‌ها برای Authorize(Roles = "Admin") و Policyها
            foreach (var userRole in user.UserRoles.Where(ur => ur.Role is not null && ur.Role.IsActive))
                claims.Add(new Claim(ClaimTypes.Role, userRole.Role.RoleNameEn));

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_siteSettings.JwtSettings.SecretKey)),
                SecurityAlgorithms.HmacSha256Signature);

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                NotBefore = DateTime.UtcNow.AddMinutes(-_siteSettings.JwtSettings.NotBeforeMinutes),
                Expires = session.TokenExpireDate,
                SigningCredentials = signingCredentials,
                Issuer = _siteSettings.JwtSettings.Issuer,
                Audience = _siteSettings.JwtSettings.Audience
            };

            var securityToken = new JwtSecurityTokenHandler().CreateJwtSecurityToken(descriptor);

            return new AccessToken(securityToken) { RefreshToken = refreshToken };
        }

        /// <summary>تولید Refresh Token تصادفی رمزنگارانه (۶۴ بایت، Base64Url).</summary>
        private static string GenerateRefreshToken()
            => Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
    }
}