using SanjeshP.Payeh.Entities.Exam;

namespace SanjeshP.Payeh.Services.Exams;

public interface IExamService
{
    Task<List<Exam>> GetAllAsync(
        string? search,
        Guid? examSessionId,
        int? examTypeId,
        int? examModeId,
        bool? isActive,
        CancellationToken cancellationToken);

    Task<Exam> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Exam> CreateAsync(
        Exam exam,
        Guid createdBy,
        CancellationToken cancellationToken);

    Task<Exam> UpdateAsync(
        Guid id,
        Exam updatedExam,
        CancellationToken cancellationToken);

    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken);
}