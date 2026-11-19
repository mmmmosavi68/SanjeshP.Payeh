
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

        var university = await ValidateUniversityAsync(
            createdBy,
            universityId,
            cancellationToken);

        var exam = await _repository.GetActiveExamsForUniversityAsync(
            universityId,
            cancellationToken);

        var selectedExam = exam.FirstOrDefault(x => x.Id == examId);

        if (selectedExam is null)
        {
            throw new NotFoundException(
                "آزمون موردنظر وجود ندارد یا غیرفعال است.");
        }

        var capacity = await _repository.GetByUniversityAndExamAsync(
            universityId,
            examId,
            cancellationToken);

        var registeredCount = 0;
        var ownRegisteredCount = 0;

        if (capacity is not null)
        {
            registeredCount = await _repository.CountRegistrationsAsync(
                capacity.Id,
                cancellationToken);

            ownRegisteredCount = await _repository.CountOwnRegistrationsAsync(
                capacity.Id,
                universityId,
                cancellationToken);

            if (allocatedCapacity < registeredCount)
            {
                throw new BadRequestException(
                    $"ظرفیت کل نمی‌تواند کمتر از تعداد ثبت‌نام‌های موجود ({registeredCount}) باشد.");
            }

            if (ownCapacity < ownRegisteredCount)
            {
                throw new BadRequestException(
                    $"ظرفیت داوطلبان دانشگاه نمی‌تواند کمتر از تعداد ثبت‌نام‌های خودی ({ownRegisteredCount}) باشد.");
            }
        }

        var isElectronic =
            selectedExam.ExamMode?.Title?.Trim() == "الکترونیکی";

        if (isElectronic)
        {
            var allocatedInOtherExams =
                await _repository.GetAllocatedCapacityForElectronicExamsInSessionAsync(
                    universityId,
                    selectedExam.ExamSessionId,
                    examId,
                    cancellationToken);

            var totalAllocated = allocatedInOtherExams + allocatedCapacity;

            if (totalAllocated > university.CapacityApproved)
            {
                throw new BadRequestException(
                    $"مجموع ظرفیت آزمون‌های الکترونیکی این نوبت ({totalAllocated}) " +
                    $"از ظرفیت تأییدشده دانشگاه ({university.CapacityApproved}) بیشتر است.");
            }
        }

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

            await _repository.AddAsync(capacity, cancellationToken);
        }
        else
        {
            capacity.AllocatedCapacity = allocatedCapacity;
            capacity.OwnCapacity = ownCapacity;

            // بازیابی رکورد حذف نرم‌شده
            capacity.IsDeleted = false;
            capacity.IsActive = true;

            capacity.UpdatedDate = DateTime.UtcNow;

            await _repository.UpdateAsync(
                capacity,
                cancellationToken);
        }

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

        // دریافت ظرفیت اصلی آزمون
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

        // ظرفیت قابل تخصیص به دانشگاه‌های مهمان
        var guestCapacityLimit = allocatedCapacity - ownCapacity;

        if (guestCapacityLimit <= 0)
        {
            throw new BadRequestException(
                "ظرفیتی برای تخصیص به دانشگاه‌های مهمان باقی نمانده است.");
        }

        // دریافت رکورد قبلی؛ حتی اگر حذف نرم شده باشد
        var guestCapacity =
            await _repository.GetGuestCapacityByUniversityAsync(
                universityExamCapacity.Id,
                guestUniversityId,
                cancellationToken);

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
                    $"ظرفیت مهمان نمی‌تواند کمتر از تعداد ثبت‌نام‌های موجود ({registeredCount}) باشد.");
            }
        }

        // مجموع ظرفیت مهمان‌های فعلی
        var totalGuestCapacity =
            await _repository.GetTotalGuestCapacityAsync(
                universityExamCapacity.Id,
                cancellationToken);

        // در صورت ویرایش، ظرفیت قبلی این دانشگاه از مجموع کم می‌شود
        var previousCapacity = 0;

        if (guestCapacity is not null &&
            guestCapacity.IsActive &&
            !guestCapacity.IsDeleted)
        {
            previousCapacity = guestCapacity.Capacity;
        }

        var newTotalGuestCapacity =
            totalGuestCapacity - previousCapacity + capacity;

        if (newTotalGuestCapacity > guestCapacityLimit)
        {
            throw new BadRequestException(
                $"مجموع ظرفیت دانشگاه‌های مهمان ({newTotalGuestCapacity}) " +
                $"از ظرفیت مجاز مهمان ({guestCapacityLimit}) بیشتر است.");
        }

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
            guestCapacity.Capacity = capacity;
            guestCapacity.IsActive = true;
            guestCapacity.IsDeleted = false;
            guestCapacity.UpdatedDate = DateTime.UtcNow;

            await _repository.UpdateGuestCapacityAsync(
                guestCapacity,
                cancellationToken);
        }

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
}
