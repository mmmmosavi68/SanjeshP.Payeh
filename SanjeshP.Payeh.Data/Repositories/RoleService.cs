using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Data;
using SanjeshP.Payeh.Entities.User;

namespace SanjeshP.Payeh.Services.Roles
{
    public class RoleService : IRoleService, IScopedDependency
    {
        private readonly ApplicationDbContext _dbContext;

        public RoleService(ApplicationDbContext dbContext) => _dbContext = dbContext;

        public async Task<IReadOnlyList<Role>> GetActiveRolesAsync(CancellationToken cancellationToken)
            => await _dbContext.Set<Role>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsActive)
                .OrderBy(x => x.Id)
                .ToListAsync(cancellationToken);
    }
}
