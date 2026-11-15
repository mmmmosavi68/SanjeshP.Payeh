
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

    public CodesController(ICodesService codesService)
    {
        _codesService = codesService;
    }

    #region Read

    /// <summary>
    /// دریافت تمام کدها برای نمایش ساختار درختی
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<List<CodeDto>>), StatusCodes.Status200OK)]
    public async Task<ApiResult<List<CodeDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var codes = await _codesService.GetAllAsync(cancellationToken);
        return codes.Select(MapToDto).ToList();
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
        return MapToDto(code);
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
        return groups.Select(MapToDto).ToList();
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

        return items.Select(MapToDto).ToList();
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
        var code = new Code
        {
            Title = dto.Title,
            ParentId = dto.ParentId,
            StandardCode = dto.StandardCode,
            PrimaryCode = dto.PrimaryCode,
            SubCode = dto.SubCode,
            AdditionalInformation = dto.AdditionalInformation
        };

        var createdCode = await _codesService.CreateAsync(
            code,
            CurrentUserId,
            cancellationToken);

        return MapToDto(createdCode);
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
        var updatedCode = new Code
        {
            Title = dto.Title,
            ParentId = dto.ParentId,
            StandardCode = dto.StandardCode,
            PrimaryCode = dto.PrimaryCode,
            SubCode = dto.SubCode,
            AdditionalInformation = dto.AdditionalInformation,
            IsActive = dto.IsActive
        };

        var code = await _codesService.UpdateAsync(
            id,
            updatedCode,
            cancellationToken);

        return MapToDto(code);
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

    #region Mapping

    private static CodeDto MapToDto(Code code)
    {
        return new CodeDto
        {
            Id = code.Id,
            Title = code.Title,
            ParentId = code.ParentId,
            StandardCode = code.StandardCode,
            PrimaryCode = code.PrimaryCode,
            SubCode = code.SubCode,
            AdditionalInformation = code.AdditionalInformation,
            IsActive = code.IsActive
        };
    }

    #endregion
}
