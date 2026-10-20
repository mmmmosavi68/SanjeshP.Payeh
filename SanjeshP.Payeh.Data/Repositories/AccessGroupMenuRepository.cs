using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Entities.Menu;

namespace SanjeshP.Payeh.Data.Repositories
{
    public class AccessGroupMenuRepository : EFRepository<GroupAccessMenus>, IAccessGroupMenuRepository, IScopedDependency
    {
        public AccessGroupMenuRepository(ApplicationDbContext dbContext)
          : base(dbContext)
        {
        }

        public async Task<List<GroupAccessMenus>> GetAllByGroupIdAsync(Guid groupID, CancellationToken cancellationToken)
        {
            return await TableNoTracking.Where(u => u.GroupId.Equals(groupID)).ToListAsync(cancellationToken);

        }
    }
}
