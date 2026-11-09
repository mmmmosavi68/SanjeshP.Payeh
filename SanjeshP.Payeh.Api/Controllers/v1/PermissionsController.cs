using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.Api.Models.Auth;
using SanjeshP.Payeh.Services.Permissions;
using SanjeshP.Payeh.WebFramework.Api;
using SanjeshP.Payeh.WebFramework.Middlewares;

namespace SanjeshP.Payeh.Api.Controllers.V1;

/// <summary>منوها و Actionهای مجاز کاربر جاری.</summary>
[Authorize]
[ApiVersion("1")]
public sealed class PermissionsController : BaseController
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    /// <summary>دریافت منوی کاربر جاری بر اساس دسترسی مستقیم و گروهی.</summary>
    [HttpGet("me")]
    [SkipMenuPermission]
    [ProducesResponseType(typeof(ApiResult<List<MenuPermissionDto>>), StatusCodes.Status200OK)]
    public async Task<ApiResult<List<MenuPermissionDto>>> GetMyMenu(CancellationToken cancellationToken)
    {
        var items = await _permissionService.GetUserMenuItemsAsync(CurrentUserId, cancellationToken);
        return items.Select(item => new MenuPermissionDto
        {
            Id = item.Id,
            MenuTitle = item.MenuTitle,
            ParentId = item.ParentId,
            PageCode = item.PageCode,
            Version = item.version,
            ControllerName = item.ControllerName,
            ActionName = item.ActionName,
            ShowMenu = item.ShowMenu ?? false,
            Icon = item.Icon,
            Link = item.Link
        }).ToList();
    }
}
