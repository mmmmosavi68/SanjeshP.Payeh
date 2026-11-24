
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.Api.DTOs.Users;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.WebFramework.Api;

namespace SanjeshP.Payeh.Api.Controllers.V1;

[ApiVersion("1")]
[Authorize]
public sealed class UserManagementController : BaseController
{
    private readonly IUserManagementService _service;

    public UserManagementController(IUserManagementService service)
    {
        _service = service;
    }

    /// <summary>
    /// دریافت فهرست منوهای فعال و قابل انتخاب
    /// </summary>
    [HttpGet("available-menus")]
    public async Task<ApiResult<List<UserAvailableMenuDto>>> GetAvailableMenus(
        CancellationToken cancellationToken)
    {
        var menus = await _service.GetAvailableMenusAsync(
            cancellationToken);

        var result = menus.Select(x => new UserAvailableMenuDto
        {
            Id = x.Id,
            MenuTitle = x.MenuTitle,
            ParentId = x.ParentId,
            PageCode = x.PageCode,
            Path = x.Path,
            Icon = x.Icon,
            ShowMenu = x.ShowMenu ?? false
        }).ToList();

        return new ApiResult<List<UserAvailableMenuDto>>(
            true,
            ApiResultStatusCode.Success,
            result);
    }

    /// <summary>
    /// دریافت دسترسی‌های مستقیم یک کاربر
    /// </summary>
    [HttpGet("{id:guid}/menu-access")]
    public async Task<ApiResult<List<Guid>>> GetMenuAccess(
        Guid id,
        CancellationToken cancellationToken)
    {
        var menuIds = await _service.GetAssignedMenuIdsAsync(
            id,
            cancellationToken);

        return new ApiResult<List<Guid>>(
            true,
            ApiResultStatusCode.Success,
            menuIds);
    }

    /// <summary>
    /// ثبت یا جایگزینی دسترسی مستقیم کاربر
    /// </summary>
    [HttpPut("{id:guid}/menu-access")]
    public async Task<ApiResult> UpdateMenuAccess(
        Guid id,
        [FromBody] UpdateUserMenuAccessDto dto,
        CancellationToken cancellationToken)
    {
        await _service.UpdateMenuAccessAsync(
            id,
            dto.MenuIds,
            CurrentUserId,
            cancellationToken);

        return new ApiResult(
            true,
            ApiResultStatusCode.Success,
            ApiMessages.UserManagement.M001);
    }


    /// <summary>
    /// دریافت فهرست دانشگاه‌های فعال و قابل تخصیص
    /// </summary>
    [HttpGet("available-universities")]
    public async Task<ApiResult<List<UniversityOption>>> GetAvailableUniversities(
        CancellationToken cancellationToken)
    {
        var universities = await _service.GetAvailableUniversitiesAsync(
            cancellationToken);

        return new ApiResult<List<UniversityOption>>(
            true,
            ApiResultStatusCode.Success,
            universities);
    }

    /// <summary>
    /// دریافت شناسه دانشگاه‌های تخصیص‌یافته به کاربر
    /// </summary>
    [HttpGet("{id:guid}/university-access")]
    public async Task<ApiResult<List<int>>> GetUniversityAccess(
        Guid id,
        CancellationToken cancellationToken)
    {
        var universityIds = await _service.GetAssignedUniversityIdsAsync(
            id,
            cancellationToken);

        return new ApiResult<List<int>>(
            true,
            ApiResultStatusCode.Success,
            universityIds);
    }

    /// <summary>
    /// ثبت یا جایگزینی دانشگاه‌های قابل دسترس کاربر
    /// </summary>
    [HttpPut("{id:guid}/university-access")]
    public async Task<ApiResult> UpdateUniversityAccess(
        Guid id,
        [FromBody] UpdateUserUniversityAccessDto dto,
        CancellationToken cancellationToken)
    {
        await _service.UpdateUniversityAccessAsync(
            id,
            dto.UniversityIds,
            CurrentUserId,
            cancellationToken);

        return new ApiResult(
            true,
            ApiResultStatusCode.Success,
            ApiMessages.UserManagement.M002);
    }

    /// <summary>
    /// غیرفعال‌سازی گروهی کاربران و ابطال نشست‌های فعال
    /// </summary>
    [HttpPost("bulk/deactivate")]
    public async Task<ApiResult> BulkDeactivate(
        [FromBody] BulkUserIdsDto dto,
        CancellationToken cancellationToken)
    {
        await _service.BulkDeactivateAsync(
            dto.UserIds,
            CurrentUserId,
            cancellationToken);

        return new ApiResult(
            true,
            ApiResultStatusCode.Success,
            $"{dto.UserIds.Distinct().Count()} کاربر با موفقیت غیرفعال شدند.");
    }

    /// <summary>
    /// حذف نرم گروهی کاربران و ابطال نشست‌های فعال
    /// </summary>
    [HttpDelete("bulk")]
    public async Task<ApiResult> BulkDelete(
        [FromBody] BulkUserIdsDto dto,
        CancellationToken cancellationToken)
    {
        await _service.BulkDeleteAsync(
            dto.UserIds,
            CurrentUserId,
            cancellationToken);

        return new ApiResult(
            true,
            ApiResultStatusCode.Success,
            $"{dto.UserIds.Distinct().Count()} کاربر با موفقیت حذف شدند.");
    }
}
