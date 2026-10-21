using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Entities.User;

namespace SanjeshP.Payeh.Data.Repositories
{
    /// <summary>
    /// پیاده‌سازی مخزن نشست‌ها (تغییر نام یافته از UserTokenRepository).
    /// ثبت در DI از طریق نشانگر IScopedDependency انجام می‌شود.
    /// </summary>
    public class UserSessionRepository : EFRepository<UserSession>, IUserSessionRepository, IScopedDependency
    {
        public UserSessionRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        /// <inheritdoc />
        public async Task<UserSession?> GetByJtiAsync(string jti, CancellationToken cancellationToken)
        {
            return await Table.FirstOrDefaultAsync(x => x.Jti == jti, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<UserSession?> GetActiveByIdAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken)
        {
            return await Table.FirstOrDefaultAsync(
                x => x.Id == sessionId && x.UserId == userId && x.RevocationDate == null,
                cancellationToken);
        }

        /// <inheritdoc />
        public async Task<UserSession?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken)
        {
            return await Table
                .Include(x => x.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(x => x.RefreshTokenHash == refreshTokenHash, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<UserSession?> GetByPreviousRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken)
        {
            return await Table.FirstOrDefaultAsync(x => x.PreviousRefreshTokenHash == refreshTokenHash, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<List<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            return await TableNoTracking
                .Where(x => x.UserId == userId && x.RevocationDate == null && x.RefreshTokenExpireDate > now)
                .OrderByDescending(x => x.LastUsedAt ?? x.CreatedDate)
                .ToListAsync(cancellationToken);
        }
    }
}