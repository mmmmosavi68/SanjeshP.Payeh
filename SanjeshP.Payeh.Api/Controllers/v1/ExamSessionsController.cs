
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.Api.DTOs.ExamSessions;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Entities.Exam;
using SanjeshP.Payeh.Services.Exams;
using SanjeshP.Payeh.WebFramework.Api;

namespace SanjeshP.Payeh.Api.Controllers.V1;

[ApiVersion("1")]
[Authorize]
public sealed class ExamSessionsController : BaseController
{
    private readonly IExamSessionService _service;

    public ExamSessionsController(IExamSessionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ApiResult<List<ExamSessionDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var sessions = await _service.GetAllAsync(
            search,
            isActive,
            cancellationToken);

        return new ApiResult<List<ExamSessionDto>>(
            true,
            ApiResultStatusCode.Success,
            sessions.Select(MapToDto).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ApiResult<ExamSessionDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var session = await _service.GetByIdAsync(id, cancellationToken);

        return new ApiResult<ExamSessionDto>(
            true,
            ApiResultStatusCode.Success,
            MapToDto(session));
    }

    [HttpPost]
    public async Task<ApiResult<ExamSessionDto>> Create(
        [FromBody] CreateExamSessionDto dto,
        CancellationToken cancellationToken)
    {
        var session = new ExamSession
        {
            Title = dto.Title,
            ExamDate = dto.ExamDate,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime
        };

        var created = await _service.CreateAsync(
            session,
            CurrentUserId,
            cancellationToken);

        return new ApiResult<ExamSessionDto>(
            true,
            ApiResultStatusCode.Success,
            MapToDto(created),
            "نوبت آزمون با موفقیت ثبت شد.");
    }

    [HttpPut("{id:guid}")]
    public async Task<ApiResult<ExamSessionDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateExamSessionDto dto,
        CancellationToken cancellationToken)
    {
        var session = new ExamSession
        {
            Title = dto.Title,
            ExamDate = dto.ExamDate,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            IsActive = dto.IsActive
        };

        var updated = await _service.UpdateAsync(
            id,
            session,
            cancellationToken);

        return new ApiResult<ExamSessionDto>(
            true,
            ApiResultStatusCode.Success,
            MapToDto(updated),
            "نوبت آزمون با موفقیت ویرایش شد.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ApiResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);

        return new ApiResult(
            true,
            ApiResultStatusCode.Success,
            "نوبت آزمون با موفقیت حذف شد.");
    }

    private static ExamSessionDto MapToDto(ExamSession session)
    {
        return new ExamSessionDto
        {
            Id = session.Id,
            Title = session.Title,
            ExamDate = session.ExamDate,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            IsActive = session.IsActive,
            CreatedDate = session.CreatedDate,
            UpdatedDate = session.UpdatedDate
        };
    }
}
