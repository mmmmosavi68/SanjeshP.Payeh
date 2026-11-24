using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.Api.Models.Users;
using SanjeshP.Payeh.WebFramework.Api;
using SanjeshP.Payeh.WebFramework.Middlewares;

namespace SanjeshP.Payeh.Api.Controllers.V1
{
    /// <summary>فهرست نقش‌ها با عنوان فارسی و انگلیسی (برای فرم‌های مدیریت کاربر).</summary>
    [ApiVersion("1")]
    [Authorize]
    public class RolesController : BaseController
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService) => _roleService = roleService;

        /// <summary>فهرست نقش‌های فعال.</summary>
        [HttpGet]
        [SkipMenuPermission]
        [ProducesResponseType(typeof(ApiResult<List<RoleDto>>), StatusCodes.Status200OK)]
        public async Task<ApiResult<List<RoleDto>>> Get(CancellationToken cancellationToken)
        {
            var roles = await _roleService.GetActiveRolesAsync(cancellationToken);
            return roles
                .Select(r => new RoleDto { Id = r.Id, NameEn = r.RoleNameEn, NameFa = r.RoleNameFa })
                .ToList();
        }
    }
}
