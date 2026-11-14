
using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.Data;
using SanjeshP.Payeh.Entities.Menu;
using SanjeshP.Payeh.Entities.User;
using SanjeshP.Payeh.Services.Permissions;

namespace SanjeshP.Payeh.Services.Users;

public sealed class UserManagementService : IUserManagementService, IScopedDependency
{
    private readonly ApplicationDbContext _db;
    private readonly IPermissionService _permissionService;

    public UserManagementService(
        ApplicationDbContext db,
        IPermissionService permissionService)
    {
        _db = db;
        _permissionService = permissionService;
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
        Guid userId,
        CancellationToken cancellationToken)
    {
        var userExists = await _db.Set<User>()
            .AnyAsync(
                x => x.Id == userId && !x.IsDeleted,
                cancellationToken);

        if (!userExists)
            throw new NotFoundException("کاربر مورد نظر یافت نشد.");

        return await _db.Set<UserAccessMenus>()
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.IsActive &&
                !x.IsDeleted)
            .Select(x => x.MenuId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateMenuAccessAsync(
        Guid userId,
        List<Guid> menuIds,
        Guid updatedBy,
        CancellationToken cancellationToken)
    {
        var userExists = await _db.Set<User>()
            .AnyAsync(
                x => x.Id == userId && !x.IsDeleted,
                cancellationToken);

        if (!userExists)
            throw new NotFoundException("کاربر مورد نظر یافت نشد.");

        if (menuIds is null)
            throw new BadRequestException("فهرست منوها معتبر نیست.");

        var selectedMenuIds = menuIds
            .Distinct()
            .ToHashSet();

        if (selectedMenuIds.Contains(Guid.Empty))
            throw new BadRequestException("شناسه یک یا چند منو نامعتبر است.");

        if (selectedMenuIds.Count > 500)
            throw new BadRequestException("حداکثر ۵۰۰ منو قابل انتخاب است.");

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

        // رکوردهای حذف‌شده قبلی نیز باید بررسی شوند تا
        // هنگام انتخاب مجدد، رکورد تکراری ساخته نشود.
        var existingAccesses = await _db.Set<UserAccessMenus>()
            .IgnoreQueryFilters()
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;

        // دسترسی‌هایی که دیگر انتخاب نشده‌اند حذف نرم می‌شوند.
        foreach (var access in existingAccesses)
        {
            if (!selectedMenuIds.Contains(access.MenuId))
            {
                access.IsActive = false;
                access.IsDeleted = true;
                access.UpdatedDate = now;
            }
        }

        foreach (var menuId in selectedMenuIds)
        {
            var previousRecords = existingAccesses
                .Where(x => x.MenuId == menuId)
                .ToList();

            if (previousRecords.Count > 0)
            {
                var access = previousRecords[0];

                access.IsActive = true;
                access.IsDeleted = false;
                access.UpdatedDate = now;

                // پاک‌سازی رکوردهای تکراری احتمالی
                foreach (var duplicate in previousRecords.Skip(1))
                {
                    duplicate.IsActive = false;
                    duplicate.IsDeleted = true;
                    duplicate.UpdatedDate = now;
                }
            }
            else
            {
                await _db.Set<UserAccessMenus>().AddAsync(
                    new UserAccessMenus
                    {
                        UserId = userId,
                        MenuId = menuId,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = updatedBy
                    },
                    cancellationToken);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        // فقط کش همین کاربر باید باطل شود.
        // دسترسی‌های گروهی او دست‌نخورده باقی می‌مانند.
        _permissionService.Invalidate(userId);
    }

    public async Task BulkDeactivateAsync(
        List<Guid> userIds,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        var users = await ValidateBulkUsersAsync(
            userIds,
            currentUserId,
            cancellationToken);

        var now = DateTime.UtcNow;

        foreach (var user in users)
        {
            user.IsActive = false;
            user.UpdatedDate = now;
        }

        await RevokeSessionsAsync(
            users.Select(x => x.Id).ToList(),
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);

        foreach (var user in users)
            _permissionService.Invalidate(user.Id);
    }

    public async Task BulkDeleteAsync(
        List<Guid> userIds,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        var users = await ValidateBulkUsersAsync(
            userIds,
            currentUserId,
            cancellationToken);

        var now = DateTime.UtcNow;

        foreach (var user in users)
        {
            user.IsActive = false;
            user.IsDeleted = true;
            user.UpdatedDate = now;
        }

        await RevokeSessionsAsync(
            users.Select(x => x.Id).ToList(),
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);

        foreach (var user in users)
            _permissionService.Invalidate(user.Id);
    }

    private async Task<List<User>> ValidateBulkUsersAsync(
        List<Guid> userIds,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        if (userIds is null || userIds.Count == 0)
            throw new BadRequestException(
                "حداقل یک کاربر باید انتخاب شود.");

        var distinctIds = userIds
            .Distinct()
            .ToList();

        if (distinctIds.Any(x => x == Guid.Empty))
            throw new BadRequestException(
                "شناسه یک یا چند کاربر نامعتبر است.");

        if (distinctIds.Count > 200)
            throw new BadRequestException(
                "حداکثر ۲۰۰ کاربر در هر درخواست قابل انتخاب است.");

        if (distinctIds.Contains(currentUserId))
            throw new BadRequestException(
                "امکان حذف یا غیرفعال‌سازی حساب کاربری خودتان وجود ندارد.");

        var users = await _db.Set<User>()
            .Where(x =>
                distinctIds.Contains(x.Id) &&
                !x.IsDeleted)
            .ToListAsync(cancellationToken);

        if (users.Count != distinctIds.Count)
            throw new NotFoundException(
                "یک یا چند کاربر یافت نشدند یا قبلاً حذف شده‌اند.");

        return users;
    }

    private async Task RevokeSessionsAsync(
        List<Guid> userIds,
        CancellationToken cancellationToken)
    {
        var sessions = await _db.Set<UserSession>()
            .Where(x =>
                userIds.Contains(x.UserId) &&
                x.RevocationDate == null)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;

        foreach (var session in sessions)
            session.RevocationDate = now;
    }
}
