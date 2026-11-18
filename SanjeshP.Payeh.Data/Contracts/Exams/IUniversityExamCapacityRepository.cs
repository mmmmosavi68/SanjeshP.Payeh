
using SanjeshP.Payeh.Entities.Exam;
using SanjeshP.Payeh.Entities.University;

namespace SanjeshP.Payeh.Data.Contracts.Exams;

public interface IUniversityExamCapacityRepository
{
    Task<University?> GetActiveUniversityAsync(
        int universityId,
        CancellationToken cancellationToken);

    Task<List<Exam>> GetActiveExamsForUniversityAsync(
        int universityId,
        CancellationToken cancellationToken);

    Task<UniversityExamCapacity?> GetByUniversityAndExamAsync(
        int universityId,
        Guid examId,
        CancellationToken cancellationToken);

    Task<int> GetAllocatedCapacityForElectronicExamsInSessionAsync(
        int universityId,
        Guid examSessionId,
        Guid? excludedExamId,
        CancellationToken cancellationToken);

    Task<int> CountRegistrationsAsync(
        Guid capacityId,
        CancellationToken cancellationToken);

    Task<int> CountOwnRegistrationsAsync(
        Guid capacityId,
        int universityId,
        CancellationToken cancellationToken);

    Task AddAsync(
        UniversityExamCapacity capacity,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        UniversityExamCapacity capacity,
        CancellationToken cancellationToken);

    Task<List<University>> GetAccessibleUniversitiesAsync(
    Guid userId,
    CancellationToken cancellationToken);

    Task<University?> GetAccessibleUniversityAsync(
    Guid userId,
    int universityId,
    CancellationToken cancellationToken);

    Task<List<Exam>> GetDefinedExamsForUniversityAsync(
    int universityId,
    CancellationToken cancellationToken);
}
