
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.Common.Utilities;
using SanjeshP.Payeh.Data;
using SanjeshP.Payeh.Entities.Exam;
using SanjeshP.Payeh.Entities.University;
using SanjeshP.Payeh.Services.Validations;

namespace SanjeshP.Payeh.Services.Exams;

public sealed class ExamRegistrationService : IExamRegistrationService, IScopedDependency
{
    private readonly IUnitOfWork _uow;
    private readonly IIdentityValidationService _identityValidation;

    public ExamRegistrationService(
        IUnitOfWork uow,
        IIdentityValidationService identityValidation)
    {
        _uow = uow;
        _identityValidation = identityValidation;
    }

    public async Task<List<University>> GetAccessibleUniversitiesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
            throw new BadRequestException("شناسه کاربر معتبر نیست.");

        return await _uow.Repository<SanjeshP.Payeh.Entities.User.UserUniversity>().Entities
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.IsActive)
            .Select(x => x.University)
            .Where(x => x.IsActive)
            .OrderBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Exam>> GetAvailableExamsAsync(
        int universityId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (universityId <= 0)
            throw new BadRequestException("دانشگاه انتخاب‌شده معتبر نیست.");

        await EnsureUniversityAccessAsync(
            userId,
            universityId,
            cancellationToken);

        var now = DateTime.UtcNow;

        return await _uow.Repository<Exam>().Entities
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                x.RegistrationStartDate <= now &&
                x.RegistrationEndDate >= now &&
                x.UniversityExamCapacities.Any(c =>
                    c.IsActive &&
                    c.University.IsActive &&
                    (
                        c.UniversityId == universityId ||
                        c.GuestCapacities.Any(g =>
                            g.IsActive &&
                            g.GuestUniversityId == universityId)
                    )))
            .OrderByDescending(x => x.RegistrationStartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<ExamRegistration> CreateAsync(
        ExamRegistration registration,
        IReadOnlyCollection<ExamRegistrationCustomFieldValue> customFieldValues,
        Guid createdBy,
        CancellationToken cancellationToken)
    {
        if (createdBy == Guid.Empty)
            throw new BadRequestException("شناسه کارشناس ثبت‌کننده معتبر نیست.");

        await EnsureUniversityAccessAsync(
            createdBy,
            registration.UniversityId,
            cancellationToken);

        if (registration.ExamId == Guid.Empty)
            throw new BadRequestException("انتخاب آزمون الزامی است.");

        if (string.IsNullOrWhiteSpace(registration.FirstName))
            throw new BadRequestException("نام داوطلب الزامی است.");

        if (string.IsNullOrWhiteSpace(registration.LastName))
            throw new BadRequestException("نام خانوادگی داوطلب الزامی است.");

        var exam = await _uow.Repository<Exam>().Entities
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == registration.ExamId, cancellationToken);

        if (exam is null)
            throw new NotFoundException("آزمون موردنظر پیدا نشد.");

        if (!exam.IsActive)
            throw new BadRequestException("ثبت‌نام در این آزمون غیرفعال است.");

        var now = DateTime.UtcNow;

        if (now < exam.RegistrationStartDate || now > exam.RegistrationEndDate)
            throw new BadRequestException("در حال حاضر بازه ثبت‌نام این آزمون فعال نیست.");

        var candidateUniversity = await _uow.Repository<University>().Entities
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == registration.UniversityId && x.IsActive,
                cancellationToken);

        if (candidateUniversity is null)
            throw new BadRequestException("دانشگاه محل تحصیل معتبر نیست یا غیرفعال شده است.");

        var venueUniversity = await _uow.Repository<University>().Entities
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == registration.ExamVenueUniversityId && x.IsActive,
                cancellationToken);

        if (venueUniversity is null)
            throw new BadRequestException("دانشگاه محل آزمون معتبر نیست یا غیرفعال شده است.");

        var capacity = await _uow.Repository<UniversityExamCapacity>().Entities
            .Include(x => x.GuestCapacities)
            .FirstOrDefaultAsync(
                x => x.Id == registration.UniversityExamCapacityId &&
                     x.ExamId == registration.ExamId &&
                     x.UniversityId == registration.ExamVenueUniversityId &&
                     x.IsActive,
                cancellationToken);

        if (capacity is null)
            throw new BadRequestException("تخصیص ظرفیت انتخاب‌شده با آزمون و دانشگاه محل برگزاری مطابقت ندارد.");

        registration.NationalCode = await _identityValidation
            .ValidateExamRegistrationNationalCodeAsync(
                registration.ExamId,
                registration.NationalCode,
                null,
                cancellationToken);

        registration.PhoneNumber = await _identityValidation
            .ValidateExamRegistrationMobileNumberAsync(
                registration.ExamId,
                registration.PhoneNumber,
                null,
                cancellationToken);

        registration.FirstName = registration.FirstName.Trim();
        registration.LastName = registration.LastName.Trim();
        registration.FatherName = NullIfWhiteSpace(registration.FatherName);
        registration.BirthCertificateNumber = NullIfWhiteSpace(registration.BirthCertificateNumber);
        registration.StudentNumber = NullIfWhiteSpace(registration.StudentNumber);
        registration.EmergencyPhoneNumber = NullIfWhiteSpace(registration.EmergencyPhoneNumber);

        var isOwnUniversity = registration.UniversityId == capacity.UniversityId;

        UniversityExamGuestCapacity? guestCapacity = null;

        if (isOwnUniversity)
        {
            if (registration.UniversityExamGuestCapacityId.HasValue)
                throw new BadRequestException("برای داوطلب دانشگاه برگزارکننده، انتخاب ظرفیت دانشگاه مهمان مجاز نیست.");

            if (capacity.OwnCapacity.HasValue)
            {
                var ownCount = await _uow.Repository<ExamRegistration>().Entities
                    .CountAsync(
                        x => x.UniversityExamCapacityId == capacity.Id &&
                             x.UniversityId == capacity.UniversityId,
                        cancellationToken);

                if (ownCount >= capacity.OwnCapacity.Value)
                    throw new BadRequestException("ظرفیت داوطلبان دانشگاه برگزارکننده تکمیل شده است.");
            }
        }
        else
        {
            if (!registration.UniversityExamGuestCapacityId.HasValue)
                throw new BadRequestException("برای داوطلب دانشگاه مهمان، انتخاب ظرفیت مهمان الزامی است.");

            guestCapacity = capacity.GuestCapacities.FirstOrDefault(
                x => x.Id == registration.UniversityExamGuestCapacityId.Value &&
                     x.GuestUniversityId == registration.UniversityId &&
                     x.IsActive);

            if (guestCapacity is null)
                throw new BadRequestException("ظرفیت مهمان انتخاب‌شده با دانشگاه داوطلب مطابقت ندارد.");

            var guestCount = await _uow.Repository<ExamRegistration>().Entities
                .CountAsync(
                    x => x.UniversityExamGuestCapacityId == guestCapacity.Id,
                    cancellationToken);

            if (guestCount >= guestCapacity.Capacity)
                throw new BadRequestException("ظرفیت دانشگاه مهمان تکمیل شده است.");
        }

        if (capacity.AllocatedCapacity.HasValue)
        {
            var totalCount = await _uow.Repository<ExamRegistration>().Entities
                .CountAsync(
                    x => x.UniversityExamCapacityId == capacity.Id,
                    cancellationToken);

            if (totalCount >= capacity.AllocatedCapacity.Value)
                throw new BadRequestException("ظرفیت کل تخصیص‌یافته به دانشگاه محل آزمون تکمیل شده است.");
        }

        var values = await ValidateCustomFieldsAsync(
            registration.ExamId,
            customFieldValues,
            cancellationToken);

        registration.Id = Guid.NewGuid();
        registration.CreatedBy = createdBy;
        registration.CreatedDate = DateTime.UtcNow;
        registration.UpdatedDate = null;
        registration.IsActive = true;
        registration.IsDeleted = false;
        registration.CustomFieldValues = values;

        foreach (var item in values)
        {
            item.Id = Guid.NewGuid();
            item.RegistrationId = registration.Id;
            item.CreatedBy = createdBy;
            item.CreatedDate = DateTime.UtcNow;
            item.UpdatedDate = null;
            item.IsActive = true;
            item.IsDeleted = false;
        }

        _uow.Repository<ExamRegistration>().Entities.Add(registration);

        try
        {
            await _uow.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new AppException(
                ApiResultStatusCode.Conflict,
                "کد ملی یا شماره همراه قبلاً در این آزمون ثبت شده است.");
        }

        return await GetByIdAsync(registration.Id, createdBy, cancellationToken);
    }

    public async Task<ExamRegistration> UpdateAsync(
        Guid id,
        ExamRegistration updatedRegistration,
        IReadOnlyCollection<ExamRegistrationCustomFieldValue> customFieldValues,
        Guid updatedBy,
        CancellationToken cancellationToken)
    {
        if (updatedBy == Guid.Empty)
            throw new BadRequestException("شناسه کارشناس ویرایش‌کننده معتبر نیست.");

        var item = await _uow.Repository<ExamRegistration>().Entities
            .Include(x => x.CustomFieldValues)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (item is null)
            throw new NotFoundException("ثبت‌نام موردنظر پیدا نشد.");

        await EnsureUniversityAccessAsync(
            updatedBy,
            item.UniversityId,
            cancellationToken);

        if (updatedRegistration.UniversityId != item.UniversityId)
            throw new BadRequestException("تغییر دانشگاه محل تحصیل در ویرایش ثبت‌نام مجاز نیست.");

        var exam = await _uow.Repository<Exam>().Entities
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == item.ExamId, cancellationToken);

        if (exam is null)
            throw new NotFoundException("آزمون موردنظر پیدا نشد.");

        var now = DateTime.UtcNow;

        if (!exam.EditStartDate.HasValue ||
            !exam.EditEndDate.HasValue ||
            now < exam.EditStartDate.Value ||
            now > exam.EditEndDate.Value)
            throw new BadRequestException("در حال حاضر بازه ویرایش اطلاعات این آزمون فعال نیست.");

        if (string.IsNullOrWhiteSpace(updatedRegistration.FirstName))
            throw new BadRequestException("نام داوطلب الزامی است.");

        if (string.IsNullOrWhiteSpace(updatedRegistration.LastName))
            throw new BadRequestException("نام خانوادگی داوطلب الزامی است.");

        var venueUniversity = await _uow.Repository<University>().Entities
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == updatedRegistration.ExamVenueUniversityId &&
                     x.IsActive,
                cancellationToken);

        if (venueUniversity is null)
            throw new BadRequestException("دانشگاه محل آزمون معتبر نیست یا غیرفعال شده است.");

        var capacity = await _uow.Repository<UniversityExamCapacity>().Entities
            .Include(x => x.GuestCapacities)
            .FirstOrDefaultAsync(
                x => x.Id == updatedRegistration.UniversityExamCapacityId &&
                     x.ExamId == item.ExamId &&
                     x.UniversityId == updatedRegistration.ExamVenueUniversityId &&
                     x.IsActive,
                cancellationToken);

        if (capacity is null)
            throw new BadRequestException("تخصیص ظرفیت انتخاب‌شده با آزمون و دانشگاه محل برگزاری مطابقت ندارد.");

        var nationalCode = await _identityValidation
            .ValidateExamRegistrationNationalCodeAsync(
                item.ExamId,
                updatedRegistration.NationalCode,
                id,
                cancellationToken);

        var phoneNumber = await _identityValidation
            .ValidateExamRegistrationMobileNumberAsync(
                item.ExamId,
                updatedRegistration.PhoneNumber,
                id,
                cancellationToken);

        var isOwnUniversity = item.UniversityId == capacity.UniversityId;

        UniversityExamGuestCapacity? guestCapacity = null;

        if (isOwnUniversity)
        {
            if (updatedRegistration.UniversityExamGuestCapacityId.HasValue)
                throw new BadRequestException("برای داوطلب دانشگاه برگزارکننده، انتخاب ظرفیت دانشگاه مهمان مجاز نیست.");

            if (capacity.OwnCapacity.HasValue)
            {
                var ownCount = await _uow.Repository<ExamRegistration>().Entities
                    .CountAsync(
                        x => x.UniversityExamCapacityId == capacity.Id &&
                             x.UniversityId == capacity.UniversityId &&
                             x.Id != id,
                        cancellationToken);

                if (ownCount >= capacity.OwnCapacity.Value)
                    throw new BadRequestException("ظرفیت داوطلبان دانشگاه برگزارکننده تکمیل شده است.");
            }
        }
        else
        {
            if (!updatedRegistration.UniversityExamGuestCapacityId.HasValue)
                throw new BadRequestException("برای داوطلب دانشگاه مهمان، انتخاب ظرفیت مهمان الزامی است.");

            guestCapacity = capacity.GuestCapacities.FirstOrDefault(
                x => x.Id == updatedRegistration.UniversityExamGuestCapacityId.Value &&
                     x.GuestUniversityId == item.UniversityId &&
                     x.IsActive);

            if (guestCapacity is null)
                throw new BadRequestException("ظرفیت مهمان انتخاب‌شده با دانشگاه داوطلب مطابقت ندارد.");

            var guestCount = await _uow.Repository<ExamRegistration>().Entities
                .CountAsync(
                    x => x.UniversityExamGuestCapacityId == guestCapacity.Id &&
                         x.Id != id,
                    cancellationToken);

            if (guestCount >= guestCapacity.Capacity)
                throw new BadRequestException("ظرفیت دانشگاه مهمان تکمیل شده است.");
        }

        if (capacity.AllocatedCapacity.HasValue)
        {
            var totalCount = await _uow.Repository<ExamRegistration>().Entities
                .CountAsync(
                    x => x.UniversityExamCapacityId == capacity.Id &&
                         x.Id != id,
                    cancellationToken);

            if (totalCount >= capacity.AllocatedCapacity.Value)
                throw new BadRequestException("ظرفیت کل تخصیص‌یافته به دانشگاه محل آزمون تکمیل شده است.");
        }

        var values = await ValidateCustomFieldsAsync(
            item.ExamId,
            customFieldValues,
            cancellationToken);

        item.ExamVenueUniversityId = updatedRegistration.ExamVenueUniversityId;
        item.UniversityExamCapacityId = updatedRegistration.UniversityExamCapacityId;
        item.UniversityExamGuestCapacityId = updatedRegistration.UniversityExamGuestCapacityId;
        item.FirstName = updatedRegistration.FirstName.Trim();
        item.LastName = updatedRegistration.LastName.Trim();
        item.FatherName = NullIfWhiteSpace(updatedRegistration.FatherName);
        item.NationalCode = nationalCode;
        item.GenderId = updatedRegistration.GenderId;
        item.BirthCertificateNumber = NullIfWhiteSpace(updatedRegistration.BirthCertificateNumber);
        item.BirthDate = updatedRegistration.BirthDate;
        item.StudentNumber = NullIfWhiteSpace(updatedRegistration.StudentNumber);
        item.EntryYear = updatedRegistration.EntryYear;
        item.PhoneNumber = phoneNumber;
        item.EmergencyPhoneNumber = NullIfWhiteSpace(updatedRegistration.EmergencyPhoneNumber);
        item.UpdatedDate = DateTime.UtcNow;

        foreach (var oldValue in item.CustomFieldValues.Where(x => !x.IsDeleted))
        {
            oldValue.IsDeleted = true;
            oldValue.IsActive = false;
            oldValue.UpdatedDate = DateTime.UtcNow;
        }

        foreach (var value in values)
        {
            value.Id = Guid.NewGuid();
            value.RegistrationId = item.Id;
            value.CreatedBy = updatedBy;
            value.CreatedDate = DateTime.UtcNow;
            value.UpdatedDate = null;
            value.IsActive = true;
            value.IsDeleted = false;

            item.CustomFieldValues.Add(value);
        }

        try
        {
            await _uow.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new AppException(
                ApiResultStatusCode.Conflict,
                "کد ملی یا شماره همراه قبلاً در این آزمون ثبت شده است.");
        }

        return await GetByIdAsync(item.Id, updatedBy, cancellationToken);
    }

    public async Task<ExamRegistration> GetByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var item = await _uow.Repository<ExamRegistration>().Entities
            .AsNoTracking()
            .Include(x => x.CustomFieldValues)
                .ThenInclude(x => x.ExamCustomField)
            .Include(x => x.University)
            .Include(x => x.ExamVenueUniversity)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (item is null)
            throw new NotFoundException("ثبت‌نام موردنظر پیدا نشد.");

        await EnsureUniversityAccessAsync(
            userId,
            item.UniversityId,
            cancellationToken);

        return item;
    }

    public async Task<List<ExamRegistration>> GetByExamAsync(
        Guid examId,
        int universityId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (examId == Guid.Empty)
            throw new BadRequestException("شناسه آزمون معتبر نیست.");

        await EnsureUniversityAccessAsync(
            userId,
            universityId,
            cancellationToken);

        var examExists = await _uow.Repository<Exam>().Entities
            .AnyAsync(x => x.Id == examId, cancellationToken);

        if (!examExists)
            throw new NotFoundException("آزمون موردنظر پیدا نشد.");

        return await _uow.Repository<ExamRegistration>().Entities
            .AsNoTracking()
            .Where(x =>
                x.ExamId == examId &&
                x.UniversityId == universityId)
            .Include(x => x.University)
            .Include(x => x.ExamVenueUniversity)
            .Include(x => x.CustomFieldValues)
                .ThenInclude(x => x.ExamCustomField)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var item = await _uow.Repository<ExamRegistration>().Entities
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (item is null)
            throw new NotFoundException("ثبت‌نام موردنظر پیدا نشد.");

        await EnsureUniversityAccessAsync(
            userId,
            item.UniversityId,
            cancellationToken);

        item.IsDeleted = true;
        item.IsActive = false;
        item.UpdatedDate = DateTime.UtcNow;

        await _uow.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<ExamRegistrationCustomFieldValue>> ValidateCustomFieldsAsync(
        Guid examId,
        IReadOnlyCollection<ExamRegistrationCustomFieldValue> submittedValues,
        CancellationToken cancellationToken)
    {
        var values = submittedValues?.ToList() ?? [];

        if (values.Any(x => x.ExamCustomFieldId == Guid.Empty))
            throw new BadRequestException("شناسه یکی از فیلدهای اختصاصی معتبر نیست.");

        if (values.Select(x => x.ExamCustomFieldId).Distinct().Count() != values.Count)
            throw new BadRequestException("یک فیلد اختصاصی بیش از یک بار ارسال شده است.");

        var fields = await _uow.Repository<ExamCustomField>().Entities
            .AsNoTracking()
            .Where(x => x.ExamId == examId && x.IsActive)
            .ToListAsync(cancellationToken);

        var fieldsById = fields.ToDictionary(x => x.Id);

        foreach (var value in values)
        {
            if (!fieldsById.ContainsKey(value.ExamCustomFieldId))
                throw new BadRequestException("یکی از فیلدهای اختصاصی متعلق به این آزمون نیست یا غیرفعال است.");

            if (value.Value?.Length > 4000)
                throw new BadRequestException("مقدار فیلد اختصاصی نباید بیش از ۴۰۰۰ کاراکتر باشد.");

            value.Value = NullIfWhiteSpace(value.Value);
        }

        foreach (var field in fields.Where(x => x.IsRequired))
        {
            var submitted = values.FirstOrDefault(x => x.ExamCustomFieldId == field.Id);

            if (submitted is null || string.IsNullOrWhiteSpace(submitted.Value))
                throw new BadRequestException($"تکمیل فیلد «{field.Label}» الزامی است.");
        }

        foreach (var value in values.Where(x => !string.IsNullOrWhiteSpace(x.Value)))
        {
            var field = fieldsById[value.ExamCustomFieldId];

            if (string.IsNullOrWhiteSpace(field.OptionsJson))
                continue;

            try
            {
                using var document = JsonDocument.Parse(field.OptionsJson);

                if (document.RootElement.ValueKind != JsonValueKind.Array)
                    continue;

                var options = document.RootElement.EnumerateArray()
                    .Where(x => x.ValueKind == JsonValueKind.String)
                    .Select(x => x.GetString())
                    .Where(x => x is not null)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (options.Count > 0 && !options.Contains(value.Value))
                    throw new BadRequestException($"مقدار انتخاب‌شده برای فیلد «{field.Label}» معتبر نیست.");
            }
            catch (JsonException)
            {
                // ساختارهای JSON دیگر تفسیر نمی‌شوند.
            }
        }

        return values;
    }

    private async Task EnsureUniversityAccessAsync(
        Guid userId,
        int universityId,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
            throw new BadRequestException("شناسه کاربر معتبر نیست.");

        var hasAccess = await _uow.Repository<SanjeshP.Payeh.Entities.User.UserUniversity>().Entities
            .AnyAsync(
                x => x.UserId == userId &&
                     x.UniversityId == universityId &&
                     x.IsActive,
                cancellationToken);

        if (!hasAccess)
            throw new AppException(
                ApiResultStatusCode.Forbidden,
                "شما به این دانشگاه دسترسی ندارید.");
    }

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is SqlException sqlException &&
                (sqlException.Number == 2601 || sqlException.Number == 2627))
                return true;
        }

        return false;
    }


    public async Task<(List<UniversityExamCapacity> Capacities, List<ExamCustomField> CustomFields)>
    GetRegistrationOptionsAsync(
        Guid examId,
        int universityId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (examId == Guid.Empty)
            throw new BadRequestException("آزمون انتخاب‌شده معتبر نیست.");

        if (universityId <= 0)
            throw new BadRequestException("دانشگاه انتخاب‌شده معتبر نیست.");

        await EnsureUniversityAccessAsync(
            userId,
            universityId,
            cancellationToken);

        var now = DateTime.UtcNow;

        var examExists = await _uow.Repository<Exam>().Entities
            .AnyAsync(x =>
                x.Id == examId &&
                x.IsActive &&
                x.RegistrationStartDate <= now &&
                x.RegistrationEndDate >= now,
                cancellationToken);

        if (!examExists)
            throw new BadRequestException("آزمون فعال یا در بازه ثبت‌نام نیست.");

        var capacities = await _uow.Repository<UniversityExamCapacity>().Entities
            .AsNoTracking()
            .Where(x =>
                x.ExamId == examId &&
                x.IsActive &&
                x.University.IsActive &&
                (
                    x.UniversityId == universityId ||
                    x.GuestCapacities.Any(g =>
                        g.IsActive &&
                        g.GuestUniversityId == universityId)
                ))
            .Include(x => x.University)
            .Include(x => x.GuestCapacities
                .Where(g =>
                    g.IsActive &&
                    g.GuestUniversityId == universityId))
                .ThenInclude(x => x.GuestUniversity)
            .OrderBy(x => x.University.Title)
            .ToListAsync(cancellationToken);

        var customFields = await _uow.Repository<ExamCustomField>().Entities
            .AsNoTracking()
            .Where(x => x.ExamId == examId && x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);

        return (capacities, customFields);
    }
}
    