
using SanjeshP.Payeh.Entities.Exam;
using SanjeshP.Payeh.Entities.University;

namespace SanjeshP.Payeh.Data.Contracts.Exams;

public interface IUniversityExamCapacityRepository
{
    Task<University?> GetActiveUniversityAsync(int universityId, CancellationToken cancellationToken);

    Task<List<Exam>> GetActiveExamsForUniversityAsync(int universityId, CancellationToken cancellationToken);

    Task<UniversityExamCapacity?> GetByUniversityAndExamAsync(int universityId, Guid examId, CancellationToken cancellationToken);

    Task<int> GetAllocatedCapacityForElectronicExamsInSessionAsync(int universityId, Guid examSessionId, Guid? excludedExamId, CancellationToken cancellationToken);

    Task<int> CountRegistrationsAsync(Guid capacityId, CancellationToken cancellationToken);

    Task<int> CountOwnRegistrationsAsync(Guid capacityId, int universityId, CancellationToken cancellationToken);

    Task AddAsync(UniversityExamCapacity capacity, CancellationToken cancellationToken);

    Task UpdateAsync(UniversityExamCapacity capacity, CancellationToken cancellationToken);

    Task<List<University>> GetAccessibleUniversitiesAsync(Guid userId, CancellationToken cancellationToken);

    Task<University?> GetAccessibleUniversityAsync(Guid userId, int universityId, CancellationToken cancellationToken);

    Task<List<Exam>> GetDefinedExamsForUniversityAsync(int universityId, CancellationToken cancellationToken);

    Task<List<UniversityExamGuestCapacity>> GetGuestCapacitiesAsync(Guid universityExamCapacityId, CancellationToken cancellationToken);

    Task<UniversityExamGuestCapacity?> GetGuestCapacityByUniversityAsync(Guid universityExamCapacityId, int guestUniversityId, CancellationToken cancellationToken);

    Task<int> CountGuestRegistrationsAsync(Guid guestCapacityId, CancellationToken cancellationToken);

    Task AddGuestCapacityAsync(UniversityExamGuestCapacity guestCapacity, CancellationToken cancellationToken);

    Task UpdateGuestCapacityAsync(UniversityExamGuestCapacity guestCapacity, CancellationToken cancellationToken);

    Task<int> GetTotalGuestCapacityAsync(Guid universityExamCapacityId, CancellationToken cancellationToken);

    /// <summary>
    /// آمار ثبت‌نام و مجموع ظرفیت مهمان چند ظرفیت دانشگاه (بدون N+1)
    /// </summary>
    Task<Dictionary<Guid, UniversityExamCapacityUsage>> GetCapacityUsageAsync(int universityId, IReadOnlyCollection<Guid> capacityIds, CancellationToken cancellationToken);

    /// <summary>
    /// تعداد ثبت‌نام‌های هر ظرفیت دانشگاه مهمان
    /// </summary>
    Task<Dictionary<Guid, int>> GetGuestRegistrationCountsAsync(IReadOnlyCollection<Guid> guestCapacityIds, CancellationToken cancellationToken);
}

public sealed record UniversityExamCapacityUsage(int RegisteredCount, int OwnRegisteredCount, int GuestRegisteredCount, int TotalGuestCapacity);
