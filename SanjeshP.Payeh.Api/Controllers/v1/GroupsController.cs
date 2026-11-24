
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.Api.DTOs.Groups;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Entities.Group;
using SanjeshP.Payeh.WebFramework.Api;

namespace SanjeshP.Payeh.Api.Controllers.V1;

[ApiVersion("1")]
[Authorize]
public sealed class GroupsController : BaseController
{
    private readonly IGroupService _groupService;

    public GroupsController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    /// <summary>
    /// دریافت فهرست گروه‌ها
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<List<GroupDto>>> Get(
        CancellationToken cancellationToken)
    {
        var groups = await _groupService.GetAllAsync(
            cancellationToken);

        var result = groups
            .Select(MapToDto)
            .ToList();

        return new ApiResult<List<GroupDto>>(
            true,
            ApiResultStatusCode.Success,
            result);
    }

    /// <summary>
    /// دریافت اطلاعات یک گروه
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ApiResult<GroupDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var group = await _groupService.GetByIdAsync(
            id,
            cancellationToken);

        var result = MapToDto(group);

        return new ApiResult<GroupDto>(
            true,
            ApiResultStatusCode.Success,
            result);
    }

    /// <summary>
    /// ایجاد گروه جدید
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResult<GroupDto>>> Create(
        [FromBody] CreateGroupDto dto,
        CancellationToken cancellationToken)
    {
        var group = await _groupService.CreateAsync(
            dto.GroupName,
            dto.IsActive,
            CurrentUserId,
            cancellationToken);

        var result = MapToDto(group);

        var response = new ApiResult<GroupDto>(
            true,
            ApiResultStatusCode.Success,
            result,
            ApiMessages.Groups.M001);

        return CreatedAtAction(
            nameof(GetById),
            new { id = group.Id, version = "1" },
            response);
    }

    /// <summary>
    /// ویرایش گروه
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ApiResult<GroupDto>> Update(
        Guid id,
        [FromBody] UpdateGroupDto dto,
        CancellationToken cancellationToken)
    {
        var group = await _groupService.UpdateAsync(
            id,
            dto.GroupName,
            dto.IsActive,
            cancellationToken);

        var result = MapToDto(group);

        return new ApiResult<GroupDto>(
            true,
            ApiResultStatusCode.Success,
            result,
            ApiMessages.Groups.M002);
    }

    /// <summary>
    /// حذف نرم گروه
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ApiResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _groupService.SoftDeleteAsync(
            id,
            cancellationToken);

        return new ApiResult(
            true,
            ApiResultStatusCode.Success,
            ApiMessages.Groups.M003);
    }

    /// <summary>
    /// دریافت اعضای یک گروه
    /// </summary>
    [HttpGet("{id:guid}/users")]
    public async Task<ApiResult<List<GroupMemberDto>>> GetUsers(
        Guid id,
        CancellationToken cancellationToken)
    {
        var members = await _groupService.GetUsersAsync(
            id,
            cancellationToken);

        var result = members
            .Select(MapToMemberDto)
            .ToList();

        return new ApiResult<List<GroupMemberDto>>(
            true,
            ApiResultStatusCode.Success,
            result);
    }

    /// <summary>
    /// اضافه کردن کاربر به گروه
    /// </summary>
    [HttpPost("{id:guid}/users")]
    public async Task<ApiResult> AddUser(
        Guid id,
        [FromBody] AddGroupUserDto dto,
        CancellationToken cancellationToken)
    {
        await _groupService.AddUserAsync(
            id,
            dto.UserId,
            CurrentUserId,
            cancellationToken);

        return new ApiResult(
            true,
            ApiResultStatusCode.Success,
            ApiMessages.Groups.M004);
    }

    /// <summary>
    /// حذف کاربر از گروه
    /// </summary>
    [HttpDelete("{id:guid}/users/{userId:guid}")]
    public async Task<ApiResult> RemoveUser(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await _groupService.RemoveUserAsync(
            id,
            userId,
            cancellationToken);

        return new ApiResult(
            true,
            ApiResultStatusCode.Success,
            ApiMessages.Groups.M005);
    }


    /// <summary>
    /// افزودن گروهی کاربران به گروه
    /// </summary>
    [HttpPost("{id:guid}/users/bulk")]
    public async Task<ApiResult> AddUsers(
        Guid id,
        [FromBody] BulkGroupUsersDto dto,
        CancellationToken cancellationToken)
    {
        await _groupService.AddUsersAsync(
            id,
            dto.UserIds,
            CurrentUserId,
            cancellationToken);

        return new ApiResult(
            true,
            ApiResultStatusCode.Success,
            $"{dto.UserIds.Distinct().Count()} کاربر با موفقیت به گروه اضافه شدند.");
    }

    /// <summary>
    /// حذف گروهی کاربران از گروه
    /// </summary>
    [HttpDelete("{id:guid}/users/bulk")]
    public async Task<ApiResult> RemoveUsers(
        Guid id,
        [FromBody] BulkGroupUsersDto dto,
        CancellationToken cancellationToken)
    {
        await _groupService.RemoveUsersAsync(
            id,
            dto.UserIds,
            cancellationToken);

        return new ApiResult(
            true,
            ApiResultStatusCode.Success,
            $"{dto.UserIds.Distinct().Count()} کاربر با موفقیت از گروه خارج شدند.");
    }

    #region Group Menu Access

    /// <summary>
    /// دریافت فهرست منوهای فعال و قابل انتخاب
    /// </summary>
    [HttpGet("available-menus")]
    public async Task<ApiResult<List<GroupMenuDto>>> GetAvailableMenus(
        CancellationToken cancellationToken)
    {
        var menus = await _groupService.GetAvailableMenusAsync(
            cancellationToken);

        var result = menus
            .Select(x => new GroupMenuDto
            {
                Id = x.Id,
                MenuTitle = x.MenuTitle,
                ParentId = x.ParentId,
                PageCode = x.PageCode,
                Path = x.Path,
                Icon = x.Icon,
                ShowMenu = x.ShowMenu
            })
            .ToList();

        return new ApiResult<List<GroupMenuDto>>(
            true,
            ApiResultStatusCode.Success,
            result);
    }

    /// <summary>
    /// دریافت شناسه منوهای اختصاص‌یافته به گروه
    /// </summary>
    [HttpGet("{id:guid}/menu-access")]
    public async Task<ApiResult<List<Guid>>> GetMenuAccess(
        Guid id,
        CancellationToken cancellationToken)
    {
        var menuIds = await _groupService.GetAssignedMenuIdsAsync(
            id,
            cancellationToken);

        return new ApiResult<List<Guid>>(
            true,
            ApiResultStatusCode.Success,
            menuIds);
    }

    /// <summary>
    /// ثبت یا جایگزینی دسترسی منوهای گروه
    /// </summary>
    [HttpPut("{id:guid}/menu-access")]
    public async Task<ApiResult> UpdateMenuAccess(
        Guid id,
        [FromBody] UpdateGroupMenuAccessDto dto,
        CancellationToken cancellationToken)
    {
        await _groupService.UpdateMenuAccessAsync(
            id,
            dto.MenuIds,
            CurrentUserId,
            cancellationToken);

        return new ApiResult(
            true,
            ApiResultStatusCode.Success,
            ApiMessages.Groups.M006);
    }

    #endregion

    #region Mapping

    private static GroupDto MapToDto(Group group)
    {
        return new GroupDto
        {
            Id = group.Id,
            GroupName = group.GroupName,
            IsActive = group.IsActive,
            CreatedDate = group.CreatedDate,
            UserCount = group.GroupUsers?
                .Count(x => x.IsActive && !x.IsDeleted) ?? 0
        };
    }

    private static GroupMemberDto MapToMemberDto(GroupUsers member)
    {
        var profile = member.User?.UserProfile?
            .FirstOrDefault();

        return new GroupMemberDto
        {
            UserId = member.UserId,
            UserName = member.User?.UserName ?? string.Empty,
            FirstName = profile?.FirstName,
            LastName = profile?.LastName,
            PhoneNumber = member.User?.PhoneNumber,
            IsActive = member.IsActive && (member.User?.IsActive ?? false)
        };
    }

    #endregion
}
