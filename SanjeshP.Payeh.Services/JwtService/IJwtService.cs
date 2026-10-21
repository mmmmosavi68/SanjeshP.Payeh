using SanjeshP.Payeh.Entities.User;

namespace SanjeshP.Payeh.Services.JwtService
{
    /// <summary>
    /// سرویس صدور و مدیریت توکن‌ها بر پایه‌ی نشست (Session).
    /// هر ورود یک نشست مستقل می‌سازد؛ Access Token کوتاه‌مدت و Refresh Token چرخشی
    /// به همان نشست متصل‌اند. خروج از یک دستگاه فقط نشست خودش را باطل می‌کند.
    /// </summary>
    public interface IJwtService
    {
        /// <summary>نام claim شناسه‌ی نشست در Access Token.</summary>
        const string SessionIdClaimType = "session_id";

        /// <summary>ایجاد نشست جدید و صدور جفت توکن (ورود موفق).</summary>
        Task<AccessToken> GenerateAsync(User user, string deviceName, string ipAddress, string? userAgent, CancellationToken cancellationToken);

        /// <summary>سازگار با امضای قبلی (بدون UserAgent) — Breaking نیست.</summary>
        Task<AccessToken> GenerateAsync(User user, string deviceName, string ipAddress, CancellationToken cancellationToken);

        /// <summary>چرخش Refresh Token و صدور Access Token جدید برای همان نشست.</summary>
        Task<AccessToken> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

        /// <summary>ابطال نشستِ متعلق به jti مشخص (سازگار با نسخه‌ی قبل).</summary>
        Task RevokeAsync(string jti, CancellationToken cancellationToken);

        /// <summary>ابطال یک نشست مشخص از خود کاربر (خروج از این دستگاه).</summary>
        Task RevokeSessionAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken);

        /// <summary>ابطال همه‌ی نشست‌های کاربر (خروج از همه‌ی دستگاه‌ها).</summary>
        Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken);

        /// <summary>اعتبارسنجی نشست در هر درخواست (رویداد OnTokenValidated).</summary>
        Task<bool> ValidateSessionAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken);

        /// <summary>سازگار با نسخه‌ی قبل؛ اعتبارسنجی بر اساس jti.</summary>
        Task<bool> ValidateTokenAsync(string jti, Guid userId, CancellationToken cancellationToken);

        /// <summary>فهرست نشست‌های فعال کاربر (دستگاه‌های من).</summary>
        Task<List<UserSession>> GetActiveSessionsAsync(Guid userId, CancellationToken cancellationToken);
    }
}