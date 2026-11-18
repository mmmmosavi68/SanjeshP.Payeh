
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
            capacity.IsActive = true;
            capacity.UpdatedDate = DateTime.UtcNow;

            await _repository.UpdateAsync(capacity, cancellationToken);
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
}
