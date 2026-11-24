
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.Api.DTOs.Universities;
using SanjeshP.Payeh.Api.Mapping;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Entities.University;
using SanjeshP.Payeh.Services.Universities;
using SanjeshP.Payeh.WebFramework.Api;

namespace SanjeshP.Payeh.Api.Controllers.V1;

[ApiVersion("1")]
[Authorize]
public sealed class UniversitiesController : BaseController
{
    private readonly IUniversityService _service;
    private readonly IApiMapper _mapper;
    private readonly ICodesService _codes;

    public UniversitiesController(IUniversityService service, IApiMapper mapper, ICodesService codes)
    {
        _service = service;
        _mapper = mapper;
        _codes = codes;
    }

    #region Read

    /// <summary>
    /// دریافت فهرست دانشگاه‌ها همراه با فیلتر
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<List<UniversityDto>>), StatusCodes.Status200OK)]
    public async Task<ApiResult<List<UniversityDto>>> GetAll([FromQuery] string? search, [FromQuery] int? universityTypeId, [FromQuery] int? universityTypePrimaryCode, [FromQuery] int? universityTypeSubCode, [FromQuery] int? region, [FromQuery] int? parentUniversityId, [FromQuery] bool? isActive, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        var result = await _service.GetPageAsync<UniversityDto>(search, universityTypeId, universityTypePrimaryCode, universityTypeSubCode, region, parentUniversityId, isActive, page, pageSize, _mapper.ProjectToDto, cancellationToken);
        Response.ApplyPaging(result);

        return new ApiResult<List<UniversityDto>>(true, ApiResultStatusCode.Success, result.Items.ToList());
    }

    /// <summary>
    /// دریافت اطلاعات یک دانشگاه
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResult<UniversityDto>), StatusCodes.Status200OK)]
    public async Task<ApiResult<UniversityDto>> GetById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var university = await _service.GetByIdAsync(
            id,
            cancellationToken);

        return new ApiResult<UniversityDto>(
            true,
            ApiResultStatusCode.Success,
            _mapper.ToDto(university));
    }

    #endregion

    #region Create

    /// <summary>
    /// ایجاد دانشگاه جدید
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<UniversityDto>), StatusCodes.Status200OK)]
    public async Task<ApiResult<UniversityDto>> Create(
        [FromBody] CreateUniversityDto dto,
        CancellationToken cancellationToken)
    {
        var university = _mapper.ToEntity(dto);
        university.UniversityTypeId = await _codes.ResolveIdAsync(dto.UniversityTypeId, dto.UniversityTypePrimaryCode, dto.UniversityTypeSubCode, null, ApiMessages.Lookup.UniversityTypeInvalid, cancellationToken);

        var created = await _service.CreateAsync(
            university,
            CurrentUserId,
            cancellationToken);

        return new ApiResult<UniversityDto>(
            true,
            ApiResultStatusCode.Success,
            _mapper.ToDto(created),
            "دانشگاه با موفقیت ثبت شد.");
    }

    #endregion

    #region Update

    /// <summary>
    /// ویرایش اطلاعات دانشگاه
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResult<UniversityDto>), StatusCodes.Status200OK)]
    public async Task<ApiResult<UniversityDto>> Update(
        [FromRoute] int id,
        [FromBody] UpdateUniversityDto dto,
        CancellationToken cancellationToken)
    {
        var university = _mapper.ToEntity(dto);
        university.UniversityTypeId = await _codes.ResolveIdAsync(dto.UniversityTypeId, dto.UniversityTypePrimaryCode, dto.UniversityTypeSubCode, null, ApiMessages.Lookup.UniversityTypeInvalid, cancellationToken);

        var updated = await _service.UpdateAsync(
            id,
            university,
            cancellationToken);

        return new ApiResult<UniversityDto>(
            true,
            ApiResultStatusCode.Success,
            _mapper.ToDto(updated),
            "اطلاعات دانشگاه با موفقیت ویرایش شد.");
    }

    #endregion

    #region Delete

    /// <summary>
    /// حذف نرم دانشگاه
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    public async Task<ApiResult> Delete(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        await _service.SoftDeleteAsync(
            id,
            cancellationToken);

        return new ApiResult(
            true,
            ApiResultStatusCode.Success,
            "دانشگاه با موفقیت حذف شد.");
    }

    #endregion

    #region Mapping


    #endregion
}
