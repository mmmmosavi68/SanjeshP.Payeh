using SanjeshP.Payeh.Entities.Menu;

namespace SanjeshP.Payeh.Data.Contracts
{
    public interface IView_UserMenubarRepository
    {
        Task<List<View_UserMenubar>> GetUserSelectedAccessMenuByUserCurrentIdAndUSerSelectedID(Guid userCurrent_Userid, Guid UserSelected_UserId, CancellationToken cancellationToken);
        Task<List<View_UserMenubar>> GetGroupSelectedAccessMenuByUserCurrentIdAndGroupSelectedID(Guid userCurrent_Userid, Guid GroupSelected_GroupId, CancellationToken cancellationToken);
        Task<bool> GetUserMenuByControllerNameAndActionName(Guid userid, string controllerName, string actionName, CancellationToken cancellationToken);
        Task<List<View_UserMenubar>> GetUserAccessMenu(Guid userid, CancellationToken cancellationToken);
    }

}
