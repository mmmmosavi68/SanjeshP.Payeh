
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

    Task<PagedItems<TDto>> GetPageAsync<TDto>(string? search, bool? isActive, int? page, int? pageSize, Func<IQueryable<ExamSession>, IQueryable<TDto>> project, CancellationToken cancellationToken);
}
