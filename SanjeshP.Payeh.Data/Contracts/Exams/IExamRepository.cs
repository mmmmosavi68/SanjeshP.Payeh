using SanjeshP.Payeh.Entities.Exam;

namespace SanjeshP.Payeh.Data.Contracts.Exams;

public interface IExamRepository
{
    Task<List<Exam>> GetAllAsync(
        string? search,
        Guid? examSessionId,
        int? examTypeId,
        int? examModeId,
        bool? isActive,
        CancellationToken cancellationToken);

    Task<Exam?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<bool> SessionExistsAsync(
        Guid sessionId,
        CancellationToken cancellationToken);

    Task<bool> CodeExistsInGroupAsync(
        int codeId,
        string groupTitle,
        CancellationToken cancellationToken);

    Task<bool> HasDependenciesAsync(
        Guid examId,
        CancellationToken cancellationToken);

    Task AddAsync(Exam exam, CancellationToken cancellationToken);
    Task UpdateAsync(Exam exam, CancellationToken cancellationToken);
}