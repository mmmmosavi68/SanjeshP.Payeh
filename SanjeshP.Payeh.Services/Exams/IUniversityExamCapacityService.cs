
using SanjeshP.Payeh.Entities.Exam;

namespace SanjeshP.Payeh.Services.Exams;

public interface IUniversityExamCapacityService
{
    Task<List<Exam>> GetActiveExamsForUniversityAsync(
        int universityId,
        CancellationToken cancellationToken);

    Task<UniversityExamCapacity> SaveAsync(
        int universityId,
        Guid examId,
        int allocatedCapacity,
        int ownCapacity,
        Guid createdBy,
        CancellationToken cancellationToken);
}
