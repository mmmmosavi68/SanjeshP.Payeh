
using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Data.Contracts.Menus;
using SanjeshP.Payeh.Entities.Menu;

namespace SanjeshP.Payeh.Data.Repositories.Menus;

public class MenuRepository : EFRepository<Menu>, IMenuRepository, IScopedDependency
{
    public MenuRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<List<Menu>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await TableNoTracking
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.PageCode)
            .ThenBy(m => m.MenuTitle)
            .ToListAsync(cancellationToken);
    }

    public async Task<Menu?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await Table
            .FirstOrDefaultAsync(
                m => m.Id == id && !m.IsDeleted,
                cancellationToken);
    }

    public async Task<bool> ParentExistsAsync(
        Guid parentId,
        CancellationToken cancellationToken)
    {
        return await TableNoTracking
            .AnyAsync(
                m => m.Id == parentId && !m.IsDeleted,
                cancellationToken);
    }

    public async Task<bool> HasNonDeletedChildrenAsync(
        Guid menuId,
        CancellationToken cancellationToken)
    {
        return await TableNoTracking
            .AnyAsync(
                m => m.ParentId == menuId && !m.IsDeleted,
                cancellationToken);
    }

    public Task AddAsync(
        Menu menu,
        CancellationToken cancellationToken)
    {
        return base.AddAsync(menu, cancellationToken);
    }

    public Task UpdateAsync(
        Menu menu,
        CancellationToken cancellationToken)
    {
        return base.UpdateAsync(menu, cancellationToken);
    }

    public async Task SoftDeleteAsync(
        Menu menu,
        CancellationToken cancellationToken)
    {
        menu.IsDeleted = true;
        menu.UpdatedDate = DateTime.UtcNow;

        await base.UpdateAsync(menu, cancellationToken);
    }

    public Task<PagedItems<TDto>> GetPageAsync<TDto>(int? page, int? pageSize, Func<IQueryable<Menu>, IQueryable<TDto>> project, CancellationToken cancellationToken)
    {
        return TableNoTracking.Where(m => !m.IsDeleted).OrderBy(m => m.PageCode).ThenBy(m => m.MenuTitle).ThenBy(m => m.Id).ToPagedAsync(project, page, pageSize, cancellationToken);
    }
}
