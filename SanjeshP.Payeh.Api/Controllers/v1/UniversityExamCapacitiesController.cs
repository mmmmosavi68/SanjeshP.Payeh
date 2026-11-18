
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

        var result = exams.Select(exam =>
        {
            var capacity = exam.UniversityExamCapacities
                .FirstOrDefault(x => x.UniversityId == universityId);

            return new UniversityExamCapacityExamDto
            {
                ExamId = exam.Id,
                ExamTitle = exam.Title,

                ExamSessionId = exam.ExamSessionId,
                ExamSessionTitle = exam.ExamSession?.Title,
                ExamDate = exam.ExamSession?.ExamDate ?? default,

                ExamModeId = exam.ExamModeId,
                ExamModeTitle = exam.ExamMode?.Title,

                IsCapacityDefined = capacity is not null,
                CapacityId = capacity?.Id,

                AllocatedCapacity = capacity?.AllocatedCapacity,
                OwnCapacity = capacity?.OwnCapacity,

                CapacityIsActive = capacity?.IsActive
            };
        }).ToList();

        return new ApiResult<List<UniversityExamCapacityExamDto>>(
            true,
            ApiResultStatusCode.Success,
            result);
    }

    /// <summary>
    /// ثبت یا ویرایش ظرفیت دانشگاه برای یک آزمون
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
            "ظرفیت دانشگاه برای آزمون با موفقیت ثبت شد.");
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


    /// <summary>
    /// دریافت دانشگاه‌های مجاز کاربر جاری برای مدیریت ظرفیت آزمون‌ها
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
}
