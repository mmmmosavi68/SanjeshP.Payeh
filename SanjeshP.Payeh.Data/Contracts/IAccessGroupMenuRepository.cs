using SanjeshP.Payeh.Entities.Menu;

namespace SanjeshP.Payeh.Data.Contracts
{
    public interface IAccessGroupMenuRepository : IEFRepository<GroupAccessMenus>
    {

        public Task<List<GroupAccessMenus>> GetAllByGroupIdAsync(Guid groupID, CancellationToken cancellationToken);
    }
}
