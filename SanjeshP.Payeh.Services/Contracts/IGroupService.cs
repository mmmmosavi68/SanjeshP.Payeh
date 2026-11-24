using SanjeshP.Payeh.Entities.Group;
using SanjeshP.Payeh.Entities.Menu;


namespace SanjeshP.Payeh.Services.Contracts;

public interface IGroupService
{
    Task<List<Group>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<Group> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<List<GroupUsers>> GetUsersAsync(
        Guid groupId,
        CancellationToken cancellationToken);

    Task<Group> CreateAsync(
        string groupName,
        bool isActive,
        Guid createdBy,
        CancellationToken cancellationToken);

    Task<Group> UpdateAsync(
        Guid id,
        string groupName,
        bool isActive,
        CancellationToken cancellationToken);

    Task SoftDeleteAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task AddUserAsync(
        Guid groupId,
        Guid userId,
        Guid createdBy,
        CancellationToken cancellationToken);

    Task RemoveUserAsync(
        Guid groupId,
        Guid userId,
        CancellationToken cancellationToken);
    Task AddUsersAsync(
    Guid groupId,
    List<Guid> userIds,
    Guid createdBy,
    CancellationToken cancellationToken);

    Task RemoveUsersAsync(
        Guid groupId,
        List<Guid> userIds,
        CancellationToken cancellationToken);
    // دریافت تمام منوهای فعال سامانه
    Task<List<Menu>> GetAvailableMenusAsync(
        CancellationToken cancellationToken);

    // دریافت شناسه منوهای اختصاص‌یافته به گروه
    Task<List<Guid>> GetAssignedMenuIdsAsync(
        Guid groupId,
        CancellationToken cancellationToken);

    // جایگزینی دسترسی‌های منوی گروه
    Task UpdateMenuAccessAsync(
        Guid groupId,
        List<Guid> menuIds,
        Guid updatedBy,
        CancellationToken cancellationToken);


}