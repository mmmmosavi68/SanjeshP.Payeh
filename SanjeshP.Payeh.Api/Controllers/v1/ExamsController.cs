using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.Api.DTOs.Exams;
using SanjeshP.Payeh.Api.Mapping;
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
    private readonly IApiMapper _mapper;
    private readonly ICodesService _codes;

    public ExamsController(IExamService service, IApiMapper mapper, ICodesService codes)
    {
        _service = service;
        _mapper = mapper;
        _codes = codes;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<List<ExamDto>>), StatusCodes.Status200OK)]
    public async Task<ApiResult<List<ExamDto>>> GetAll([FromQuery] string? search, [FromQuery] Guid? examSessionId, [FromQuery] int? examTypeId, [FromQuery] int? examTypePrimaryCode, [FromQuery] int? examTypeSubCode, [FromQuery] int? examModeId, [FromQuery] int? examModePrimaryCode, [FromQuery] int? examModeSubCode, [FromQuery] bool? isActive, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        var result = await _service.GetPageAsync<ExamDto>(search, examSessionId, examTypeId, examTypePrimaryCode, examTypeSubCode, examModeId, examModePrimaryCode, examModeSubCode, isActive, page, pageSize, _mapper.ProjectToDto, cancellationToken);
        Response.ApplyPaging(result);

        return new ApiResult<List<ExamDto>>(true, ApiResultStatusCode.Success, result.Items.ToList());
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
            _mapper.ToDto(exam));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<ExamDto>), StatusCodes.Status200OK)]
    public async Task<ApiResult<ExamDto>> Create(
        [FromBody] CreateExamDto dto,
        CancellationToken cancellationToken)
    {
        var exam = _mapper.ToEntity(dto);
        await ResolveCodesAsync(exam, dto.ExamTypeId, dto.ExamTypePrimaryCode, dto.ExamTypeSubCode, dto.ExamModeId, dto.ExamModePrimaryCode, dto.ExamModeSubCode, cancellationToken);

        var created = await _service.CreateAsync(
            exam,
            CurrentUserId,
            cancellationToken);

        return new ApiResult<ExamDto>(
            true,
            ApiResultStatusCode.Success,
            _mapper.ToDto(created),
            ApiMessages.Exams.M001);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResult<ExamDto>), StatusCodes.Status200OK)]
    public async Task<ApiResult<ExamDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateExamDto dto,
        CancellationToken cancellationToken)
    {
        var exam = _mapper.ToEntity(dto);
        await ResolveCodesAsync(exam, dto.ExamTypeId, dto.ExamTypePrimaryCode, dto.ExamTypeSubCode, dto.ExamModeId, dto.ExamModePrimaryCode, dto.ExamModeSubCode, cancellationToken);

        var updated = await _service.UpdateAsync(
            id,
            exam,
            cancellationToken);

        return new ApiResult<ExamDto>(
            true,
            ApiResultStatusCode.Success,
            _mapper.ToDto(updated),
            ApiMessages.Exams.M002);
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
            ApiMessages.Exams.M003);
    }

    private async Task ResolveCodesAsync(Exam exam, int examTypeId, int? examTypePrimaryCode, int? examTypeSubCode, int examModeId, int? examModePrimaryCode, int? examModeSubCode, CancellationToken cancellationToken)
    {
        exam.ExamTypeId = await _codes.ResolveIdAsync(examTypeId, examTypePrimaryCode, examTypeSubCode, CodeGroupTitles.ExamType, ApiMessages.Lookup.ExamTypeInvalid, cancellationToken);
        exam.ExamModeId = await _codes.ResolveIdAsync(examModeId, examModePrimaryCode, examModeSubCode, CodeGroupTitles.ExamMode, ApiMessages.Lookup.ExamModeInvalid, cancellationToken);
    }
}
