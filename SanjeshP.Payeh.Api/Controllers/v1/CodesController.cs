
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.Api.DTOs.Codes;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.WebFramework.Api;
using SanjeshP.Payeh.Entities.Menu;

namespace SanjeshP.Payeh.Api.Controllers.V1;

/// <summary>
/// دریافت اطلاعات پایه و کدهای استاندارد سامانه.
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

    /// <summary>
    /// دریافت گروه‌های اطلاعات پایه.
    /// </summary>
    [HttpGet("groups")]
    [ProducesResponseType(
        typeof(ApiResult<List<CodeDto>>),
        StatusCodes.Status200OK)]
    public async Task<ApiResult<List<CodeDto>>> GetGroups(
        CancellationToken cancellationToken)
    {
        var groups = await _codesService.GetGroupsAsync(
            cancellationToken);

        return groups.Select(MapToDto).ToList();
    }

    /// <summary>
    /// دریافت گزینه‌های یک گروه بر اساس کد اصلی.
    /// </summary>
    [HttpGet("groups/{primaryCode:int}/items")]
    [ProducesResponseType(
        typeof(ApiResult<List<CodeDto>>),
        StatusCodes.Status200OK)]
    public async Task<ApiResult<List<CodeDto>>> GetItems(
        [FromRoute] int primaryCode,
        CancellationToken cancellationToken)
    {
        var items = await _codesService.GetItemsAsync(
            primaryCode,
            cancellationToken);

        return items.Select(MapToDto).ToList();
    }

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
            AdditionalInformation = code.AdditionalInformation
        };
    }
}
