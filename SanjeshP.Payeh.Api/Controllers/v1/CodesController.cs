
using SanjeshP.Payeh.Api.Mapping;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.Api.DTOs.Codes;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Entities.Menu;
using SanjeshP.Payeh.WebFramework.Api;

namespace SanjeshP.Payeh.Api.Controllers.V1;

/// <summary>
/// مدیریت اطلاعات پایه و کدهای استاندارد سامانه
/// </summary>
[ApiVersion("1")]
[Authorize]
public sealed class CodesController : BaseController
{
    private readonly ICodesService _codesService;
    private readonly IApiMapper _mapper;

    public CodesController(ICodesService codesService, IApiMapper mapper)
    {
        _codesService = codesService;
        _mapper = mapper;
    }

    #region Read

    /// <summary>
    /// دریافت تمام کدها برای نمایش ساختار درختی
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<List<CodeDto>>), StatusCodes.Status200OK)]
    public async Task<ApiResult<List<CodeDto>>> GetAll([FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        var result = await _codesService.GetPageAsync<CodeDto>(page, pageSize, _mapper.ProjectToDto, cancellationToken);
        Response.ApplyPaging(result);

        return result.Items.ToList();
    }

    /// <summary>
    /// دریافت اطلاعات یک کد
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResult<CodeDto>), StatusCodes.Status200OK)]
    public async Task<ApiResult<CodeDto>> GetById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var code = await _codesService.GetByIdAsync(id, cancellationToken);
        return _mapper.ToDto(code);
    }

    /// <summary>
    /// دریافت گروه‌های اطلاعات پایه
    /// </summary>
    [HttpGet("groups")]
    [ProducesResponseType(typeof(ApiResult<List<CodeDto>>), StatusCodes.Status200OK)]
    public async Task<ApiResult<List<CodeDto>>> GetGroups(
        CancellationToken cancellationToken)
    {
        var groups = await _codesService.GetGroupsAsync(cancellationToken);
        return groups.Select(_mapper.ToDto).ToList();
    }

    /// <summary>
    /// دریافت آیتم‌های یک گروه
    /// </summary>
    [HttpGet("groups/{primaryCode:int}/items")]
    [ProducesResponseType(typeof(ApiResult<List<CodeDto>>), StatusCodes.Status200OK)]
    public async Task<ApiResult<List<CodeDto>>> GetItems(
        [FromRoute] int primaryCode,
        CancellationToken cancellationToken)
    {
        var items = await _codesService.GetItemsAsync(
            primaryCode,
            cancellationToken);

        return items.Select(_mapper.ToDto).ToList();
    }
    /// <summary>
    /// دریافت گروه‌های اصلی برای مدیریت
    /// </summary>
    [HttpGet("management/groups")]
    [ProducesResponseType(
        typeof(ApiResult<List<CodeDto>>),
        StatusCodes.Status200OK)]
    public async Task<ApiResult<List<CodeDto>>> GetManagementGroups(
        CancellationToken cancellationToken)
    {
        var groups = await _codesService.GetManagementGroupsAsync(
            cancellationToken);

        return groups.Select(_mapper.ToDto).ToList();
    }

    /// <summary>
    /// دریافت اعضای یک گروه برای مدیریت
    /// </summary>
    [HttpGet("management/groups/{groupId:int}/items")]
    [ProducesResponseType(
        typeof(ApiResult<List<CodeDto>>),
        StatusCodes.Status200OK)]
    public async Task<ApiResult<List<CodeDto>>> GetManagementItems(
        [FromRoute] int groupId,
        CancellationToken cancellationToken)
    {
        var items = await _codesService.GetManagementItemsAsync(
            groupId,
            cancellationToken);

        return items.Select(_mapper.ToDto).ToList();
    }
    #endregion

    #region Create

    /// <summary>
    /// ایجاد گروه یا آیتم جدید
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<CodeDto>), StatusCodes.Status200OK)]
    public async Task<ApiResult<CodeDto>> Create(
        [FromBody] CreateCodeDto dto,
        CancellationToken cancellationToken)
    {
        var code = _mapper.ToEntity(dto);
        code.ParentId = await ResolveParentIdAsync(dto.ParentId, dto.ParentPrimaryCode, dto.ParentSubCode, cancellationToken);

        var createdCode = await _codesService.CreateAsync(
            code,
            CurrentUserId,
            cancellationToken);

        return _mapper.ToDto(createdCode);
    }

    #endregion

    #region Update

    /// <summary>
    /// ویرایش گروه یا آیتم
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResult<CodeDto>), StatusCodes.Status200OK)]
    public async Task<ApiResult<CodeDto>> Update(
        [FromRoute] int id,
        [FromBody] UpdateCodeDto dto,
        CancellationToken cancellationToken)
    {
        var updatedCode = _mapper.ToEntity(dto);
        updatedCode.ParentId = await ResolveParentIdAsync(dto.ParentId, dto.ParentPrimaryCode, dto.ParentSubCode, cancellationToken);

        var code = await _codesService.UpdateAsync(
            id,
            updatedCode,
            cancellationToken);

        return _mapper.ToDto(code);
    }

    #endregion

    #region Delete

    /// <summary>
    /// حذف نرم کد
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        await _codesService.SoftDeleteAsync(id, cancellationToken);
        return Ok();
    }

    #endregion

    #region Helpers

    private Task<int> ResolveParentIdAsync(int? parentId, int? parentPrimaryCode, int? parentSubCode, CancellationToken cancellationToken)
    {
        return _codesService.ResolveIdAsync(parentId, parentPrimaryCode, parentSubCode, null, ApiMessages.Lookup.ParentCodeInvalid, cancellationToken);
    }

    #endregion
}
