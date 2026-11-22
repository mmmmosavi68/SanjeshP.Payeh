
using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.Data.Contracts.Exams;
using SanjeshP.Payeh.Entities.Exam;
using SanjeshP.Payeh.Entities.University;

namespace SanjeshP.Payeh.Services.Exams;

public sealed class UniversityExamCapacityService
    : IUniversityExamCapacityService, IScopedDependency
{
    private readonly IUniversityExamCapacityRepository _repository;

    public UniversityExamCapacityService(
        IUniversityExamCapacityRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Exam>> GetActiveExamsForUniversityAsync(
        Guid userId,
        int universityId,
        CancellationToken cancellationToken)
    {
        await ValidateUniversityAsync(
            userId,
            universityId,
            cancellationToken);

        return await _repository.GetActiveExamsForUniversityAsync(
            universityId,
            cancellationToken);
    }


    public async Task<UniversityExamCapacity> SaveAsync(
        int universityId,
        Guid examId,
        int allocatedCapacity,
        int ownCapacity,
        Guid createdBy,
        CancellationToken cancellationToken)
    {
        if (createdBy == Guid.Empty)
            throw new BadRequestException("شناسه کاربر ثبت‌کننده معتبر نیست.");

        if (examId == Guid.Empty)
            throw new BadRequestException("آزمون را انتخاب کنید.");

        if (allocatedCapacity <= 0)
            throw new BadRequestException("ظرفیت تخصیص‌یافته باید حداقل ۱ باشد.");

        if (ownCapacity < 0)
            throw new BadRequestException("ظرفیت داوطلبان دانشگاه معتبر نیست.");

        if (ownCapacity > allocatedCapacity)
        {
            throw new BadRequestException(
                "ظرفیت داوطلبان دانشگاه نمی‌تواند بیشتر از ظرفیت کل تخصیص‌یافته باشد.");
        }

        // بررسی دسترسی کاربر به دانشگاه
        var university = await ValidateUniversityAsync(
            createdBy,
            universityId,
            cancellationToken);

        // دریافت آزمون‌های فعال دانشگاه
        var exams = await _repository.GetActiveExamsForUniversityAsync(
            universityId,
            cancellationToken);

        var selectedExam = exams.FirstOrDefault(
            x => x.Id == examId);

        if (selectedExam is null)
        {
            throw new NotFoundException(
                "آزمون موردنظر وجود ندارد یا غیرفعال است.");
        }

        // دریافت ظرفیت قبلی
        var capacity = await _repository.GetByUniversityAndExamAsync(
            universityId,
            examId,
            cancellationToken);

        var registeredCount = 0;
        var ownRegisteredCount = 0;

        // ---------------------------------------------------------
        // کنترل ثبت‌نام‌های موجود
        // این کنترل برای آزمون کاغذی و الکترونیکی اعمال می‌شود.
        // ---------------------------------------------------------

        if (capacity is not null)
        {
            registeredCount = await _repository.CountRegistrationsAsync(
                capacity.Id,
                cancellationToken);

            ownRegisteredCount = await _repository.CountOwnRegistrationsAsync(
                capacity.Id,
                universityId,
                cancellationToken);

            // ظرفیت کل نباید کمتر از تعداد کل ثبت‌نام‌ها باشد.
            if (allocatedCapacity < registeredCount)
            {
                throw new BadRequestException(
                    $"ظرفیت کل نمی‌تواند کمتر از تعداد ثبت‌نام‌های موجود ({registeredCount}) باشد.");
            }

            // ظرفیت خودی نباید کمتر از ثبت‌نام‌های دانشگاه میزبان باشد.
            if (ownCapacity < ownRegisteredCount)
            {
                throw new BadRequestException(
                    $"ظرفیت داوطلبان دانشگاه نمی‌تواند کمتر از تعداد ثبت‌نام‌های خودی ({ownRegisteredCount}) باشد.");
            }
        }

        var isElectronic =
            selectedExam.ExamMode?.Title?.Trim() == "الکترونیکی";

        // ---------------------------------------------------------
        // کنترل ظرفیت دانشگاه در آزمون‌های الکترونیکی این نوبت
        // ---------------------------------------------------------

        if (isElectronic)
        {
            var allocatedInOtherExams =
                await _repository.GetAllocatedCapacityForElectronicExamsInSessionAsync(
                    universityId,
                    selectedExam.ExamSessionId,
                    examId,
                    cancellationToken);

            var totalAllocated =
                allocatedInOtherExams + allocatedCapacity;

            if (totalAllocated > university.CapacityApproved)
            {
                throw new BadRequestException(
                    $"مجموع ظرفیت آزمون‌های الکترونیکی این نوبت ({totalAllocated}) " +
                    $"از ظرفیت تأییدشده دانشگاه ({university.CapacityApproved}) بیشتر است.");
            }

            // -----------------------------------------------------
            // کنترل ظرفیت مهمان‌های تعریف‌شده قبلی
            //
            // مجموع ظرفیت مهمان‌ها نباید از ظرفیت جدید مهمان
            // (AllocatedCapacity - OwnCapacity) بیشتر باشد.
            //
            // این کنترل هنگام کاهش ظرفیت کل یا افزایش ظرفیت خودی
            // نیز اعمال می‌شود.
            // -----------------------------------------------------

            var guestCapacityLimit =
                allocatedCapacity - ownCapacity;

            if (capacity is not null)
            {
                var totalGuestCapacity =
                    await _repository.GetTotalGuestCapacityAsync(
                        capacity.Id,
                        cancellationToken);

                if (totalGuestCapacity > guestCapacityLimit)
                {
                    throw new BadRequestException(
                        $"مجموع ظرفیت دانشگاه‌های مهمان ({totalGuestCapacity}) " +
                        $"از ظرفیت مجاز جدید مهمان ({guestCapacityLimit}) بیشتر است. " +
                        "ابتدا ظرفیت دانشگاه‌های مهمان را کاهش دهید.");
                }
            }
        }

        // ---------------------------------------------------------
        // ثبت ظرفیت جدید
        // ---------------------------------------------------------

        if (capacity is null)
        {
            capacity = new UniversityExamCapacity
            {
                UniversityId = universityId,
                ExamId = examId,
                AllocatedCapacity = allocatedCapacity,
                OwnCapacity = ownCapacity,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _repository.AddAsync(
                capacity,
                cancellationToken);
        }
        else
        {
            // ویرایش ظرفیت قبلی یا بازیابی حذف نرم‌شده
            capacity.AllocatedCapacity = allocatedCapacity;
            capacity.OwnCapacity = ownCapacity;

            capacity.IsDeleted = false;
            capacity.IsActive = true;

            capacity.UpdatedDate = DateTime.UtcNow;

            await _repository.UpdateAsync(
                capacity,
                cancellationToken);
        }

        // دریافت رکورد ذخیره‌شده
        return await _repository.GetByUniversityAndExamAsync(
            universityId,
            examId,
            cancellationToken)
            ?? throw new NotFoundException(
                "اطلاعات ظرفیت پس از ثبت پیدا نشد.");
    }


    private async Task<University> ValidateUniversityAsync(
        Guid userId,
        int universityId,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
            throw new UnauthorizedException();

        if (universityId <= 0)
            throw new BadRequestException("شناسه دانشگاه معتبر نیست.");

        var university = await _repository.GetAccessibleUniversityAsync(
            userId,
            universityId,
            cancellationToken);

        if (university is null)
        {
            throw new ForbiddenException(
                "شما به این دانشگاه دسترسی ندارید یا دانشگاه غیرفعال است.");
        }

        return university;
    }


    public async Task<List<University>> GetAccessibleUniversitiesAsync(
    Guid userId,
    CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
            throw new UnauthorizedAccessException(
                "شناسه کاربر معتبر نیست.");

        return await _repository.GetAccessibleUniversitiesAsync(
            userId,
            cancellationToken);
    }

    public async Task<List<Exam>> GetDefinedExamsForUniversityAsync(
    Guid userId,
    int universityId,
    CancellationToken cancellationToken)
    {
        await ValidateUniversityAsync(
            userId,
            universityId,
            cancellationToken);

        return await _repository.GetDefinedExamsForUniversityAsync(
            universityId,
            cancellationToken);
    }


    public async Task<UniversityExamGuestCapacity> SaveGuestCapacityAsync(
        int universityId,
        Guid examId,
        int guestUniversityId,
        int capacity,
        Guid createdBy,
        CancellationToken cancellationToken)
    {
        if (createdBy == Guid.Empty)
            throw new BadRequestException("شناسه کاربر ثبت‌کننده معتبر نیست.");

        if (examId == Guid.Empty)
            throw new BadRequestException("آزمون را انتخاب کنید.");

        if (guestUniversityId <= 0)
            throw new BadRequestException("دانشگاه مهمان را انتخاب کنید.");

        if (capacity <= 0)
            throw new BadRequestException("ظرفیت مهمان باید حداقل ۱ باشد.");

        // بررسی دسترسی کاربر به دانشگاه میزبان
        await ValidateUniversityAsync(
            createdBy,
            universityId,
            cancellationToken);

        if (universityId == guestUniversityId)
        {
            throw new BadRequestException(
                "دانشگاه میزبان نمی‌تواند به‌عنوان دانشگاه مهمان انتخاب شود.");
        }

        // بررسی فعال بودن دانشگاه مهمان
        var guestUniversity = await _repository.GetActiveUniversityAsync(
            guestUniversityId,
            cancellationToken);

        if (guestUniversity is null)
        {
            throw new NotFoundException(
                "دانشگاه مهمان وجود ندارد یا غیرفعال است.");
        }

        // دریافت آزمون‌های فعال دانشگاه میزبان
        var activeExams =
            await _repository.GetActiveExamsForUniversityAsync(
                universityId,
                cancellationToken);

        var selectedExam = activeExams.FirstOrDefault(
            x => x.Id == examId);

        if (selectedExam is null)
        {
            throw new NotFoundException(
                "آزمون موردنظر وجود ندارد یا غیرفعال است.");
        }

        // دریافت ظرفیت اصلی آزمون دانشگاه میزبان
        var universityExamCapacity =
            await _repository.GetByUniversityAndExamAsync(
                universityId,
                examId,
                cancellationToken);

        if (universityExamCapacity is null ||
            !universityExamCapacity.IsActive ||
            universityExamCapacity.IsDeleted)
        {
            throw new BadRequestException(
                "ابتدا باید ظرفیت فعال آزمون برای دانشگاه میزبان تعریف شود.");
        }

        var allocatedCapacity =
            universityExamCapacity.AllocatedCapacity ?? 0;

        var ownCapacity =
            universityExamCapacity.OwnCapacity ?? 0;

        // ظرفیت مجاز تخصیص به دانشگاه‌های مهمان
        var guestCapacityLimit = allocatedCapacity - ownCapacity;

        // دریافت رکورد قبلی دانشگاه مهمان
        // این متد باید رکورد را حتی در صورت حذف نرم‌شده برگرداند.
        var guestCapacity =
            await _repository.GetGuestCapacityByUniversityAsync(
                universityExamCapacity.Id,
                guestUniversityId,
                cancellationToken);

        // ---------------------------------------------------------
        // کنترل تعداد ثبت‌نام‌های واقعی دانشگاه مهمان
        // این کنترل برای آزمون کاغذی و الکترونیکی الزامی است.
        // حتی اگر رکورد ظرفیت غیرفعال یا حذف نرم شده باشد،
        // ثبت‌نام‌های مرتبط با آن باید شمارش شوند.
        // ---------------------------------------------------------

        var registeredCount = 0;

        if (guestCapacity is not null)
        {
            registeredCount =
                await _repository.CountGuestRegistrationsAsync(
                    guestCapacity.Id,
                    cancellationToken);

            if (capacity < registeredCount)
            {
                throw new BadRequestException(
                    $"ظرفیت دانشگاه مهمان نمی‌تواند کمتر از تعداد ثبت‌نام‌های موجود ({registeredCount}) باشد.");
            }
        }

        // ---------------------------------------------------------
        // محدودیت مجموع ظرفیت مهمان:
        // آزمون الکترونیکی:
        // مجموع ظرفیت مهمان‌ها نباید از
        // AllocatedCapacity - OwnCapacity بیشتر شود.
        //
        // آزمون کاغذی:
        // محدودیت مجموع ظرفیت مهمان اعمال نمی‌شود.
        // ---------------------------------------------------------

        var isElectronic =
            selectedExam.ExamMode?.Title?.Trim() == "الکترونیکی";

        if (isElectronic)
        {
            if (guestCapacityLimit <= 0)
            {
                throw new BadRequestException(
                    "ظرفیتی برای تخصیص به دانشگاه‌های مهمان باقی نمانده است.");
            }

            // مجموع ظرفیت مهمان‌های فعال فعلی
            var totalGuestCapacity =
                await _repository.GetTotalGuestCapacityAsync(
                    universityExamCapacity.Id,
                    cancellationToken);

            // ظرفیت قبلی همین دانشگاه
            // فقط در صورتی از مجموع کم می‌شود که قبلاً فعال بوده باشد.
            var previousCapacity = 0;

            if (guestCapacity is not null &&
                guestCapacity.IsActive &&
                !guestCapacity.IsDeleted)
            {
                previousCapacity = guestCapacity.Capacity;
            }

            // محاسبه مجموع ظرفیت پس از ثبت یا ویرایش
            var newTotalGuestCapacity =
                totalGuestCapacity - previousCapacity + capacity;

            if (newTotalGuestCapacity > guestCapacityLimit)
            {
                throw new BadRequestException(
                    $"مجموع ظرفیت دانشگاه‌های مهمان ({newTotalGuestCapacity}) " +
                    $"از ظرفیت مجاز مهمان ({guestCapacityLimit}) بیشتر است.");
            }
        }

        // ---------------------------------------------------------
        // ثبت ظرفیت جدید یا ویرایش / بازیابی رکورد قبلی
        // ---------------------------------------------------------

        if (guestCapacity is null)
        {
            guestCapacity = new UniversityExamGuestCapacity
            {
                UniversityExamCapacityId = universityExamCapacity.Id,
                GuestUniversityId = guestUniversityId,
                Capacity = capacity,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _repository.AddGuestCapacityAsync(
                guestCapacity,
                cancellationToken);
        }
        else
        {
            // ویرایش ظرفیت قبلی یا بازیابی حذف نرم‌شده
            guestCapacity.Capacity = capacity;
            guestCapacity.IsActive = true;
            guestCapacity.IsDeleted = false;
            guestCapacity.UpdatedDate = DateTime.UtcNow;

            await _repository.UpdateGuestCapacityAsync(
                guestCapacity,
                cancellationToken);
        }

        // دریافت رکورد ذخیره‌شده
        var savedGuestCapacities =
            await _repository.GetGuestCapacitiesAsync(
                universityExamCapacity.Id,
                cancellationToken);

        return savedGuestCapacities.FirstOrDefault(
            x => x.GuestUniversityId == guestUniversityId)
            ?? throw new NotFoundException(
                "ظرفیت دانشگاه مهمان پس از ثبت پیدا نشد.");
    }

    public async Task<List<UniversityExamGuestCapacity>> GetGuestCapacitiesAsync(
    Guid userId,
    int universityId,
    Guid examId,
    CancellationToken cancellationToken)
    {
        await ValidateUniversityAsync(
            userId,
            universityId,
            cancellationToken);

        if (examId == Guid.Empty)
            throw new BadRequestException("آزمون را انتخاب کنید.");

        var universityExamCapacity =
            await _repository.GetByUniversityAndExamAsync(
                universityId,
                examId,
                cancellationToken);

        if (universityExamCapacity is null ||
            !universityExamCapacity.IsActive ||
            universityExamCapacity.IsDeleted)
        {
            throw new NotFoundException(
                "ظرفیت فعال این آزمون برای دانشگاه پیدا نشد.");
        }

        return await _repository.GetGuestCapacitiesAsync(
            universityExamCapacity.Id,
            cancellationToken);
    }

    public Task<Dictionary<Guid, UniversityExamCapacityUsage>> GetCapacityUsageAsync(
        int universityId,
        IReadOnlyCollection<Guid> capacityIds,
        CancellationToken cancellationToken)
    {
        return _repository.GetCapacityUsageAsync(
            universityId,
            capacityIds,
            cancellationToken);
    }

    public Task<Dictionary<Guid, int>> GetGuestRegistrationCountsAsync(
        IReadOnlyCollection<Guid> guestCapacityIds,
        CancellationToken cancellationToken)
    {
        return _repository.GetGuestRegistrationCountsAsync(
            guestCapacityIds,
            cancellationToken);
    }
}
