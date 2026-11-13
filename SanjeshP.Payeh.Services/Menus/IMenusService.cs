
using SanjeshP.Payeh.Entities.Menu;

namespace SanjeshP.Payeh.Services.Menus;

public interface IMenusService
{
    Task<List<Menu>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<Menu> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<Menu> CreateAsync(
        Menu menu,
        Guid createdBy,
        CancellationToken cancellationToken);

    Task<Menu> UpdateAsync(
        Guid id,
        Menu updatedMenu,
        CancellationToken cancellationToken);

    Task SoftDeleteAsync(
        Guid id,
        CancellationToken cancellationToken);
}
