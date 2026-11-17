
using SanjeshP.Payeh.Entities.Exam;

namespace SanjeshP.Payeh.Services.Exams;

public interface IExamSessionService
{
    Task<List<ExamSession>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken);

    Task<ExamSession> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<ExamSession> CreateAsync(
        ExamSession session,
        Guid createdBy,
        CancellationToken cancellationToken);

    Task<ExamSession> UpdateAsync(
        Guid id,
        ExamSession updatedSession,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken);
}
