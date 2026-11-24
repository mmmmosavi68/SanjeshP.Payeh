
using SanjeshP.Payeh.Entities.Menu;

namespace SanjeshP.Payeh.Data.Contracts.Menus;

public interface IMenuRepository
{
    Task<List<Menu>> GetAllAsync(CancellationToken cancellationToken);

    Task<Menu?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<bool> ParentExistsAsync(
        Guid parentId,
        CancellationToken cancellationToken);

    Task<bool> HasNonDeletedChildrenAsync(
        Guid menuId,
        CancellationToken cancellationToken);

    Task AddAsync(
        Menu menu,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        Menu menu,
        CancellationToken cancellationToken);

    Task SoftDeleteAsync(
        Menu menu,
        CancellationToken cancellationToken);

    Task<PagedItems<TDto>> GetPageAsync<TDto>(int? page, int? pageSize, Func<IQueryable<Menu>, IQueryable<TDto>> project, CancellationToken cancellationToken);
}
