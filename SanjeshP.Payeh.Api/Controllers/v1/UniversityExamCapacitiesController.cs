
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.Api.DTOs.ExamCapacities;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Entities.Exam;
using SanjeshP.Payeh.Services.Exams;
using SanjeshP.Payeh.WebFramework.Api;

namespace SanjeshP.Payeh.Api.Controllers.V1;

[ApiVersion("1")]
public sealed class UniversityExamCapacitiesController : BaseController
{
    private readonly IUniversityExamCapacityService _service;

    public UniversityExamCapacitiesController(
        IUniversityExamCapacityService service)
    {
        _service = service;
    }

    #region Read

    /// <summary>
    /// دریافت آزمون‌های فعال و ظرفیت‌های ثبت‌شده دانشگاه
    /// </summary>
    [HttpGet("universities/{universityId:int}/active-exams")]
    public async Task<ApiResult<List<UniversityExamCapacityExamDto>>> GetActiveExams(
        [FromRoute] int universityId,
        CancellationToken cancellationToken)
    {
        var exams = await _service.GetActiveExamsForUniversityAsync(
            CurrentUserId,
            universityId,
            cancellationToken);

        var result = await BuildExamDtosAsync(
            exams,
            universityId,
            cancellationToken);

        return new ApiResult<List<UniversityExamCapacityExamDto>>(
            true,
            ApiResultStatusCode.Success,
            result);
    }

    /// <summary>
    /// دریافت آزمون‌هایی که برای دانشگاه ظرفیت آن‌ها تعیین شده است
    /// </summary>
    [HttpGet("universities/{universityId:int}/defined-capacities")]
    public async Task<ApiResult<List<UniversityExamCapacityExamDto>>> GetDefinedCapacities(
        [FromRoute] int universityId,
        CancellationToken cancellationToken)
    {
        var exams = await _service.GetDefinedExamsForUniversityAsync(
            CurrentUserId,
            universityId,
            cancellationToken);

        var result = await BuildExamDtosAsync(
            exams,
            universityId,
            cancellationToken);

        return new ApiResult<List<UniversityExamCapacityExamDto>>(
            true,
            ApiResultStatusCode.Success,
            result);
    }

    /// <summary>
    /// دریافت دانشگاه‌های مجاز کاربر جاری
    /// </summary>
    [HttpGet("my-universities")]
    public async Task<ApiResult<List<UniversityExamCapacityUniversityDto>>> GetMyUniversities(
        CancellationToken cancellationToken)
    {
        var universities = await _service.GetAccessibleUniversitiesAsync(
            CurrentUserId,
            cancellationToken);

        var result = universities.Select(x =>
            new UniversityExamCapacityUniversityDto
            {
                Id = x.Id,
                Title = x.Title,

                UniversityTypeId = x.UniversityTypeId,
                UniversityTypePrimaryCode = x.UniversityType?.PrimaryCode,
                UniversityTypeSubCode = x.UniversityType?.SubCode,
                UniversityTypeTitle = x.UniversityType?.Title,

                Region = x.Region,
                CapacityApproved = x.CapacityApproved,

                ParentUniversityId = x.ParentUniversityId,
                ParentUniversityTitle = x.ParentUniversity?.Title
            }).ToList();

        return new ApiResult<List<UniversityExamCapacityUniversityDto>>(
            true,
            ApiResultStatusCode.Success,
            result);
    }

    /// <summary>
    /// دریافت ظرفیت دانشگاه‌های مهمان برای یک آزمون
    /// </summary>
    [HttpGet("universities/{universityId:int}/exams/{examId:guid}/guest-capacities")]
    public async Task<ApiResult<List<UniversityExamGuestCapacityDto>>> GetGuestCapacities(
        [FromRoute] int universityId,
        [FromRoute] Guid examId,
        CancellationToken cancellationToken)
    {
        var capacities = await _service.GetGuestCapacitiesAsync(
            CurrentUserId,
            universityId,
            examId,
            cancellationToken);

        var registeredCounts = await _service.GetGuestRegistrationCountsAsync(
            capacities.Select(x => x.Id).ToList(),
            cancellationToken);

        var result = capacities
            .Select(x =>
            {
                var dto = MapGuestCapacityToDto(x);
                dto.RegisteredCount = registeredCounts.GetValueOrDefault(x.Id);
                return dto;
            })
            .ToList();

        return new ApiResult<List<UniversityExamGuestCapacityDto>>(
            true,
            ApiResultStatusCode.Success,
            result);
    }

    #endregion

    #region Save

    /// <summary>
    /// ثبت یا ویرایش ظرفیت دانشگاه میزبان برای یک آزمون
    /// </summary>
    [HttpPut("universities/{universityId:int}/exams/{examId:guid}/capacity")]
    public async Task<ApiResult<UniversityExamCapacityDto>> Save(
        [FromRoute] int universityId,
        [FromRoute] Guid examId,
        [FromBody] SaveUniversityExamCapacityDto dto,
        CancellationToken cancellationToken)
    {
        var capacity = await _service.SaveAsync(
            universityId,
            examId,
            dto.AllocatedCapacity,
            dto.OwnCapacity,
            CurrentUserId,
            cancellationToken);

        return new ApiResult<UniversityExamCapacityDto>(
            true,
            ApiResultStatusCode.Success,
            MapToDto(capacity),
            ApiMessages.UniversityExamCapacities.M001);
    }

    /// <summary>
    /// ثبت یا ویرایش ظرفیت یک دانشگاه مهمان
    /// </summary>
    [HttpPut("universities/{universityId:int}/exams/{examId:guid}/guest-capacities/{guestUniversityId:int}")]
    public async Task<ApiResult<UniversityExamGuestCapacityDto>> SaveGuestCapacity(
        [FromRoute] int universityId,
        [FromRoute] Guid examId,
        [FromRoute] int guestUniversityId,
        [FromBody] SaveUniversityExamGuestCapacityDto dto,
        CancellationToken cancellationToken)
    {
        var capacity = await _service.SaveGuestCapacityAsync(
            universityId,
            examId,
            guestUniversityId,
            dto.Capacity,
            CurrentUserId,
            cancellationToken);

        var guestDto = MapGuestCapacityToDto(capacity);

        var registeredCounts = await _service.GetGuestRegistrationCountsAsync(
            new[] { capacity.Id },
            cancellationToken);

        guestDto.RegisteredCount = registeredCounts.GetValueOrDefault(capacity.Id);

        return new ApiResult<UniversityExamGuestCapacityDto>(
            true,
            ApiResultStatusCode.Success,
            guestDto,
            ApiMessages.UniversityExamCapacities.M002);
    }

    #endregion

    #region Mapping

    private async Task<List<UniversityExamCapacityExamDto>> BuildExamDtosAsync(
        List<Exam> exams,
        int universityId,
        CancellationToken cancellationToken)
    {
        var capacityIds = exams
            .Select(exam => exam.UniversityExamCapacities
                .FirstOrDefault(x => x.UniversityId == universityId)?.Id)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var usages = await _service.GetCapacityUsageAsync(
            universityId,
            capacityIds,
            cancellationToken);

        return exams.Select(exam =>
        {
            var capacity = exam.UniversityExamCapacities
                .FirstOrDefault(x => x.UniversityId == universityId);

            var usage = capacity is not null &&
                        usages.TryGetValue(capacity.Id, out var found)
                ? found
                : null;

            return new UniversityExamCapacityExamDto
            {
                ExamId = exam.Id,
                ExamTitle = exam.Title,

                ExamSessionId = exam.ExamSessionId,
                ExamSessionTitle = exam.ExamSession?.Title,
                ExamDate = exam.ExamSession?.ExamDate ?? default,

                ExamModeId = exam.ExamModeId,
                ExamModePrimaryCode = exam.ExamMode?.PrimaryCode,
                ExamModeSubCode = exam.ExamMode?.SubCode,
                ExamModeTitle = exam.ExamMode?.Title,

                IsCapacityDefined = capacity is not null,
                CapacityId = capacity?.Id,

                AllocatedCapacity = capacity?.AllocatedCapacity,
                OwnCapacity = capacity?.OwnCapacity,

                CapacityIsActive = capacity?.IsActive,

                RegisteredCount = usage?.RegisteredCount ?? 0,
                OwnRegisteredCount = usage?.OwnRegisteredCount ?? 0,
                GuestRegisteredCount = usage?.GuestRegisteredCount ?? 0,
                TotalGuestCapacity = usage?.TotalGuestCapacity ?? 0
            };
        }).ToList();
    }

    private static UniversityExamCapacityDto MapToDto(
        UniversityExamCapacity capacity)
    {
        return new UniversityExamCapacityDto
        {
            Id = capacity.Id,

            UniversityId = capacity.UniversityId,

            ExamId = capacity.ExamId,
            ExamTitle = capacity.Exam?.Title,

            ExamSessionId = capacity.Exam?.ExamSessionId ?? Guid.Empty,
            ExamSessionTitle = capacity.Exam?.ExamSession?.Title,

            AllocatedCapacity = capacity.AllocatedCapacity ?? 0,
            OwnCapacity = capacity.OwnCapacity ?? 0,

            IsActive = capacity.IsActive,

            CreatedDate = capacity.CreatedDate,
            UpdatedDate = capacity.UpdatedDate
        };
    }

    private static UniversityExamGuestCapacityDto MapGuestCapacityToDto(
        UniversityExamGuestCapacity capacity)
    {
        return new UniversityExamGuestCapacityDto
        {
            Id = capacity.Id,
            UniversityExamCapacityId = capacity.UniversityExamCapacityId,
            GuestUniversityId = capacity.GuestUniversityId,
            GuestUniversityTitle = capacity.GuestUniversity?.Title
                ?? string.Empty,
            Capacity = capacity.Capacity,
            IsActive = capacity.IsActive,
            CreatedDate = capacity.CreatedDate,
            UpdatedDate = capacity.UpdatedDate
        };
    }

    #endregion
}
