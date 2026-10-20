using SanjeshP.Payeh.Entities.Menu;

namespace SanjeshP.Payeh.Data.Contracts

{

    public interface IAccessMenuRepository : IEFRepository<UserAccessMenus>
    {

      public  Task<List<UserAccessMenus>> GetAllByUserIdAsync(Guid userId,  CancellationToken cancellationToken);
    }
}
