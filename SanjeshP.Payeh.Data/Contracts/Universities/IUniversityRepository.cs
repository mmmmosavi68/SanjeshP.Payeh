
using SanjeshP.Payeh.Entities.University;

namespace SanjeshP.Payeh.Data.Contracts.Universities;

public interface IUniversityRepository
{
    Task<List<University>> GetAllAsync(
        string? search,
        int? universityTypeId,
        int? region,
        int? parentUniversityId,
        bool? isActive,
        CancellationToken cancellationToken);

    Task<University?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<bool> UniversityTypeExistsAsync(
        int universityTypeId,
        CancellationToken cancellationToken);

    Task<bool> ParentExistsAsync(
        int parentUniversityId,
        CancellationToken cancellationToken);

    Task<bool> HasChildrenAsync(
        int universityId,
        CancellationToken cancellationToken);

    Task<bool> HasDependenciesAsync(
        int universityId,
        CancellationToken cancellationToken);

    Task AddAsync(
        University university,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        University university,
        CancellationToken cancellationToken);
}
