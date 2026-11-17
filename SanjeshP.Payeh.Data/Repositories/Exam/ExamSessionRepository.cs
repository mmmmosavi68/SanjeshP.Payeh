
using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Data.Contracts.Exams;
using SanjeshP.Payeh.Entities.Exam;

namespace SanjeshP.Payeh.Data.Repositories.Exams;

public sealed class ExamSessionRepository
    : IExamSessionRepository, IScopedDependency
{
    private readonly ApplicationDbContext _dbContext;

    public ExamSessionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ExamSession>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<ExamSession>()
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(x => x.Title.Contains(search));
        }

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        return await query
            .OrderByDescending(x => x.ExamDate)
            .ThenBy(x => x.StartTime)
            .ToListAsync(cancellationToken);
    }

    public Task<ExamSession?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return _dbContext.Set<ExamSession>()
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task AddAsync(
        ExamSession session,
        CancellationToken cancellationToken)
    {
        await _dbContext.Set<ExamSession>().AddAsync(
            session,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        ExamSession session,
        CancellationToken cancellationToken)
    {
        _dbContext.Set<ExamSession>().Update(session);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> HasExamsAsync(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Set<Exam>()
            .AnyAsync(
                x => x.ExamSessionId == sessionId,
                cancellationToken);
    }
}
