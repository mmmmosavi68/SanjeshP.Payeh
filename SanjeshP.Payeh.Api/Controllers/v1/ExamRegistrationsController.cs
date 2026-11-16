using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.Api.DTOs.ExamRegistrations;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Entities.Exam;
using SanjeshP.Payeh.Services.Exams;
using SanjeshP.Payeh.WebFramework.Api;

namespace SanjeshP.Payeh.Api.Controllers.V1;

[ApiVersion("1")]
[Authorize]
public sealed class ExamRegistrationsController : BaseController
{
    private readonly IExamRegistrationService _service;

    public ExamRegistrationsController(IExamRegistrationService service)
    {
        _service = service;
    }

    /// <summary>
    /// ثبت اطلاعات داوطلب توسط کارشناس واردشده به سامانه
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<ExamRegistrationDto>), StatusCodes.Status200OK)]
    public async Task<ApiResult<ExamRegistrationDto>> Create(
        [FromBody] CreateExamRegistrationDto dto,
        CancellationToken cancellationToken)
    {
        var registration = new ExamRegistration
        {
            ExamId = dto.ExamId,
            UniversityId = dto.UniversityId,
            ExamVenueUniversityId = dto.ExamVenueUniversityId,
            UniversityExamCapacityId = dto.UniversityExamCapacityId,
            UniversityExamGuestCapacityId = dto.UniversityExamGuestCapacityId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            FatherName = dto.FatherName,
            NationalCode = dto.NationalCode,
            GenderId = dto.GenderId,
            BirthCertificateNumber = dto.BirthCertificateNumber,
            BirthDate = dto.BirthDate,
            StudentNumber = dto.StudentNumber,
            EntryYear = dto.EntryYear,
            PhoneNumber = dto.PhoneNumber,
            EmergencyPhoneNumber = dto.EmergencyPhoneNumber
        };

        var customValues = dto.CustomFieldValues
            .Select(x => new ExamRegistrationCustomFieldValue
            {
                ExamCustomFieldId = x.ExamCustomFieldId,
                Value = x.Value
            })
            .ToList();

        var created = await _service.CreateAsync(
            registration,
            customValues,
            CurrentUserId,
            cancellationToken);

        return new ApiResult<ExamRegistrationDto>(
            true,
            ApiResultStatusCode.Success,
            MapToDto(created),
            "اطلاعات داوطلب با موفقیت ثبت شد.");
    }

    /// <summary>
    /// دریافت ثبت‌نام‌های یک آزمون
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<List<ExamRegistrationDto>>> GetByExam(
        [FromQuery] Guid examId,
        CancellationToken cancellationToken)
    {
        var registrations = await _service.GetByExamAsync(
            examId,
            cancellationToken);

        return new ApiResult<List<ExamRegistrationDto>>(
            true,
            ApiResultStatusCode.Success,
            registrations.Select(MapToDto).ToList());
    }

    /// <summary>
    /// دریافت جزئیات یک ثبت‌نام
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ApiResult<ExamRegistrationDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var registration = await _service.GetByIdAsync(
            id,
            cancellationToken);

        return new ApiResult<ExamRegistrationDto>(
            true,
            ApiResultStatusCode.Success,
            MapToDto(registration));
    }

    /// <summary>
    /// حذف نرم ثبت‌نام
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ApiResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _service.SoftDeleteAsync(id, cancellationToken);

        return new ApiResult(
            true,
            ApiResultStatusCode.Success,
            "ثبت‌نام با موفقیت حذف شد.");
    }

    private static ExamRegistrationDto MapToDto(ExamRegistration item)
    {
        return new ExamRegistrationDto
        {
            Id = item.Id,
            ExamId = item.ExamId,
            UniversityId = item.UniversityId,
            UniversityTitle = item.University?.Title,
            ExamVenueUniversityId = item.ExamVenueUniversityId,
            ExamVenueUniversityTitle = item.ExamVenueUniversity?.Title,
            UniversityExamCapacityId = item.UniversityExamCapacityId,
            UniversityExamGuestCapacityId = item.UniversityExamGuestCapacityId,
            FirstName = item.FirstName,
            LastName = item.LastName,
            FatherName = item.FatherName,
            NationalCode = item.NationalCode,
            GenderId = item.GenderId,
            BirthCertificateNumber = item.BirthCertificateNumber,
            BirthDate = item.BirthDate,
            StudentNumber = item.StudentNumber,
            EntryYear = item.EntryYear,
            PhoneNumber = item.PhoneNumber,
            EmergencyPhoneNumber = item.EmergencyPhoneNumber,
            CreatedDate = item.CreatedDate,
            CustomFieldValues = item.CustomFieldValues?
                .Select(x => new ExamRegistrationCustomFieldValueResultDto
                {
                    ExamCustomFieldId = x.ExamCustomFieldId,
                    FieldLabel = x.ExamCustomField?.Label,
                    Value = x.Value
                })
                .ToList() ?? []
        };
    }
}
