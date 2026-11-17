using SanjeshP.Payeh.Entities.Menu;

namespace SanjeshP.Payeh.Services.Users;

public record UniversityOption(
    int Id,
    string Title,
    int? ParentUniversityId);

public interface IUserManagementService
{
    Task<List<Menu>> GetAvailableMenusAsync(
        CancellationToken cancellationToken);

    Task<List<Guid>> GetAssignedMenuIdsAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task UpdateMenuAccessAsync(
        Guid userId,
        List<Guid> menuIds,
        Guid updatedBy,
        CancellationToken cancellationToken);

    Task<List<UniversityOption>> GetAvailableUniversitiesAsync(
        CancellationToken cancellationToken);

    Task<List<int>> GetAssignedUniversityIdsAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task UpdateUniversityAccessAsync(
        Guid userId,
        List<int> universityIds,
        Guid updatedBy,
        CancellationToken cancellationToken);

    Task BulkDeactivateAsync(
        List<Guid> userIds,
        Guid currentUserId,
        CancellationToken cancellationToken);

    Task BulkDeleteAsync(
        List<Guid> userIds,
        Guid currentUserId,
        CancellationToken cancellationToken);
}