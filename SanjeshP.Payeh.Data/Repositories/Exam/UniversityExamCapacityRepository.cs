
using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Data.Contracts.Exams;
using SanjeshP.Payeh.Entities.Exam;
using SanjeshP.Payeh.Entities.University;
using SanjeshP.Payeh.Entities.User;

namespace SanjeshP.Payeh.Data.Repositories.Exams;

public sealed class UniversityExamCapacityRepository
    : IUniversityExamCapacityRepository, IScopedDependency
{
    private readonly ApplicationDbContext _dbContext;

    public UniversityExamCapacityRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<University?> GetActiveUniversityAsync(
        int universityId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Set<University>()
            .FirstOrDefaultAsync(
                x => x.Id == universityId &&
                     x.IsActive &&
                     !x.IsDeleted,
                cancellationToken);
    }

    public async Task<List<Exam>> GetActiveExamsForUniversityAsync(
        int universityId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Set<Exam>()
            .AsNoTracking()
            .Include(x => x.ExamSession)
            .Include(x => x.ExamType)
            .Include(x => x.ExamMode)
            .Include(x => x.UniversityExamCapacities
                .Where(c =>
                    c.UniversityId == universityId &&
                    c.IsActive &&
                    !c.IsDeleted))
            .Where(x =>
                x.IsActive &&
                !x.IsDeleted &&
                x.ExamSession.IsActive &&
                !x.ExamSession.IsDeleted)
            .OrderByDescending(x => x.ExamSession.ExamDate)
            .ThenBy(x => x.ExamSession.StartTime)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    public Task<UniversityExamCapacity?> GetByUniversityAndExamAsync(
        int universityId,
        Guid examId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Set<UniversityExamCapacity>()
            .Include(x => x.Exam)
                .ThenInclude(x => x.ExamSession)
            .Include(x => x.Exam)
                .ThenInclude(x => x.ExamMode)
            .FirstOrDefaultAsync(
                x => x.UniversityId == universityId &&
                     x.ExamId == examId,
                cancellationToken);
    }

    public Task<int> GetAllocatedCapacityForElectronicExamsInSessionAsync(
        int universityId,
        Guid examSessionId,
        Guid? excludedExamId,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<UniversityExamCapacity>()
            .Where(x =>
                x.UniversityId == universityId &&
                x.IsActive &&
                !x.IsDeleted &&
                x.Exam.IsActive &&
                !x.Exam.IsDeleted &&
                x.Exam.ExamSession.IsActive &&
                !x.Exam.ExamSession.IsDeleted &&
                x.Exam.ExamSessionId == examSessionId &&
                x.Exam.ExamMode.Title == "الکترونیکی");

        if (excludedExamId.HasValue)
        {
            query = query.Where(x =>
                x.ExamId != excludedExamId.Value);
        }

        return query.SumAsync(
            x => x.AllocatedCapacity ?? 0,
            cancellationToken);
    }

    public Task<int> CountRegistrationsAsync(
        Guid capacityId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Set<ExamRegistration>()
            .CountAsync(
                x => x.UniversityExamCapacityId == capacityId &&
                     !x.IsDeleted,
                cancellationToken);
    }

    public Task<int> CountOwnRegistrationsAsync(
        Guid capacityId,
        int universityId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Set<ExamRegistration>()
            .CountAsync(
                x => x.UniversityExamCapacityId == capacityId &&
                     x.UniversityId == universityId &&
                     !x.IsDeleted,
                cancellationToken);
    }

    public async Task AddAsync(
        UniversityExamCapacity capacity,
        CancellationToken cancellationToken)
    {
        await _dbContext.Set<UniversityExamCapacity>()
            .AddAsync(capacity, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        UniversityExamCapacity capacity,
        CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<University>> GetAccessibleUniversitiesAsync(
    Guid userId,
    CancellationToken cancellationToken)
    {
        return await _dbContext.Set<UserUniversity>()
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.IsActive &&
                !x.IsDeleted &&
                x.User.IsActive &&
                !x.User.IsDeleted &&
                x.University.IsActive &&
                !x.University.IsDeleted)
            .Include(x => x.University)
                .ThenInclude(x => x.UniversityType)
            .Include(x => x.University)
                .ThenInclude(x => x.ParentUniversity)
            .OrderBy(x => x.University.Title)
            .Select(x => x.University)
            .ToListAsync(cancellationToken);
    }

    public async Task<University?> GetAccessibleUniversityAsync(
    Guid userId,
    int universityId,
    CancellationToken cancellationToken)
    {
        return await _dbContext.Set<UserUniversity>()
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.UniversityId == universityId &&
                x.IsActive &&
                !x.IsDeleted &&
                x.User.IsActive &&
                !x.User.IsDeleted &&
                x.University.IsActive &&
                !x.University.IsDeleted)
            .Select(x => x.University)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Exam>> GetDefinedExamsForUniversityAsync(
    int universityId,
    CancellationToken cancellationToken)
    {
        return await _dbContext.Set<Exam>()
            .AsNoTracking()
            .Include(x => x.ExamSession)
            .Include(x => x.ExamType)
            .Include(x => x.ExamMode)
            .Include(x => x.UniversityExamCapacities
                .Where(c =>
                    c.UniversityId == universityId &&
                    !c.IsDeleted))
            .Include(x => x.UniversityExamCapacities
                .Where(c =>
                    c.UniversityId == universityId &&
                    c.IsActive &&
                    !c.IsDeleted))
            .Where(x =>
                x.IsActive &&
                !x.IsDeleted &&
                x.ExamSession.IsActive &&
                !x.ExamSession.IsDeleted &&
                x.UniversityExamCapacities.Any(c =>
                    c.UniversityId == universityId &&
                    c.IsActive &&
                    !c.IsDeleted))
            .OrderByDescending(x => x.ExamSession.ExamDate)
            .ThenBy(x => x.ExamSession.StartTime)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }


    public async Task<List<UniversityExamGuestCapacity>> GetGuestCapacitiesAsync(
    Guid universityExamCapacityId,
    CancellationToken cancellationToken)
    {
        return await _dbContext.Set<UniversityExamGuestCapacity>()
            .AsNoTracking()
            .Include(x => x.GuestUniversity)
            .Where(x =>
                x.UniversityExamCapacityId == universityExamCapacityId &&
                x.IsActive &&
                !x.IsDeleted &&
                x.GuestUniversity.IsActive &&
                !x.GuestUniversity.IsDeleted)
            .OrderBy(x => x.GuestUniversity.Title)
            .ToListAsync(cancellationToken);
    }

    public Task<UniversityExamGuestCapacity?> GetGuestCapacityByUniversityAsync(
    Guid universityExamCapacityId,
    int guestUniversityId,
    CancellationToken cancellationToken)
    {
        return _dbContext.Set<UniversityExamGuestCapacity>()
            .FirstOrDefaultAsync(
                x => x.UniversityExamCapacityId == universityExamCapacityId &&
                     x.GuestUniversityId == guestUniversityId,
                cancellationToken);
    }

    public Task<int> CountGuestRegistrationsAsync(
    Guid guestCapacityId,
    CancellationToken cancellationToken)
    {
        return _dbContext.Set<ExamRegistration>()
            .CountAsync(
                x => x.UniversityExamGuestCapacityId == guestCapacityId &&
                     !x.IsDeleted,
                cancellationToken);
    }

    public async Task AddGuestCapacityAsync(
    UniversityExamGuestCapacity guestCapacity,
    CancellationToken cancellationToken)
    {
        await _dbContext.Set<UniversityExamGuestCapacity>()
            .AddAsync(guestCapacity, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateGuestCapacityAsync(
    UniversityExamGuestCapacity guestCapacity,
    CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<int> GetTotalGuestCapacityAsync(
    Guid universityExamCapacityId,
    CancellationToken cancellationToken)
    {
        return _dbContext.Set<UniversityExamGuestCapacity>()
            .Where(x =>
                x.UniversityExamCapacityId == universityExamCapacityId &&
                x.IsActive &&
                !x.IsDeleted &&
                x.GuestUniversity.IsActive &&
                !x.GuestUniversity.IsDeleted)
            .SumAsync(x => x.Capacity, cancellationToken);
    }
}
