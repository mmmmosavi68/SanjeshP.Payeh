using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Entities.User;
using SanjeshP.Payeh.Common;

namespace SanjeshP.Payeh.Data.Repositories
{
    public class UserTokenRepository : EFRepository<UserToken>, IUserTokenRepository, IScopedDependency
    {
        public UserTokenRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<UserToken?> GetByJtiAsync(string jti, CancellationToken cancellationToken)
        {
            return await Table.FirstOrDefaultAsync(x => x.Jti == jti, cancellationToken);
        }

        public async Task<List<UserToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await Table
                .Where(x => x.UserId == userId && x.RevocationDate == null && x.TokenExpireDate > DateTime.UtcNow)
                .ToListAsync(cancellationToken);
        }
    }
}