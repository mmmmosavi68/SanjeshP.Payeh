using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Utilities;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Entities.Menu;
using System.Data;

namespace SanjeshP.Payeh.Data.Repositories
{
    public class View_UserMenubarRepository : EFRepository<View_UserMenubar>, IView_UserMenubarRepository, IScopedDependency
    {
        public View_UserMenubarRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public async Task<List<View_UserMenubar>> GetUserSelectedAccessMenuByUserCurrentIdAndUSerSelectedID(Guid userCurrent_Userid, Guid UserSelected_UserId, CancellationToken cancellationToken)
        {
            CreateSqlParameter createSqlParameter = new CreateSqlParameter();
            SqlParameter[] param_userid = new SqlParameter[2];
            param_userid[0] = createSqlParameter.Create_Param("@CurrentUserId", userCurrent_Userid, SqlDbType.UniqueIdentifier, 16);
            param_userid[1] = createSqlParameter.Create_Param("@UserSelected_UserId", UserSelected_UserId, SqlDbType.UniqueIdentifier, 16);
            return await Entities.FromSqlRaw($"[dbo].[Get_UserSelectedAccess_With_CurrentUserAccess_For_TreeCheckBox] @CurrentUserId,@UserSelected_UserId", param_userid).AsNoTracking().ToListAsync(cancellationToken);
        }
        public async Task<List<View_UserMenubar>> GetGroupSelectedAccessMenuByUserCurrentIdAndGroupSelectedID(Guid userCurrent_Userid, Guid GroupSelected_GroupId, CancellationToken cancellationToken)
        {
            CreateSqlParameter createSqlParameter = new CreateSqlParameter();
            SqlParameter[] param_userid = new SqlParameter[2];
            param_userid[0] = createSqlParameter.Create_Param("@CurrentUserId", userCurrent_Userid, SqlDbType.UniqueIdentifier, 16);
            param_userid[1] = createSqlParameter.Create_Param("@GroupSelected_GroupId", GroupSelected_GroupId, SqlDbType.UniqueIdentifier, 16);

            return await Entities.FromSqlRaw($"[dbo].[Get_GroupAccess_With_CurrentUserAccess_For_TreeCheckBox] @CurrentUserId,@GroupSelected_GroupId", param_userid).AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<bool> GetUserMenuByControllerNameAndActionName(Guid userid, string controllerName, string actionName, CancellationToken cancellationToken)
        {
            CreateSqlParameter createSqlParameter = new CreateSqlParameter();
            SqlParameter[] param_userid = new SqlParameter[1];
            param_userid[0] = createSqlParameter.Create_Param("@CurrentUserId", userid, SqlDbType.UniqueIdentifier, 16);

            var result = await Entities.FromSqlRaw($"EXEC Get_UserCurrentAccess @CurrentUserId", param_userid).AsNoTracking().ToListAsync(cancellationToken);
            return result.Any(u => u.ControllerName == (controllerName)
                                 && u.ActionName == (actionName)
                                 && (u.Person_Checkecd == true || u.Group_Checkecd == true));
        }
        public async Task<List<View_UserMenubar>> GetUserAccessMenu(Guid userid, CancellationToken cancellationToken)
        {
            CreateSqlParameter createSqlParameter = new CreateSqlParameter();
            SqlParameter[] param_userid = new SqlParameter[1];
            param_userid[0] = createSqlParameter.Create_Param("@CurrentUserId", userid, SqlDbType.UniqueIdentifier, 16);

            var result = await Entities.FromSqlRaw($"EXEC Get_UserCurrentAccess @CurrentUserId", param_userid).AsNoTracking().ToListAsync(cancellationToken);
            return result;
        }
    }
}
