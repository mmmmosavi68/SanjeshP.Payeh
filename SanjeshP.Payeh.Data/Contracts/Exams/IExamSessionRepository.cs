
using SanjeshP.Payeh.Entities.Exam;

namespace SanjeshP.Payeh.Data.Contracts.Exams;

public interface IExamSessionRepository
{
    Task<List<ExamSession>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken);

    Task<ExamSession?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task AddAsync(
        ExamSession session,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        ExamSession session,
        CancellationToken cancellationToken);

    Task<bool> HasExamsAsync(
        Guid sessionId,
        CancellationToken cancellationToken);
}
