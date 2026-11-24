using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.Api.DTOs.ExamRegistrations;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Entities.Exam;
using SanjeshP.Payeh.Entities.University;
using SanjeshP.Payeh.Services.Exams;
using SanjeshP.Payeh.WebFramework.Api;

namespace SanjeshP.Payeh.Api.Controllers.V1;

[ApiVersion("1")]
[Authorize]
public sealed class ExamRegistrationsController : BaseController
{
    private readonly IExamRegistrationService _service;

    private readonly ICodesService _codes;

    public ExamRegistrationsController(IExamRegistrationService service, ICodesService codes)
    {
        _service = service;
        _codes = codes;
    }

    private async Task<int?> ResolveGenderIdAsync(int? genderId, int? primaryCode, int? subCode, CancellationToken cancellationToken)
    {
        if (!genderId.HasValue && !primaryCode.HasValue && !subCode.HasValue)
            return null;

        return await _codes.ResolveIdAsync(genderId, primaryCode, subCode, null, ApiMessages.Lookup.GenderInvalid, cancellationToken);
    }

    /// <summary>
    /// دریافت دانشگاه‌های مجاز کاربر واردشده
    /// </summary>
    [HttpGet("universities")]
    [ProducesResponseType(typeof(ApiResult<List<ExamRegistrationUniversityDto>>), StatusCodes.Status200OK)]
    public async Task<ApiResult<List<ExamRegistrationUniversityDto>>> GetAccessibleUniversities(CancellationToken cancellationToken)
    {
        var universities = await _service.GetAccessibleUniversitiesAsync(
            CurrentUserId,
            cancellationToken);

        var result = universities
            .Select(MapToUniversityDto)
            .ToList();

        return new ApiResult<List<ExamRegistrationUniversityDto>>(
            true,
            ApiResultStatusCode.Success,
            result);
    }

    /// <summary>
    /// دریافت آزمون‌های قابل ثبت‌نام برای دانشگاه انتخاب‌شده
    /// </summary>
    [HttpGet("exams")]
    [ProducesResponseType(typeof(ApiResult<List<ExamRegistrationExamDto>>), StatusCodes.Status200OK)]
    public async Task<ApiResult<List<ExamRegistrationExamDto>>> GetAvailableExams([FromQuery] int universityId, CancellationToken cancellationToken)
    {
        var exams = await _service.GetAvailableExamsAsync(
            universityId,
            CurrentUserId,
            cancellationToken);

        var result = exams
            .Select(MapToExamDto)
            .ToList();

        return new ApiResult<List<ExamRegistrationExamDto>>(
            true,
            ApiResultStatusCode.Success,
            result);
    }




    /// <summary>
    /// دریافت ظرفیت‌های قابل انتخاب و فیلدهای اختصاصی فرم ثبت‌نام
    /// </summary>
    [HttpGet("options")]
    [ProducesResponseType(typeof(ApiResult<ExamRegistrationOptionsDto>), StatusCodes.Status200OK)]
    public async Task<ApiResult<ExamRegistrationOptionsDto>> GetRegistrationOptions([FromQuery] Guid examId, [FromQuery] int universityId, CancellationToken cancellationToken)
    {
        var options = await _service.GetRegistrationOptionsAsync(
            examId,
            universityId,
            CurrentUserId,
            cancellationToken);

        var result = new ExamRegistrationOptionsDto
        {
            Capacities = options.Capacities.Select(x =>
            {
                var isOwnUniversity = x.UniversityId == universityId;
                var guestCapacity = x.GuestCapacities.FirstOrDefault();

                return new ExamRegistrationCapacityOptionDto
                {
                    UniversityExamCapacityId = x.Id,
                    ExamVenueUniversityId = x.UniversityId,
                    ExamVenueUniversityTitle = x.University.Title,
                    AllocatedCapacity = x.AllocatedCapacity,
                    OwnCapacity = x.OwnCapacity,
                    IsOwnUniversity = isOwnUniversity,
                    UniversityExamGuestCapacityId = isOwnUniversity
                        ? null
                        : guestCapacity?.Id,
                    GuestCapacity = isOwnUniversity
                        ? null
                        : guestCapacity?.Capacity
                };
            }).ToList(),

            CustomFields = options.CustomFields.Select(x =>
                new ExamRegistrationCustomFieldDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Label = x.Label,
                    FieldTypeId = x.FieldTypeId,
                    FieldTypePrimaryCode = x.FieldType?.PrimaryCode,
                    FieldTypeSubCode = x.FieldType?.SubCode,
                    IsRequired = x.IsRequired,
                    OptionsJson = x.OptionsJson,
                    ValidationJson = x.ValidationJson,
                    DisplayOrder = x.DisplayOrder
                }).ToList()
        };

        return new ApiResult<ExamRegistrationOptionsDto>(
            true,
            ApiResultStatusCode.Success,
            result);
    }
    /// <summary>
    /// ثبت اطلاعات داوطلب توسط کارشناس واردشده به سامانه
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<ExamRegistrationDto>), StatusCodes.Status200OK)]
    public async Task<ApiResult<ExamRegistrationDto>> Create([FromBody] CreateExamRegistrationDto dto, CancellationToken cancellationToken)
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
            GenderId = await ResolveGenderIdAsync(dto.GenderId, dto.GenderPrimaryCode, dto.GenderSubCode, cancellationToken),
            BirthCertificateNumber = dto.BirthCertificateNumber,
            BirthDate = dto.BirthDate,
            StudentNumber = dto.StudentNumber,
            EntryYear = dto.EntryYear,
            PhoneNumber = dto.PhoneNumber,
            EmergencyPhoneNumber = dto.EmergencyPhoneNumber
        };

        var customValues = dto.CustomFieldValues?
            .Select(x => new ExamRegistrationCustomFieldValue
            {
                ExamCustomFieldId = x.ExamCustomFieldId,
                Value = x.Value
            })
            .ToList() ?? [];

        var created = await _service.CreateAsync(
            registration,
            customValues,
            CurrentUserId,
            cancellationToken);

        return new ApiResult<ExamRegistrationDto>(
            true,
            ApiResultStatusCode.Success,
            MapToDto(created),
            ApiMessages.ExamRegistrations.M001);
    }

    /// <summary>
    /// دریافت ثبت‌نام‌های یک آزمون در دانشگاه انتخاب‌شده
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<List<ExamRegistrationDto>>), StatusCodes.Status200OK)]
    public async Task<ApiResult<List<ExamRegistrationDto>>> GetByExam([FromQuery] Guid examId, [FromQuery] int universityId, CancellationToken cancellationToken)
    {
        var registrations = await _service.GetByExamAsync(
            examId,
            universityId,
            CurrentUserId,
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
    [ProducesResponseType(typeof(ApiResult<ExamRegistrationDto>), StatusCodes.Status200OK)]
    public async Task<ApiResult<ExamRegistrationDto>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var registration = await _service.GetByIdAsync(
            id,
            CurrentUserId,
            cancellationToken);

        return new ApiResult<ExamRegistrationDto>(
            true,
            ApiResultStatusCode.Success,
            MapToDto(registration));
    }

    /// <summary>
    /// ویرایش اطلاعات داوطلب
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResult<ExamRegistrationDto>), StatusCodes.Status200OK)]
    public async Task<ApiResult<ExamRegistrationDto>> Update([FromRoute] Guid id, [FromBody] UpdateExamRegistrationDto dto, CancellationToken cancellationToken)
    {
        var registration = new ExamRegistration
        {
            UniversityId = dto.UniversityId,
            ExamVenueUniversityId = dto.ExamVenueUniversityId,
            UniversityExamCapacityId = dto.UniversityExamCapacityId,
            UniversityExamGuestCapacityId = dto.UniversityExamGuestCapacityId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            FatherName = dto.FatherName,
            NationalCode = dto.NationalCode,
            GenderId = await ResolveGenderIdAsync(dto.GenderId, dto.GenderPrimaryCode, dto.GenderSubCode, cancellationToken),
            BirthCertificateNumber = dto.BirthCertificateNumber,
            BirthDate = dto.BirthDate,
            StudentNumber = dto.StudentNumber,
            EntryYear = dto.EntryYear,
            PhoneNumber = dto.PhoneNumber,
            EmergencyPhoneNumber = dto.EmergencyPhoneNumber
        };

        var customValues = dto.CustomFieldValues?
            .Select(x => new ExamRegistrationCustomFieldValue
            {
                ExamCustomFieldId = x.ExamCustomFieldId,
                Value = x.Value
            })
            .ToList() ?? [];

        var updated = await _service.UpdateAsync(
            id,
            registration,
            customValues,
            CurrentUserId,
            cancellationToken);

        return new ApiResult<ExamRegistrationDto>(
            true,
            ApiResultStatusCode.Success,
            MapToDto(updated),
            ApiMessages.ExamRegistrations.M002);
    }

    /// <summary>
    /// حذف نرم ثبت‌نام
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    public async Task<ApiResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await _service.SoftDeleteAsync(
            id,
            CurrentUserId,
            cancellationToken);

        return new ApiResult(
            true,
            ApiResultStatusCode.Success,
            ApiMessages.ExamRegistrations.M003);
    }

    private static ExamRegistrationUniversityDto MapToUniversityDto(University university)
    {
        return new ExamRegistrationUniversityDto
        {
            UniversityId = university.Id,
            UniversityTitle = university.Title
        };
    }

    private static ExamRegistrationExamDto MapToExamDto(Exam exam)
    {
        return new ExamRegistrationExamDto
        {
            ExamId = exam.Id,
            ExamTitle = exam.Title
        };
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
            GenderPrimaryCode = item.Gender?.PrimaryCode,
            GenderSubCode = item.Gender?.SubCode,
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