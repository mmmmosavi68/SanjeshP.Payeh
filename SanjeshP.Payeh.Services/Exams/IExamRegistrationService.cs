
using SanjeshP.Payeh.Entities.Exam;
using SanjeshP.Payeh.Entities.University;

namespace SanjeshP.Payeh.Services.Exams;

public interface IExamRegistrationService
{
    Task<List<University>> GetAccessibleUniversitiesAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<List<Exam>> GetAvailableExamsAsync(
        int universityId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<ExamRegistration> CreateAsync(
        ExamRegistration registration,
        IReadOnlyCollection<ExamRegistrationCustomFieldValue> customFieldValues,
        Guid createdBy,
        CancellationToken cancellationToken);

    Task<ExamRegistration> UpdateAsync(
        Guid id,
        ExamRegistration updatedRegistration,
        IReadOnlyCollection<ExamRegistrationCustomFieldValue> customFieldValues,
        Guid updatedBy,
        CancellationToken cancellationToken);

    Task<ExamRegistration> GetByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken);

    Task<List<ExamRegistration>> GetByExamAsync(
        Guid examId,
        int universityId,
        Guid userId,
        CancellationToken cancellationToken);

    Task SoftDeleteAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken);

    Task<(List<UniversityExamCapacity> Capacities, List<ExamCustomField> CustomFields)>
    GetRegistrationOptionsAsync(
        Guid examId,
        int universityId,
        Guid userId,
        CancellationToken cancellationToken);
}
