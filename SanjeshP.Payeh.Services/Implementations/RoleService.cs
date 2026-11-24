using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Data;
using SanjeshP.Payeh.Entities.User;

namespace SanjeshP.Payeh.Services.Implementations
{
    public class RoleService : IRoleService, IScopedDependency
    {
        private readonly IUnitOfWork _uow;

        public RoleService(IUnitOfWork uow) => _uow = uow;

        public async Task<IReadOnlyList<Role>> GetActiveRolesAsync(CancellationToken cancellationToken)
            => await _uow.Repository<Role>().Entities
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsActive)
                .OrderBy(x => x.Id)
                .ToListAsync(cancellationToken);
    }
}
