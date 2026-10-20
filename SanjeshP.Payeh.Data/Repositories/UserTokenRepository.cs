using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Entities.User;

namespace SanjeshP.Payeh.Data.Repositories
{
    public class UserTokenRepository
        : EFRepository<UserToken>, IUserTokenRepository, IScopedDependency
    {
        public UserTokenRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
        }

        public async Task<UserToken> CreateToken(User user, bool allowMultipleSessions, CancellationToken cancellationToken)
        {
            // فقط وقتی ورود هم‌زمان غیرفعال است،
            // نشست‌های قبلی کاربر را باطل کن.
            if (!allowMultipleSessions)
            {
                var previousTokens = await Table
                    .Where(token =>
                        token.UserId == user.Id &&
                        !token.IsDeleted &&
                        token.IsActive)
                    .ToListAsync(cancellationToken);

                foreach (var token in previousTokens)
                {
                    token.IsActive = false;
                    token.IsDeleted = true;
                    token.UpdatedDate = DateTime.UtcNow;
                }
            }

            var now = DateTime.UtcNow;

            var userToken = new UserToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                SessionId = Guid.NewGuid().ToString(),
                UserAgent = string.Empty,
                CreatedBy = user.Id,
                CreatedDate = now,
                ExpirationDate = now.AddMinutes(20),
                IsActive = true,
                IsDeleted = false
            };

            await base.AddAsync(userToken, cancellationToken, saveNow: false);

            await DbContext.SaveChangesAsync(cancellationToken);

            return userToken;
        }
    }
}