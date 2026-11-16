using SanjeshP.Payeh.Entities.Exam;

namespace SanjeshP.Payeh.Services.Exams;

public interface IExamRegistrationService
{
    Task<ExamRegistration> CreateAsync(
        ExamRegistration registration,
        IReadOnlyCollection<ExamRegistrationCustomFieldValue> customFieldValues,
        Guid createdBy,
        CancellationToken cancellationToken);

    Task<ExamRegistration> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<List<ExamRegistration>> GetByExamAsync(
        Guid examId,
        CancellationToken cancellationToken);

    Task SoftDeleteAsync(
        Guid id,
        CancellationToken cancellationToken);
}
