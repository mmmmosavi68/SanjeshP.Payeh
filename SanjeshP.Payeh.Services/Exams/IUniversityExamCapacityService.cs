
using SanjeshP.Payeh.Entities.Exam;
using SanjeshP.Payeh.Entities.University;

namespace SanjeshP.Payeh.Services.Exams;

public interface IUniversityExamCapacityService
{
    Task<List<Exam>> GetActiveExamsForUniversityAsync(
    Guid userId,
    int universityId,
    CancellationToken cancellationToken);

    Task<UniversityExamCapacity> SaveAsync(
        int universityId,
        Guid examId,
        int allocatedCapacity,
        int ownCapacity,
        Guid createdBy,
        CancellationToken cancellationToken);

    Task<List<University>> GetAccessibleUniversitiesAsync(
    Guid userId,
    CancellationToken cancellationToken);
    Task<List<Exam>> GetDefinedExamsForUniversityAsync(
    Guid userId,
    int universityId,
    CancellationToken cancellationToken);

}
