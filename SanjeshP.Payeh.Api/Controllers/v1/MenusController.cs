
using Asp.Versioning;
using AutoMapper;
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
    private readonly IMapper _mapper;

    public MenusController(
        IMenusService menusService,
        IMapper mapper)
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
    public async Task<ApiResult<List<MenuDto>>> Get(
        CancellationToken cancellationToken)
    {
        var menus = await _menusService.GetAllAsync(cancellationToken);

        var result = _mapper.Map<List<MenuDto>>(menus);

        return result;
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

        return MenuDto.FromEntity(_mapper, menu);
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
        var entity = _mapper.Map<Menu>(dto);

        var created = await _menusService.CreateAsync(
            entity,
            CurrentUserId,
            cancellationToken);

        var result = new ApiResult<MenuDto>(
            true,
            ApiResultStatusCode.Success,
            MenuDto.FromEntity(_mapper, created),
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
        var entity = _mapper.Map<Menu>(dto);

        var updated = await _menusService.UpdateAsync(
            id,
            entity,
            cancellationToken);

        return MenuDto.FromEntity(_mapper, updated);
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
