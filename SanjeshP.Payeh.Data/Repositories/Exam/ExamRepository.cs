using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Data.Contracts.Exams;
using SanjeshP.Payeh.Entities.Exam;
using SanjeshP.Payeh.Entities.Menu;

namespace SanjeshP.Payeh.Data.Repositories.Exams;

public sealed class ExamRepository
    : EFRepository<Exam>, IExamRepository, IScopedDependency
{
    public ExamRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<List<Exam>> GetAllAsync(string? search, Guid? examSessionId, int? examTypeId, int? examModeId, bool? isActive, CancellationToken cancellationToken)
    {
        var query = TableNoTracking
            .Include(x => x.ExamSession)
            .Include(x => x.ExamType)
            .Include(x => x.ExamMode)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(x => x.Title.Contains(search));
        }

        if (examSessionId.HasValue)
            query = query.Where(x => x.ExamSessionId == examSessionId.Value);

        if (examTypeId.HasValue)
            query = query.Where(x => x.ExamTypeId == examTypeId.Value);

        if (examModeId.HasValue)
            query = query.Where(x => x.ExamModeId == examModeId.Value);

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        return await query
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public Task<Exam?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Table
            .Include(x => x.ExamSession)
            .Include(x => x.ExamType)
            .Include(x => x.ExamMode)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> SessionExistsAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        return DbContext.Set<ExamSession>()
            .AnyAsync(
                x => x.Id == sessionId && x.IsActive,
                cancellationToken);
    }

    public Task<bool> CodeExistsInGroupAsync(int codeId, string groupTitle, CancellationToken cancellationToken)
    {
        return DbContext.Set<Code>()
            .AnyAsync(
                x => x.Id == codeId &&
                     x.IsActive &&
                     x.Parent != null &&
                     x.Parent.Title == groupTitle &&
                     x.Parent.IsActive,
                cancellationToken);
    }

    public async Task<bool> HasDependenciesAsync(Guid examId, CancellationToken cancellationToken)
    {
        if (await DbContext.Set<ExamRegistration>()
            .AnyAsync(x => x.ExamId == examId, cancellationToken))
            return true;

        if (await DbContext.Set<UniversityExamCapacity>()
            .AnyAsync(x => x.ExamId == examId, cancellationToken))
            return true;

        return await DbContext.Set<ExamCustomField>()
            .AnyAsync(x => x.ExamId == examId, cancellationToken);
    }

    public Task AddAsync(Exam exam, CancellationToken cancellationToken)
    {
        return base.AddAsync(exam, cancellationToken);
    }

    public Task UpdateAsync(Exam exam, CancellationToken cancellationToken)
    {
        return base.UpdateAsync(exam, cancellationToken);
    }

    public Task<PagedItems<TDto>> GetPageAsync<TDto>(string? search, Guid? examSessionId, int? examTypeId, int? examTypePrimaryCode, int? examTypeSubCode, int? examModeId, int? examModePrimaryCode, int? examModeSubCode, bool? isActive, int? page, int? pageSize, Func<IQueryable<Exam>, IQueryable<TDto>> project, CancellationToken cancellationToken)
    {
        var query = TableNoTracking.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Title.Contains(term));
        }

        if (examSessionId.HasValue) query = query.Where(x => x.ExamSessionId == examSessionId.Value);
        if (examTypeId.HasValue) query = query.Where(x => x.ExamTypeId == examTypeId.Value);
        if (examTypePrimaryCode.HasValue) query = query.Where(x => x.ExamType!.PrimaryCode == examTypePrimaryCode.Value);
        if (examTypeSubCode.HasValue) query = query.Where(x => x.ExamType!.SubCode == examTypeSubCode.Value);
        if (examModeId.HasValue) query = query.Where(x => x.ExamModeId == examModeId.Value);
        if (examModePrimaryCode.HasValue) query = query.Where(x => x.ExamMode!.PrimaryCode == examModePrimaryCode.Value);
        if (examModeSubCode.HasValue) query = query.Where(x => x.ExamMode!.SubCode == examModeSubCode.Value);
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

        return query.OrderByDescending(x => x.CreatedDate).ThenBy(x => x.Id).ToPagedAsync(project, page, pageSize, cancellationToken);
    }
}