
using SanjeshP.Payeh.Entities.University;

namespace SanjeshP.Payeh.Services.Universities;

public interface IUniversityService
{
    Task<List<University>> GetAllAsync(
        string? search,
        int? universityTypeId,
        int? region,
        int? parentUniversityId,
        bool? isActive,
        CancellationToken cancellationToken);

    Task<University> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<University> CreateAsync(
        University university,
        Guid createdBy,
        CancellationToken cancellationToken);

    Task<University> UpdateAsync(
        int id,
        University updatedUniversity,
        CancellationToken cancellationToken);

    Task SoftDeleteAsync(
        int id,
        CancellationToken cancellationToken);

    Task<PagedItems<TDto>> GetPageAsync<TDto>(string? search, int? universityTypeId, int? universityTypePrimaryCode, int? universityTypeSubCode, int? region, int? parentUniversityId, bool? isActive, int? page, int? pageSize, Func<IQueryable<University>, IQueryable<TDto>> project, CancellationToken cancellationToken);
}
