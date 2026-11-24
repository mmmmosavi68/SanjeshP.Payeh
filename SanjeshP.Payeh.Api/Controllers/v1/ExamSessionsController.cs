
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.Api.DTOs.ExamSessions;
using SanjeshP.Payeh.Api.Mapping;
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
    private readonly IApiMapper _mapper;

    public ExamSessionsController(IExamSessionService service, IApiMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ApiResult<List<ExamSessionDto>>> GetAll([FromQuery] string? search, [FromQuery] bool? isActive, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        var result = await _service.GetPageAsync<ExamSessionDto>(search, isActive, page, pageSize, _mapper.ProjectToDto, cancellationToken);
        Response.ApplyPaging(result);

        return new ApiResult<List<ExamSessionDto>>(true, ApiResultStatusCode.Success, result.Items.ToList());
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
            _mapper.ToDto(session));
    }

    [HttpPost]
    public async Task<ApiResult<ExamSessionDto>> Create(
        [FromBody] CreateExamSessionDto dto,
        CancellationToken cancellationToken)
    {
        var session = _mapper.ToEntity(dto);

        var created = await _service.CreateAsync(
            session,
            CurrentUserId,
            cancellationToken);

        return new ApiResult<ExamSessionDto>(
            true,
            ApiResultStatusCode.Success,
            _mapper.ToDto(created),
            ApiMessages.ExamSessions.M001);
    }

    [HttpPut("{id:guid}")]
    public async Task<ApiResult<ExamSessionDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateExamSessionDto dto,
        CancellationToken cancellationToken)
    {
        var session = _mapper.ToEntity(dto);

        var updated = await _service.UpdateAsync(
            id,
            session,
            cancellationToken);

        return new ApiResult<ExamSessionDto>(
            true,
            ApiResultStatusCode.Success,
            _mapper.ToDto(updated),
            ApiMessages.ExamSessions.M002);
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
            ApiMessages.ExamSessions.M003);
    }

}
