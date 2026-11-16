using System.Data;
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
    private readonly ApplicationDbContext _db;
    private readonly IIdentityValidationService _identityValidation;

    public ExamRegistrationService(
        ApplicationDbContext db,
        IIdentityValidationService identityValidation)
    {
        _db = db;
        _identityValidation = identityValidation;
    }

    public async Task<ExamRegistration> CreateAsync(
        ExamRegistration registration,
        IReadOnlyCollection<ExamRegistrationCustomFieldValue> customFieldValues,
        Guid createdBy,
        CancellationToken cancellationToken)
    {
        if (createdBy == Guid.Empty)
            throw new BadRequestException("شناسه کارشناس ثبت‌کننده معتبر نیست.");

        if (registration.ExamId == Guid.Empty)
            throw new BadRequestException("انتخاب آزمون الزامی است.");

        if (string.IsNullOrWhiteSpace(registration.FirstName))
            throw new BadRequestException("نام داوطلب الزامی است.");

        if (string.IsNullOrWhiteSpace(registration.LastName))
            throw new BadRequestException("نام خانوادگی داوطلب الزامی است.");

        var exam = await _db.Set<Exam>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == registration.ExamId, cancellationToken);

        if (exam is null)
            throw new NotFoundException("آزمون موردنظر پیدا نشد.");

        if (!exam.IsActive)
            throw new BadRequestException("ثبت‌نام در این آزمون غیرفعال است.");

        var now = DateTime.UtcNow;
        if (now < exam.RegistrationStartDate || now > exam.RegistrationEndDate)
            throw new BadRequestException("در حال حاضر بازه ثبت‌نام این آزمون فعال نیست.");

        var candidateUniversity = await _db.Set<University>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == registration.UniversityId && x.IsActive,
                cancellationToken);

        if (candidateUniversity is null)
            throw new BadRequestException("دانشگاه محل تحصیل معتبر نیست یا غیرفعال شده است.");

        var venueUniversity = await _db.Set<University>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == registration.ExamVenueUniversityId && x.IsActive,
                cancellationToken);

        if (venueUniversity is null)
            throw new BadRequestException("دانشگاه محل آزمون معتبر نیست یا غیرفعال شده است.");

        var capacity = await _db.Set<UniversityExamCapacity>()
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
                var ownCount = await _db.Set<ExamRegistration>()
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

            var guestCount = await _db.Set<ExamRegistration>()
                .CountAsync(
                    x => x.UniversityExamGuestCapacityId == guestCapacity.Id,
                    cancellationToken);

            if (guestCount >= guestCapacity.Capacity)
                throw new BadRequestException("ظرفیت دانشگاه مهمان تکمیل شده است.");
        }

        if (capacity.AllocatedCapacity.HasValue)
        {
            var totalCount = await _db.Set<ExamRegistration>()
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

        _db.Set<ExamRegistration>().Add(registration);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new AppException(
                ApiResultStatusCode.Conflict,
                "کد ملی یا شماره همراه قبلاً در این آزمون ثبت شده است.");
        }

        return await GetByIdAsync(registration.Id, cancellationToken);
    }

    public async Task<ExamRegistration> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _db.Set<ExamRegistration>()
            .AsNoTracking()
            .Include(x => x.CustomFieldValues)
                .ThenInclude(x => x.ExamCustomField)
            .Include(x => x.University)
            .Include(x => x.ExamVenueUniversity)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (item is null)
            throw new NotFoundException("ثبت‌نام موردنظر پیدا نشد.");

        return item;
    }

    public async Task<List<ExamRegistration>> GetByExamAsync(
        Guid examId,
        CancellationToken cancellationToken)
    {
        if (examId == Guid.Empty)
            throw new BadRequestException("شناسه آزمون معتبر نیست.");

        var examExists = await _db.Set<Exam>()
            .AnyAsync(x => x.Id == examId, cancellationToken);

        if (!examExists)
            throw new NotFoundException("آزمون موردنظر پیدا نشد.");

        return await _db.Set<ExamRegistration>()
            .AsNoTracking()
            .Where(x => x.ExamId == examId)
            .Include(x => x.University)
            .Include(x => x.ExamVenueUniversity)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _db.Set<ExamRegistration>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (item is null)
            throw new NotFoundException("ثبت‌نام موردنظر پیدا نشد.");

        item.IsDeleted = true;
        item.IsActive = false;
        item.UpdatedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
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

        var fields = await _db.Set<ExamCustomField>()
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

        // در صورتی که OptionsJson یک آرایه JSON از رشته‌ها باشد، مقدار باید یکی از گزینه‌ها باشد.
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
                // ساختارهای JSON دیگر تا زمان تعیین قرارداد دقیق فیلدها، تفسیر نمی‌شوند.
            }
        }

        return values;
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
}
