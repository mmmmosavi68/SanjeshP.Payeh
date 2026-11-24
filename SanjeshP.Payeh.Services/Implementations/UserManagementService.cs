
using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.Data;
using SanjeshP.Payeh.Entities.Menu;
using SanjeshP.Payeh.Entities.User;
using SanjeshP.Payeh.Entities.University;

namespace SanjeshP.Payeh.Services.Implementations;

public sealed class UserManagementService : IUserManagementService, IScopedDependency
{
    private readonly IUnitOfWork _uow;
    private readonly IPermissionService _permissionService;

    public UserManagementService(IUnitOfWork uow, IPermissionService permissionService)
    {
        _uow = uow;
        _permissionService = permissionService;
    }

    public async Task<List<Menu>> GetAvailableMenusAsync(CancellationToken cancellationToken)
    {
        return await _uow.Repository<Menu>().Entities
            .AsNoTracking()
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.PageCode)
            .ThenBy(x => x.MenuTitle)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Guid>> GetAssignedMenuIdsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userExists = await _uow.Repository<User>().Entities
            .AnyAsync(
                x => x.Id == userId && !x.IsDeleted,
                cancellationToken);

        if (!userExists)
            throw new NotFoundException(ApiMessages.UserManagement.M003);

        return await _uow.Repository<UserAccessMenus>().Entities
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.IsActive &&
                !x.IsDeleted)
            .Select(x => x.MenuId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateMenuAccessAsync(Guid userId, List<Guid> menuIds, Guid updatedBy, CancellationToken cancellationToken)
    {
        if (userId == updatedBy)
            throw new ForbiddenException(ApiMessages.Authorization.SelfAccessChange);

        if (menuIds is { Count: > 0 })
        {
            var alreadyAssigned = await GetAssignedMenuIdsAsync(userId, cancellationToken);
            await _permissionService.EnsureCanGrantMenusAsync(updatedBy, menuIds.Except(alreadyAssigned), cancellationToken);
        }

        var userExists = await _uow.Repository<User>().Entities
            .AnyAsync(
                x => x.Id == userId && !x.IsDeleted,
                cancellationToken);

        if (!userExists)
            throw new NotFoundException(ApiMessages.UserManagement.M003);

        if (menuIds is null)
            throw new BadRequestException(ApiMessages.UserManagement.M004);

        var selectedMenuIds = menuIds
            .Distinct()
            .ToHashSet();

        if (selectedMenuIds.Contains(Guid.Empty))
            throw new BadRequestException(ApiMessages.UserManagement.M005);

        if (selectedMenuIds.Count > 500)
            throw new BadRequestException(ApiMessages.UserManagement.M006);

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
                    ApiMessages.UserManagement.M007);
        }

        // رکوردهای حذف‌شده قبلی نیز باید بررسی شوند تا
        // هنگام انتخاب مجدد، رکورد تکراری ساخته نشود.
        var existingAccesses = await _uow.Repository<UserAccessMenus>().Entities
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
                await _uow.Repository<UserAccessMenus>().Entities.AddAsync(
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

        await _uow.SaveChangesAsync(cancellationToken);

        // فقط کش همین کاربر باید باطل شود.
        // دسترسی‌های گروهی او دست‌نخورده باقی می‌مانند.
        _permissionService.Invalidate(userId);
    }

    public async Task BulkDeactivateAsync(List<Guid> userIds, Guid currentUserId, CancellationToken cancellationToken)
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

        await _uow.SaveChangesAsync(cancellationToken);

        foreach (var user in users)
            _permissionService.Invalidate(user.Id);
    }

    public async Task BulkDeleteAsync(List<Guid> userIds, Guid currentUserId, CancellationToken cancellationToken)
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

        await _uow.SaveChangesAsync(cancellationToken);

        foreach (var user in users)
            _permissionService.Invalidate(user.Id);
    }

    private async Task<List<User>> ValidateBulkUsersAsync(List<Guid> userIds, Guid currentUserId, CancellationToken cancellationToken)
    {
        if (userIds is null || userIds.Count == 0)
            throw new BadRequestException(
                ApiMessages.UserManagement.M008);

        var distinctIds = userIds
            .Distinct()
            .ToList();

        if (distinctIds.Any(x => x == Guid.Empty))
            throw new BadRequestException(
                ApiMessages.UserManagement.M009);

        if (distinctIds.Count > 200)
            throw new BadRequestException(
                ApiMessages.UserManagement.M010);

        if (distinctIds.Contains(currentUserId))
            throw new BadRequestException(
                ApiMessages.UserManagement.M011);

        var users = await _uow.Repository<User>().Entities
            .Where(x =>
                distinctIds.Contains(x.Id) &&
                !x.IsDeleted)
            .ToListAsync(cancellationToken);

        if (users.Count != distinctIds.Count)
            throw new NotFoundException(
                ApiMessages.UserManagement.M012);

        return users;
    }

    private async Task RevokeSessionsAsync(List<Guid> userIds, CancellationToken cancellationToken)
    {
        var sessions = await _uow.Repository<UserSession>().Entities
            .Where(x =>
                userIds.Contains(x.UserId) &&
                x.RevocationDate == null)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;

        foreach (var session in sessions)
            session.RevocationDate = now;
    }



    public async Task<List<UniversityOption>> GetAvailableUniversitiesAsync(CancellationToken cancellationToken)
    {
        return await _uow.Repository<University>().Entities
            .AsNoTracking()
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.Title)
            .Select(x => new UniversityOption(
                x.Id,
                x.Title,
                x.ParentUniversityId))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<int>> GetAssignedUniversityIdsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userExists = await _uow.Repository<User>().Entities
            .AnyAsync(
                x => x.Id == userId && !x.IsDeleted,
                cancellationToken);

        if (!userExists)
            throw new NotFoundException(ApiMessages.UserManagement.M003);

        return await _uow.Repository<UserUniversity>().Entities
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.IsActive &&
                !x.IsDeleted)
            .Select(x => x.UniversityId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateUniversityAccessAsync(Guid userId, List<int> universityIds, Guid updatedBy, CancellationToken cancellationToken)
    {
        if (userId == updatedBy)
            throw new ForbiddenException(ApiMessages.Authorization.SelfAccessChange);

        var userExists = await _uow.Repository<User>().Entities
            .AnyAsync(
                x => x.Id == userId && !x.IsDeleted,
                cancellationToken);

        if (!userExists)
            throw new NotFoundException(ApiMessages.UserManagement.M003);

        if (universityIds is null)
            throw new BadRequestException(ApiMessages.UserManagement.M013);

        var selectedUniversityIds = universityIds
            .Distinct()
            .ToHashSet();

        if (selectedUniversityIds.Any(x => x <= 0))
            throw new BadRequestException(
                ApiMessages.UserManagement.M014);

        if (selectedUniversityIds.Count > 500)
            throw new BadRequestException(
                ApiMessages.UserManagement.M015);

        if (selectedUniversityIds.Count > 0)
        {
            var validUniversityIds = await _uow.Repository<University>().Entities
                .Where(x =>
                    selectedUniversityIds.Contains(x.Id) &&
                    x.IsActive &&
                    !x.IsDeleted)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            if (validUniversityIds.Count != selectedUniversityIds.Count)
                throw new BadRequestException(
                    ApiMessages.UserManagement.M016);
        }

        // تمام ارتباط‌های قبلی کاربر، حتی رکوردهای حذف نرم‌شده
        // دریافت می‌شوند تا رکورد تکراری ایجاد نشود.
        var existingAccesses = await _uow.Repository<UserUniversity>().Entities
            .IgnoreQueryFilters()
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;

        // ابتدا تمام ارتباط‌های قبلی غیرفعال می‌شوند.
        foreach (var access in existingAccesses)
        {
            access.IsActive = false;
            access.IsDeleted = true;
            access.UpdatedDate = now;
        }

        // دانشگاه‌های انتخاب‌شده فعال یا در صورت نیاز ایجاد می‌شوند.
        foreach (var universityId in selectedUniversityIds)
        {
            var existingAccess = existingAccesses
                .FirstOrDefault(x => x.UniversityId == universityId);

            if (existingAccess is not null)
            {
                existingAccess.IsActive = true;
                existingAccess.IsDeleted = false;
                existingAccess.UpdatedDate = now;
            }
            else
            {
                await _uow.Repository<UserUniversity>().Entities.AddAsync(
                    new UserUniversity
                    {
                        UserId = userId,
                        UniversityId = universityId,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = updatedBy
                    },
                    cancellationToken);
            }
        }

        await _uow.SaveChangesAsync(cancellationToken);
    }
}
