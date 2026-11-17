using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.Api.DTOs.Exams;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Entities.Exam;
using SanjeshP.Payeh.Services.Exams;
using SanjeshP.Payeh.WebFramework.Api;

namespace SanjeshP.Payeh.Api.Controllers.V1;

[ApiVersion("1")]
[Authorize]
public sealed class ExamsController : BaseController
{
    private readonly IExamService _service;

    public ExamsController(IExamService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<List<ExamDto>>), StatusCodes.Status200OK)]
    public async Task<ApiResult<List<ExamDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] Guid? examSessionId,
        [FromQuery] int? examTypeId,
        [FromQuery] int? examModeId,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var exams = await _service.GetAllAsync(
            search,
            examSessionId,
            examTypeId,
            examModeId,
            isActive,
            cancellationToken);

        return new ApiResult<List<ExamDto>>(
            true,
            ApiResultStatusCode.Success,
            exams.Select(MapToDto).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResult<ExamDto>), StatusCodes.Status200OK)]
    public async Task<ApiResult<ExamDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var exam = await _service.GetByIdAsync(id, cancellationToken);

        return new ApiResult<ExamDto>(
            true,
            ApiResultStatusCode.Success,
            MapToDto(exam));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<ExamDto>), StatusCodes.Status200OK)]
    public async Task<ApiResult<ExamDto>> Create(
        [FromBody] CreateExamDto dto,
        CancellationToken cancellationToken)
    {
        var exam = new Exam
        {
            Title = dto.Title,
            ExamSessionId = dto.ExamSessionId,
            ExamTypeId = dto.ExamTypeId,
            ExamModeId = dto.ExamModeId,
            RegistrationStartDate = dto.RegistrationStartDate,
            RegistrationEndDate = dto.RegistrationEndDate,
            EditStartDate = dto.EditStartDate,
            EditEndDate = dto.EditEndDate,
            Description = dto.Description
        };

        var created = await _service.CreateAsync(
            exam,
            CurrentUserId,
            cancellationToken);

        return new ApiResult<ExamDto>(
            true,
            ApiResultStatusCode.Success,
            MapToDto(created),
            "آزمون با موفقیت ثبت شد.");
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResult<ExamDto>), StatusCodes.Status200OK)]
    public async Task<ApiResult<ExamDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateExamDto dto,
        CancellationToken cancellationToken)
    {
        var exam = new Exam
        {
            Title = dto.Title,
            ExamSessionId = dto.ExamSessionId,
            ExamTypeId = dto.ExamTypeId,
            ExamModeId = dto.ExamModeId,
            RegistrationStartDate = dto.RegistrationStartDate,
            RegistrationEndDate = dto.RegistrationEndDate,
            EditStartDate = dto.EditStartDate,
            EditEndDate = dto.EditEndDate,
            Description = dto.Description,
            IsActive = dto.IsActive
        };

        var updated = await _service.UpdateAsync(
            id,
            exam,
            cancellationToken);

        return new ApiResult<ExamDto>(
            true,
            ApiResultStatusCode.Success,
            MapToDto(updated),
            "اطلاعات آزمون با موفقیت ویرایش شد.");
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    public async Task<ApiResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _service.SoftDeleteAsync(id, cancellationToken);

        return new ApiResult(
            true,
            ApiResultStatusCode.Success,
            "آزمون با موفقیت حذف شد.");
    }

    private static ExamDto MapToDto(Exam exam)
    {
        return new ExamDto
        {
            Id = exam.Id,
            Title = exam.Title,

            ExamSessionId = exam.ExamSessionId,
            ExamSessionTitle = exam.ExamSession?.Title,
            ExamDate = exam.ExamSession?.ExamDate ?? default,

            ExamTypeId = exam.ExamTypeId,
            ExamTypeTitle = exam.ExamType?.Title,

            ExamModeId = exam.ExamModeId,
            ExamModeTitle = exam.ExamMode?.Title,

            RegistrationStartDate = exam.RegistrationStartDate,
            RegistrationEndDate = exam.RegistrationEndDate,
            EditStartDate = exam.EditStartDate,
            EditEndDate = exam.EditEndDate,

            Description = exam.Description,
            IsActive = exam.IsActive,
            CreatedDate = exam.CreatedDate,
            UpdatedDate = exam.UpdatedDate
        };
    }
}