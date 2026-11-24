
using Asp.Versioning;
using SanjeshP.Payeh.Api.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.Api.DTOs.Menus;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Entities.Menu;
using SanjeshP.Payeh.Services.Menus;
using SanjeshP.Payeh.WebFramework.Api;

namespace SanjeshP.Payeh.Api.Controllers.V1;

/// <summary>
/// مدیریت ساختار منوهای سامانه.
/// </summary>
[ApiVersion("1")]
[Authorize]
public class MenusController : BaseController
{
    private readonly IMenusService _menusService;
    private readonly IApiMapper _mapper;

    public MenusController(
        IMenusService menusService,
        IApiMapper mapper)
    {
        _menusService = menusService;
        _mapper = mapper;
    }

    /// <summary>
    /// دریافت تمام منوهای حذف‌نشده، اعم از فعال و غیرفعال.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResult<List<MenuDto>>),
        StatusCodes.Status200OK)]
    public async Task<ApiResult<List<MenuDto>>> Get([FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        var result = await _menusService.GetPageAsync<MenuDto>(page, pageSize, _mapper.ProjectToDto, cancellationToken);
        Response.ApplyPaging(result);

        return result.Items.ToList();
    }

    /// <summary>
    /// دریافت اطلاعات یک منو.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(ApiResult<MenuDto>),
        StatusCodes.Status200OK)]
    public async Task<ApiResult<MenuDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var menu = await _menusService.GetByIdAsync(
            id,
            cancellationToken);

        return _mapper.ToDto(menu);
    }

    /// <summary>
    /// ایجاد منوی جدید.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(
        typeof(ApiResult<MenuDto>),
        StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResult<MenuDto>>> Create(
        [FromBody] CreateMenuDto dto,
        CancellationToken cancellationToken)
    {
        var entity = _mapper.ToEntity(dto);

        var created = await _menusService.CreateAsync(
            entity,
            CurrentUserId,
            cancellationToken);

        var result = new ApiResult<MenuDto>(
            true,
            ApiResultStatusCode.Success,
            _mapper.ToDto(created),
            "منو با موفقیت ایجاد شد.");

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id, version = "1" },
            result);
    }

    /// <summary>
    /// ویرایش اطلاعات منو.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(
        typeof(ApiResult<MenuDto>),
        StatusCodes.Status200OK)]
    public async Task<ApiResult<MenuDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateMenuDto dto,
        CancellationToken cancellationToken)
    {
        var entity = _mapper.ToEntity(dto);

        var updated = await _menusService.UpdateAsync(
            id,
            entity,
            cancellationToken);

        return _mapper.ToDto(updated);
    }

    /// <summary>
    /// حذف نرم منو.
    /// حذف منویی که زیرمنوی حذف‌نشده دارد مجاز نیست.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ApiResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _menusService.SoftDeleteAsync(
            id,
            cancellationToken);

        return new ApiResult(
            true,
            ApiResultStatusCode.Success,
            "منو با موفقیت حذف شد.");
    }
}
