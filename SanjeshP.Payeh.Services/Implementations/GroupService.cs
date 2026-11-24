
using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.Data;
using SanjeshP.Payeh.Entities.Group;
using SanjeshP.Payeh.Entities.Menu;
using SanjeshP.Payeh.Entities.User;

using UserGroup = SanjeshP.Payeh.Entities.Group.Group;

namespace SanjeshP.Payeh.Services.Implementations;

public sealed class GroupService : IGroupService, IScopedDependency
{
    private readonly IUnitOfWork _uow;
    private readonly IPermissionService _permissionService;

    public GroupService(
        IUnitOfWork uow,
        IPermissionService permissionService)
    {
        _uow = uow;
        _permissionService = permissionService;
    }

    /// <summary>
    /// دریافت تمام گروه‌ها به همراه اعضای فعال آن‌ها
    /// </summary>
    public async Task<List<UserGroup>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _uow.Repository<UserGroup>().Entities
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
        var group = await _uow.Repository<UserGroup>().Entities
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
        var exists = await _uow.Repository<UserGroup>().Entities
            .AnyAsync(
                x => x.Id == groupId,
                cancellationToken);

        if (!exists)
            throw new NotFoundException(
                "گروه مورد نظر یافت نشد.");

        return await _uow.Repository<GroupUsers>().Entities
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

        var exists = await _uow.Repository<UserGroup>().Entities
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

        await _uow.Repository<UserGroup>().Entities.AddAsync(
            group,
            cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);

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
        var group = await _uow.Repository<UserGroup>().Entities
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

        var exists = await _uow.Repository<UserGroup>().Entities
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

        await _uow.SaveChangesAsync(cancellationToken);

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
        var group = await _uow.Repository<UserGroup>().Entities
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (group is null)
            throw new NotFoundException(
                "گروه مورد نظر یافت نشد.");

        group.IsDeleted = true;
        group.IsActive = false;
        group.UpdatedDate = DateTime.UtcNow;

        await _uow.SaveChangesAsync(cancellationToken);

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
        var group = await _uow.Repository<UserGroup>().Entities
            .FirstOrDefaultAsync(
                x => x.Id == groupId,
                cancellationToken);

        if (group is null)
            throw new NotFoundException(
                "گروه مورد نظر یافت نشد.");

        if (!group.IsActive)
            throw new BadRequestException(
                "امکان افزودن کاربر به گروه غیرفعال وجود ندارد.");

        var userExists = await _uow.Repository<User>().Entities
            .AnyAsync(
                x => x.Id == userId && x.IsActive,
                cancellationToken);

        if (!userExists)
            throw new NotFoundException(
                "کاربر مورد نظر یافت نشد یا غیرفعال است.");

        // بررسی عضویت فعلی یا عضویت حذف‌شده قبلی
        var membership = await _uow.Repository<GroupUsers>().Entities
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

            await _uow.Repository<GroupUsers>().Entities.AddAsync(
                membership,
                cancellationToken);
        }

        await _uow.SaveChangesAsync(cancellationToken);

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
        var membership = await _uow.Repository<GroupUsers>().Entities
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

        await _uow.SaveChangesAsync(cancellationToken);

        // دسترسی شخصی و عضویت‌های دیگر کاربر حفظ می‌شوند.
        _permissionService.Invalidate(userId);
    }


    /// <summary>
    /// افزودن گروهی کاربران به یک گروه
    /// </summary>
    public async Task AddUsersAsync(
        Guid groupId,
        List<Guid> userIds,
        Guid createdBy,
        CancellationToken cancellationToken)
    {
        if (userIds is null || userIds.Count == 0)
            throw new BadRequestException(
                "حداقل یک کاربر باید انتخاب شود.");

        var distinctUserIds = userIds
            .Distinct()
            .ToList();

        if (distinctUserIds.Any(x => x == Guid.Empty))
            throw new BadRequestException(
                "شناسه یک یا چند کاربر نامعتبر است.");

        if (distinctUserIds.Count > 200)
            throw new BadRequestException(
                "حداکثر ۲۰۰ کاربر در هر درخواست قابل انتخاب است.");

        var group = await _uow.Repository<UserGroup>().Entities
            .FirstOrDefaultAsync(
                x => x.Id == groupId,
                cancellationToken);

        if (group is null)
            throw new NotFoundException(
                "گروه مورد نظر یافت نشد.");

        if (!group.IsActive)
            throw new BadRequestException(
                "امکان افزودن کاربر به گروه غیرفعال وجود ندارد.");

        // اعتبارسنجی تمام کاربران قبل از هرگونه تغییر
        var validUserIds = await _uow.Repository<User>().Entities
            .Where(x =>
                distinctUserIds.Contains(x.Id) &&
                x.IsActive &&
                !x.IsDeleted)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (validUserIds.Count != distinctUserIds.Count)
            throw new NotFoundException(
                "یک یا چند کاربر یافت نشدند یا غیرفعال هستند.");

        // دریافت عضویت‌های قبلی، شامل رکوردهای حذف نرم‌شده
        var existingMemberships = await _uow.Repository<GroupUsers>().Entities
            .IgnoreQueryFilters()
            .Where(x =>
                x.GroupId == groupId &&
                distinctUserIds.Contains(x.UserId))
            .ToListAsync(cancellationToken);

        // اگر حتی یک کاربر در حال حاضر عضو فعال گروه باشد،
        // کل درخواست رد می‌شود.
        var alreadyMember = existingMemberships
            .Any(x => x.IsActive && !x.IsDeleted);

        if (alreadyMember)
            throw new ConflictException(
                "یک یا چند کاربر در حال حاضر عضو گروه هستند.");

        var now = DateTime.UtcNow;

        foreach (var userId in distinctUserIds)
        {
            var previousMemberships = existingMemberships
                .Where(x => x.UserId == userId)
                .ToList();

            if (previousMemberships.Count > 0)
            {
                // استفاده مجدد از اولین عضویت قبلی
                var membership = previousMemberships[0];

                membership.IsActive = true;
                membership.IsDeleted = false;
                membership.UpdatedDate = now;

                // پاک‌سازی عضویت‌های تکراری احتمالی
                foreach (var duplicate in previousMemberships.Skip(1))
                {
                    duplicate.IsActive = false;
                    duplicate.IsDeleted = true;
                    duplicate.UpdatedDate = now;
                }
            }
            else
            {
                var membership = new GroupUsers
                {
                    GroupId = groupId,
                    UserId = userId,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = createdBy
                };

                await _uow.Repository<GroupUsers>().Entities
                    .AddAsync(membership, cancellationToken);
            }
        }

        await _uow.SaveChangesAsync(cancellationToken);

        // باطل کردن کش دسترسی تمام کاربران اضافه‌شده
        foreach (var userId in distinctUserIds)
            _permissionService.Invalidate(userId);
    }

    /// <summary>
    /// حذف گروهی کاربران از یک گروه
    /// </summary>
    public async Task RemoveUsersAsync(
        Guid groupId,
        List<Guid> userIds,
        CancellationToken cancellationToken)
    {
        if (userIds is null || userIds.Count == 0)
            throw new BadRequestException(
                "حداقل یک کاربر باید انتخاب شود.");

        var distinctUserIds = userIds
            .Distinct()
            .ToList();

        if (distinctUserIds.Any(x => x == Guid.Empty))
            throw new BadRequestException(
                "شناسه یک یا چند کاربر نامعتبر است.");

        if (distinctUserIds.Count > 200)
            throw new BadRequestException(
                "حداکثر ۲۰۰ کاربر در هر درخواست قابل انتخاب است.");

        var groupExists = await _uow.Repository<UserGroup>().Entities
            .AnyAsync(
                x => x.Id == groupId,
                cancellationToken);

        if (!groupExists)
            throw new NotFoundException(
                "گروه مورد نظر یافت نشد.");

        // دریافت تمام عضویت‌های فعال درخواستی
        var memberships = await _uow.Repository<GroupUsers>().Entities
            .Where(x =>
                x.GroupId == groupId &&
                distinctUserIds.Contains(x.UserId) &&
                x.IsActive &&
                !x.IsDeleted)
            .ToListAsync(cancellationToken);

        // اگر حتی یک عضویت معتبر نباشد، هیچ تغییری انجام نمی‌شود.
        if (memberships.Count != distinctUserIds.Count)
            throw new NotFoundException(
                "یک یا چند مورد از عضویت‌های انتخاب‌شده یافت نشدند.");

        var now = DateTime.UtcNow;

        foreach (var membership in memberships)
        {
            membership.IsActive = false;
            membership.IsDeleted = true;
            membership.UpdatedDate = now;
        }

        await _uow.SaveChangesAsync(cancellationToken);

        // باطل کردن کش دسترسی تمام کاربران حذف‌شده
        foreach (var userId in distinctUserIds)
            _permissionService.Invalidate(userId);
    }
    public async Task<List<Menu>> GetAvailableMenusAsync(
        CancellationToken cancellationToken)
    {
        return await _uow.Repository<Menu>().Entities
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
        var groupExists = await _uow.Repository<UserGroup>().Entities
            .AnyAsync(x => x.Id == groupId, cancellationToken);

        if (!groupExists)
            throw new NotFoundException("گروه موردنظر یافت نشد.");

        return await _uow.Repository<GroupAccessMenus>().Entities
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
        var groupExists = await _uow.Repository<UserGroup>().Entities
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
            var validMenuIds = await _uow.Repository<Menu>().Entities
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
        var existingAccesses = await _uow.Repository<GroupAccessMenus>().Entities
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

                await _uow.Repository<GroupAccessMenus>().Entities
                    .AddAsync(newAccess, cancellationToken);
            }
        }

        await _uow.SaveChangesAsync(cancellationToken);

        // تغییر دسترسی گروه باید کش تمام کاربران را باطل کند.
        _permissionService.InvalidateAll();
    }

}
