
using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Data.Contracts.Universities;
using SanjeshP.Payeh.Entities.Exam;
using SanjeshP.Payeh.Entities.Menu;
using SanjeshP.Payeh.Entities.University;

namespace SanjeshP.Payeh.Data.Repositories.Universities;

public sealed class UniversityRepository
    : EFRepository<University>, IUniversityRepository, IScopedDependency
{
    public UniversityRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<List<University>> GetAllAsync(
        string? search,
        int? universityTypeId,
        int? region,
        int? parentUniversityId,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var query = TableNoTracking
            .Include(x => x.UniversityType)
            .Include(x => x.ParentUniversity)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x => x.Title.Contains(search));
        }

        if (universityTypeId.HasValue)
            query = query.Where(x => x.UniversityTypeId == universityTypeId.Value);

        if (region.HasValue)
            query = query.Where(x => x.Region == region.Value);

        if (parentUniversityId.HasValue)
            query = query.Where(x => x.ParentUniversityId == parentUniversityId.Value);

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        return await query
            .OrderBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<University?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await Table
            .Include(x => x.UniversityType)
            .Include(x => x.ParentUniversity)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public Task<bool> UniversityTypeExistsAsync(
        int universityTypeId,
        CancellationToken cancellationToken)
    {
        return DbContext.Set<Code>()
            .AnyAsync(
                x => x.Id == universityTypeId && x.IsActive,
                cancellationToken);
    }

    public Task<bool> ParentExistsAsync(
        int parentUniversityId,
        CancellationToken cancellationToken)
    {
        return TableNoTracking.AnyAsync(
            x => x.Id == parentUniversityId && x.IsActive,
            cancellationToken);
    }

    public Task<bool> HasChildrenAsync(
        int universityId,
        CancellationToken cancellationToken)
    {
        return TableNoTracking.AnyAsync(
            x => x.ParentUniversityId == universityId,
            cancellationToken);
    }

    public async Task<bool> HasDependenciesAsync(
        int universityId,
        CancellationToken cancellationToken)
    {
        var hasCapacity = await DbContext.Set<UniversityExamCapacity>()
            .AnyAsync(
                x => x.UniversityId == universityId,
                cancellationToken);

        if (hasCapacity)
            return true;

        var hasGuestCapacity = await DbContext.Set<UniversityExamGuestCapacity>()
            .AnyAsync(
                x => x.GuestUniversityId == universityId,
                cancellationToken);

        if (hasGuestCapacity)
            return true;

        return await DbContext.Set<ExamRegistration>()
            .AnyAsync(
                x => x.UniversityId == universityId ||
                     x.ExamVenueUniversityId == universityId,
                cancellationToken);
    }

    public Task AddAsync(
        University university,
        CancellationToken cancellationToken)
    {
        return base.AddAsync(university, cancellationToken);
    }

    public Task UpdateAsync(
        University university,
        CancellationToken cancellationToken)
    {
        return base.UpdateAsync(university, cancellationToken);
    }

    public Task<PagedItems<TDto>> GetPageAsync<TDto>(string? search, int? universityTypeId, int? universityTypePrimaryCode, int? universityTypeSubCode, int? region, int? parentUniversityId, bool? isActive, int? page, int? pageSize, Func<IQueryable<University>, IQueryable<TDto>> project, CancellationToken cancellationToken)
    {
        var query = TableNoTracking.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Title.Contains(term));
        }

        if (universityTypeId.HasValue) query = query.Where(x => x.UniversityTypeId == universityTypeId.Value);
        if (universityTypePrimaryCode.HasValue) query = query.Where(x => x.UniversityType!.PrimaryCode == universityTypePrimaryCode.Value);
        if (universityTypeSubCode.HasValue) query = query.Where(x => x.UniversityType!.SubCode == universityTypeSubCode.Value);
        if (region.HasValue) query = query.Where(x => x.Region == region.Value);
        if (parentUniversityId.HasValue) query = query.Where(x => x.ParentUniversityId == parentUniversityId.Value);
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

        return query.OrderBy(x => x.Title).ThenBy(x => x.Id).ToPagedAsync(project, page, pageSize, cancellationToken);
    }
}
