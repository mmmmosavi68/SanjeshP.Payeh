
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.Api.DTOs.Universities;
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

    public UniversitiesController(IUniversityService service)
    {
        _service = service;
    }

    #region Read

    /// <summary>
    /// دریافت فهرست دانشگاه‌ها همراه با فیلتر
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<List<UniversityDto>>), StatusCodes.Status200OK)]
    public async Task<ApiResult<List<UniversityDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? universityTypeId,
        [FromQuery] int? region,
        [FromQuery] int? parentUniversityId,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var universities = await _service.GetAllAsync(
            search,
            universityTypeId,
            region,
            parentUniversityId,
            isActive,
            cancellationToken);

        return new ApiResult<List<UniversityDto>>(
            true,
            ApiResultStatusCode.Success,
            universities.Select(MapToDto).ToList());
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
            MapToDto(university));
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
        var university = new University
        {
            Title = dto.Title,
            UniversityTypeId = dto.UniversityTypeId,
            Region = dto.Region,
            CapacityApproved = dto.CapacityApproved,
            ParentUniversityId = dto.ParentUniversityId
        };

        var created = await _service.CreateAsync(
            university,
            CurrentUserId,
            cancellationToken);

        return new ApiResult<UniversityDto>(
            true,
            ApiResultStatusCode.Success,
            MapToDto(created),
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
        var university = new University
        {
            Title = dto.Title,
            UniversityTypeId = dto.UniversityTypeId,
            Region = dto.Region,
            CapacityApproved = dto.CapacityApproved,
            ParentUniversityId = dto.ParentUniversityId,
            IsActive = dto.IsActive
        };

        var updated = await _service.UpdateAsync(
            id,
            university,
            cancellationToken);

        return new ApiResult<UniversityDto>(
            true,
            ApiResultStatusCode.Success,
            MapToDto(updated),
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

    private static UniversityDto MapToDto(University university)
    {
        return new UniversityDto
        {
            Id = university.Id,
            Title = university.Title,
            UniversityTypeId = university.UniversityTypeId,
            UniversityTypeTitle = university.UniversityType?.Title,
            Region = university.Region,
            CapacityApproved = university.CapacityApproved,
            ParentUniversityId = university.ParentUniversityId,
            ParentUniversityTitle = university.ParentUniversity?.Title,
            IsActive = university.IsActive,
            CreatedDate = university.CreatedDate,
            UpdatedDate = university.UpdatedDate
        };
    }

    #endregion
}
