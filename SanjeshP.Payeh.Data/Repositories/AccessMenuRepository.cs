using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Entities.Menu;

namespace SanjeshP.Payeh.Data.Repositories
{
    public class AccessMenuRepository : EFRepository<UserAccessMenus>, IAccessMenuRepository, IScopedDependency
    {
        public AccessMenuRepository(ApplicationDbContext dbContext)
          : base(dbContext)
        {
        }

        public async Task<List<UserAccessMenus>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await TableNoTracking.Where(u => u.UserId.Equals(userId)).ToListAsync(cancellationToken);
        }
    }
}
