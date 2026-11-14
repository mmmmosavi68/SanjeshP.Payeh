
using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.Data;
using SanjeshP.Payeh.Entities.Group;
using SanjeshP.Payeh.Entities.Menu;
using SanjeshP.Payeh.Entities.User;
using SanjeshP.Payeh.Services.Groups;
using SanjeshP.Payeh.Services.Permissions;

using UserGroup = SanjeshP.Payeh.Entities.Group.Group;

namespace SanjeshP.Payeh.Data.Repositories;

public sealed class GroupService : IGroupService, IScopedDependency
{
    private readonly ApplicationDbContext _db;
    private readonly IPermissionService _permissionService;

    public GroupService(
        ApplicationDbContext db,
        IPermissionService permissionService)
    {
        _db = db;
        _permissionService = permissionService;
    }

    /// <summary>
    /// دریافت تمام گروه‌ها به همراه اعضای فعال آن‌ها
    /// </summary>
    public async Task<List<UserGroup>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _db.Set<UserGroup>()
            .AsNoTracking()
            .Include(x => x.GroupUsers.Where(
                u => u.IsActive && !u.IsDeleted))
            .OrderBy(x => x.GroupName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// دریافت اطلاعات یک گروه
    /// </summary>
    public async Task<UserGroup> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var group = await _db.Set<UserGroup>()
            .AsNoTracking()
            .Include(x => x.GroupUsers.Where(
                u => u.IsActive && !u.IsDeleted))
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        return group ?? throw new NotFoundException(
            "گروه مورد نظر یافت نشد.");
    }

    /// <summary>
    /// دریافت اعضای یک گروه به همراه اطلاعات پروفایل
    /// </summary>
    public async Task<List<GroupUsers>> GetUsersAsync(
        Guid groupId,
        CancellationToken cancellationToken)
    {
        var exists = await _db.Set<UserGroup>()
            .AnyAsync(
                x => x.Id == groupId,
                cancellationToken);

        if (!exists)
            throw new NotFoundException(
                "گروه مورد نظر یافت نشد.");

        return await _db.Set<GroupUsers>()
            .AsNoTracking()
            .Where(x =>
                x.GroupId == groupId &&
                x.IsActive &&
                !x.IsDeleted)
            .Include(x => x.User)
                .ThenInclude(x => x.UserProfile)
            .OrderBy(x => x.User.UserName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// ایجاد گروه جدید
    /// </summary>
    public async Task<UserGroup> CreateAsync(
        string groupName,
        bool isActive,
        Guid createdBy,
        CancellationToken cancellationToken)
    {
        groupName = groupName.Trim();

        if (string.IsNullOrWhiteSpace(groupName))
            throw new BadRequestException(
                "عنوان گروه الزامی است.");

        var normalizedName = groupName.ToUpperInvariant();

        var exists = await _db.Set<UserGroup>()
            .AnyAsync(
                x => x.GroupName.ToUpper() == normalizedName,
                cancellationToken);

        if (exists)
            throw new ConflictException(
                "گروهی با این عنوان قبلاً ثبت شده است.");

        var group = new UserGroup
        {
            GroupName = groupName,
            IsActive = isActive,
            CreatedBy = createdBy
        };

        await _db.Set<UserGroup>().AddAsync(
            group,
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            group.Id,
            cancellationToken);
    }

    /// <summary>
    /// ویرایش اطلاعات گروه
    /// </summary>
    public async Task<UserGroup> UpdateAsync(
        Guid id,
        string groupName,
        bool isActive,
        CancellationToken cancellationToken)
    {
        var group = await _db.Set<UserGroup>()
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (group is null)
            throw new NotFoundException(
                "گروه مورد نظر یافت نشد.");

        groupName = groupName.Trim();

        if (string.IsNullOrWhiteSpace(groupName))
            throw new BadRequestException(
                "عنوان گروه الزامی است.");

        var normalizedName = groupName.ToUpperInvariant();

        var exists = await _db.Set<UserGroup>()
            .AnyAsync(
                x => x.Id != id &&
                     x.GroupName.ToUpper() == normalizedName,
                cancellationToken);

        if (exists)
            throw new ConflictException(
                "گروهی با این عنوان قبلاً ثبت شده است.");

        var activeChanged = group.IsActive != isActive;

        group.GroupName = groupName;
        group.IsActive = isActive;
        group.UpdatedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        if (activeChanged)
            _permissionService.InvalidateAll();

        return await GetByIdAsync(
            id,
            cancellationToken);
    }

    /// <summary>
    /// حذف نرم گروه
    /// </summary>
    public async Task SoftDeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var group = await _db.Set<UserGroup>()
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (group is null)
            throw new NotFoundException(
                "گروه مورد نظر یافت نشد.");

        group.IsDeleted = true;
        group.IsActive = false;
        group.UpdatedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        // تغییر گروه روی دسترسی تمام اعضای آن اثر می‌گذارد.
        _permissionService.InvalidateAll();
    }

    /// <summary>
    /// افزودن کاربر به گروه
    /// </summary>
    public async Task AddUserAsync(
        Guid groupId,
        Guid userId,
        Guid createdBy,
        CancellationToken cancellationToken)
    {
        var group = await _db.Set<UserGroup>()
            .FirstOrDefaultAsync(
                x => x.Id == groupId,
                cancellationToken);

        if (group is null)
            throw new NotFoundException(
                "گروه مورد نظر یافت نشد.");

        if (!group.IsActive)
            throw new BadRequestException(
                "امکان افزودن کاربر به گروه غیرفعال وجود ندارد.");

        var userExists = await _db.Set<User>()
            .AnyAsync(
                x => x.Id == userId && x.IsActive,
                cancellationToken);

        if (!userExists)
            throw new NotFoundException(
                "کاربر مورد نظر یافت نشد یا غیرفعال است.");

        // بررسی عضویت فعلی یا عضویت حذف‌شده قبلی
        var membership = await _db.Set<GroupUsers>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                x => x.GroupId == groupId &&
                     x.UserId == userId,
                cancellationToken);

        if (membership is not null)
        {
            if (!membership.IsDeleted && membership.IsActive)
                throw new ConflictException(
                    "این کاربر در حال حاضر عضو گروه است.");

            // فعال‌سازی مجدد عضویت قبلی
            membership.IsDeleted = false;
            membership.IsActive = true;
            membership.UpdatedDate = DateTime.UtcNow;
        }
        else
        {
            membership = new GroupUsers
            {
                GroupId = groupId,
                UserId = userId,
                IsActive = true,
                CreatedBy = createdBy
            };

            await _db.Set<GroupUsers>().AddAsync(
                membership,
                cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);

        // فقط کش دسترسی همین کاربر باطل می‌شود.
        _permissionService.Invalidate(userId);
    }

    /// <summary>
    /// حذف کاربر از گروه
    /// </summary>
    public async Task RemoveUserAsync(
        Guid groupId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var membership = await _db.Set<GroupUsers>()
            .FirstOrDefaultAsync(
                x => x.GroupId == groupId &&
                     x.UserId == userId &&
                     x.IsActive,
                cancellationToken);

        if (membership is null)
            throw new NotFoundException(
                "عضویت مورد نظر یافت نشد.");

        membership.IsActive = false;
        membership.IsDeleted = true;
        membership.UpdatedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        // دسترسی شخصی و عضویت‌های دیگر کاربر حفظ می‌شوند.
        _permissionService.Invalidate(userId);
    }



    public async Task<List<Menu>> GetAvailableMenusAsync(
        CancellationToken cancellationToken)
    {
        return await _db.Set<Menu>()
            .AsNoTracking()
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.PageCode)
            .ThenBy(x => x.MenuTitle)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Guid>> GetAssignedMenuIdsAsync(
        Guid groupId,
        CancellationToken cancellationToken)
    {
        var groupExists = await _db.Set<UserGroup>()
            .AnyAsync(x => x.Id == groupId, cancellationToken);

        if (!groupExists)
            throw new NotFoundException("گروه موردنظر یافت نشد.");

        return await _db.Set<GroupAccessMenus>()
            .AsNoTracking()
            .Where(x =>
                x.GroupId == groupId &&
                x.IsActive &&
                !x.IsDeleted)
            .Select(x => x.MenuId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateMenuAccessAsync(
        Guid groupId,
        List<Guid> menuIds,
        Guid updatedBy,
        CancellationToken cancellationToken)
    {
        var groupExists = await _db.Set<UserGroup>()
            .AnyAsync(x => x.Id == groupId, cancellationToken);

        if (!groupExists)
            throw new NotFoundException("گروه موردنظر یافت نشد.");

        // حذف شناسه‌های تکراری ارسالی
        var selectedMenuIds = menuIds
            .Distinct()
            .ToHashSet();

        // بررسی معتبر بودن تمام منوهای انتخاب‌شده
        if (selectedMenuIds.Count > 0)
        {
            var validMenuIds = await _db.Set<Menu>()
                .Where(x =>
                    selectedMenuIds.Contains(x.Id) &&
                    x.IsActive &&
                    !x.IsDeleted)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            if (validMenuIds.Count != selectedMenuIds.Count)
                throw new BadRequestException(
                    "یک یا چند منوی انتخاب‌شده نامعتبر یا غیرفعال هستند.");
        }

        // دریافت تمام دسترسی‌های قبلی گروه، حتی رکوردهای حذف‌شده
        var existingAccesses = await _db.Set<GroupAccessMenus>()
            .IgnoreQueryFilters()
            .Where(x => x.GroupId == groupId)
            .ToListAsync(cancellationToken);

        // دسترسی‌هایی که دیگر انتخاب نشده‌اند حذف نرم می‌شوند.
        foreach (var access in existingAccesses)
        {
            if (!selectedMenuIds.Contains(access.MenuId))
            {
                access.IsActive = false;
                access.IsDeleted = true;
                access.UpdatedDate = DateTime.UtcNow;
            }
        }

        // برای هر منوی انتخاب‌شده، رکورد قبلی را فعال می‌کنیم
        // یا در صورت نبود رکورد، یک رکورد جدید می‌سازیم.
        foreach (var menuId in selectedMenuIds)
        {
            var existing = existingAccesses
                .Where(x => x.MenuId == menuId)
                .ToList();

            if (existing.Count > 0)
            {
                // اولین رکورد را نگه می‌داریم.
                var access = existing[0];

                access.IsActive = true;
                access.IsDeleted = false;
                access.UpdatedDate = DateTime.UtcNow;

                // اگر رکورد تکراری قدیمی وجود داشته باشد،
                // بقیه را حذف نرم می‌کنیم.
                foreach (var duplicate in existing.Skip(1))
                {
                    duplicate.IsActive = false;
                    duplicate.IsDeleted = true;
                    duplicate.UpdatedDate = DateTime.UtcNow;
                }
            }
            else
            {
                var newAccess = new GroupAccessMenus
                {
                    GroupId = groupId,
                    MenuId = menuId,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = updatedBy
                };

                await _db.Set<GroupAccessMenus>()
                    .AddAsync(newAccess, cancellationToken);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        // تغییر دسترسی گروه باید کش تمام کاربران را باطل کند.
        _permissionService.InvalidateAll();
    }

}
