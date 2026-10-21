using SanjeshP.Payeh.Entities.User;

namespace SanjeshP.Payeh.Data.Contracts
{
    /// <summary>
    /// مخزن نشست‌های کاربر (تغییر نام یافته از IUserTokenRepository).
    /// متدهای قبلی با معنای جدیدِ «نشست» حفظ شده‌اند و متدهای لازم برای
    /// Refresh چرخشی و فهرست نشست‌ها اضافه شده است.
    /// </summary>
    public interface IUserSessionRepository : IEFRepository<UserSession>
    {
        /// <summary>یافتن نشست بر اساس jti آخرین Access Token.</summary>
        Task<UserSession?> GetByJtiAsync(string jti, CancellationToken cancellationToken);

        /// <summary>یافتن نشست فعال بر اساس شناسه‌ی نشست (claim مربوط به sid).</summary>
        Task<UserSession?> GetActiveByIdAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken);

        /// <summary>یافتن نشست بر اساس هش Refresh Token فعلی (برای Refresh).</summary>
        Task<UserSession?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken);

        /// <summary>یافتن نشست بر اساس هش Refresh Token قبلی (تشخیص استفاده‌ی مجدد / سرقت).</summary>
        Task<UserSession?> GetByPreviousRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken);

        /// <summary>فهرست نشست‌های فعال کاربر (برای «فهرست دستگاه‌های من» و «خروج از همه»).</summary>
        Task<List<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    }
}