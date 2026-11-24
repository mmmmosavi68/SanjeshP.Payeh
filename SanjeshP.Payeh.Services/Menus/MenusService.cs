
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.Data.Contracts.Menus;
using SanjeshP.Payeh.Entities.Menu;

namespace SanjeshP.Payeh.Services.Menus;

public class MenusService : IMenusService, IScopedDependency
{
    private readonly IMenuRepository _menuRepository;
    private readonly IPermissionService _permissionService;

    public MenusService(
        IMenuRepository menuRepository,
        IPermissionService permissionService)
    {
        _menuRepository = menuRepository;
        _permissionService = permissionService;
    }

    public Task<List<Menu>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return _menuRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Menu> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var menu = await _menuRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (menu is null)
            throw new NotFoundException("منوی موردنظر پیدا نشد.");

        return menu;
    }

    public async Task<Menu> CreateAsync(
        Menu menu,
        Guid createdBy,
        CancellationToken cancellationToken)
    {
        ValidateMenuTitle(menu.MenuTitle);

        await ValidateParentAsync(
            menu.ParentId,
            null,
            cancellationToken);

        menu.MenuTitle = menu.MenuTitle.Trim();
        menu.CreatedBy = createdBy;
        menu.CreatedDate = DateTime.UtcNow;
        menu.UpdatedDate = null;
        menu.IsDeleted = false;

        await _menuRepository.AddAsync(
            menu,
            cancellationToken);

        _permissionService.InvalidateAll();

        return menu;
    }

    public async Task<Menu> UpdateAsync(
        Guid id,
        Menu updatedMenu,
        CancellationToken cancellationToken)
    {
        var menu = await GetByIdAsync(id, cancellationToken);

        ValidateMenuTitle(updatedMenu.MenuTitle);

        await ValidateParentAsync(
            updatedMenu.ParentId,
            id,
            cancellationToken);

        // فقط فیلدهای قابل ویرایش تغییر می‌کنند.
        // شناسه، اطلاعات ایجادکننده و وضعیت حذف حفظ می‌شوند.
        menu.MenuTitle = updatedMenu.MenuTitle.Trim();
        menu.ParentId = updatedMenu.ParentId;
        menu.PageCode = updatedMenu.PageCode;
        menu.Path = updatedMenu.Path ?? string.Empty;
        menu.Link = updatedMenu.Link ?? string.Empty;
        menu.version = updatedMenu.version ?? string.Empty;
        menu.ControllerName = updatedMenu.ControllerName ?? string.Empty;
        menu.ActionName = updatedMenu.ActionName ?? string.Empty;
        menu.ShowMenu = updatedMenu.ShowMenu;
        menu.CssClass = updatedMenu.CssClass ?? string.Empty;
        menu.Icon = updatedMenu.Icon ?? string.Empty;
        menu.IsActive = updatedMenu.IsActive;
        menu.UpdatedDate = DateTime.UtcNow;

        await _menuRepository.UpdateAsync(
            menu,
            cancellationToken);

        _permissionService.InvalidateAll();

        return menu;
    }

    public async Task SoftDeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var menu = await GetByIdAsync(id, cancellationToken);

        var hasChildren = await _menuRepository.HasNonDeletedChildrenAsync(
            id,
            cancellationToken);

        if (hasChildren)
        {
            throw new BadRequestException(
                "این منو دارای زیرمنو است و ابتدا باید زیرمنوهای آن حذف شوند.");
        }

        await _menuRepository.SoftDeleteAsync(
            menu,
            cancellationToken);

        _permissionService.InvalidateAll();
    }

    private async Task ValidateParentAsync(
        Guid? parentId,
        Guid? currentMenuId,
        CancellationToken cancellationToken)
    {
        if (!parentId.HasValue)
            return;

        if (currentMenuId.HasValue && parentId.Value == currentMenuId.Value)
        {
            throw new BadRequestException(
                "یک منو نمی‌تواند والد خودش باشد.");
        }

        var parentExists = await _menuRepository.ParentExistsAsync(
            parentId.Value,
            cancellationToken);

        if (!parentExists)
        {
            throw new BadRequestException(
                "منوی والد انتخاب‌شده معتبر نیست یا حذف شده است.");
        }

        // جلوگیری از ایجاد چرخه در ساختار درختی
        if (currentMenuId.HasValue)
        {
            var menus = await _menuRepository.GetAllAsync(
                cancellationToken);

            var menusById = menus.ToDictionary(m => m.Id);

            var visited = new HashSet<Guid>();
            var ancestorId = parentId.Value;

            while (visited.Add(ancestorId))
            {
                if (ancestorId == currentMenuId.Value)
                {
                    throw new BadRequestException(
                        "امکان انتخاب یکی از زیرمنوهای این منو به‌عنوان والد وجود ندارد.");
                }

                if (!menusById.TryGetValue(ancestorId, out var ancestor))
                    break;

                if (!ancestor.ParentId.HasValue)
                    break;

                ancestorId = ancestor.ParentId.Value;
            }
        }
    }

    private static void ValidateMenuTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new BadRequestException(
                "عنوان منو الزامی است.");
        }
    }

    public Task<PagedItems<TDto>> GetPageAsync<TDto>(int? page, int? pageSize, Func<IQueryable<Menu>, IQueryable<TDto>> project, CancellationToken cancellationToken)
    {
        return _menuRepository.GetPageAsync(page, pageSize, project, cancellationToken);
    }
}
