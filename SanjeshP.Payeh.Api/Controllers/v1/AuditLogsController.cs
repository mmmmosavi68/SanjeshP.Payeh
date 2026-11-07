using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.Api.Models.Audit;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.WebFramework.Api;

namespace SanjeshP.Payeh.Api.Controllers.V1;

/// <summary>گزارش سوابق ممیزی سامانه؛ دسترسی صرفاً برای مدیر سیستم.</summary>
[ApiVersion("1")]
[Authorize(Policy = "UserManagement")]
public sealed class AuditLogsController : BaseController
{
    private readonly IAuditLogQueryService _auditLogQueryService;

    public AuditLogsController(IAuditLogQueryService auditLogQueryService)
    {
        _auditLogQueryService = auditLogQueryService;
    }

    /// <summary>جستجوی صفحه‌بندی‌شده سوابق ممیزی.</summary>
    /// <remarks>
    /// تاریخ‌ها با UTC ارسال شوند. pageSize حداکثر 100 است.
    /// EventTypeهای متداول: Activity، Security و Request.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<AuditLogPagedDto>), StatusCodes.Status200OK)]
    public async Task<ApiResult<AuditLogPagedDto>> Search(
        [FromQuery] AuditLogQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        if (parameters.FromUtc.HasValue && parameters.ToUtc.HasValue && parameters.FromUtc > parameters.ToUtc)
            throw new BadRequestException("تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.");

        var result = await _auditLogQueryService.SearchAsync(
            new AuditLogQuery(
                parameters.PageNumber,
                parameters.PageSize,
                parameters.FromUtc,
                parameters.ToUtc,
                parameters.UserId,
                parameters.UserName,
                parameters.IpAddress,
                parameters.EventType,
                parameters.Action,
                parameters.EntityName,
                parameters.EntityId,
                parameters.StatusCode,
                parameters.Search),
            cancellationToken);

        var items = result.Items.Select(AuditLogDto.FromEntity).ToArray();
        var totalPages = result.TotalCount == 0
            ? 0
            : (int)Math.Ceiling(result.TotalCount / (double)result.PageSize);

        return new AuditLogPagedDto(items, result.TotalCount, result.PageNumber, result.PageSize, totalPages);
    }
}
