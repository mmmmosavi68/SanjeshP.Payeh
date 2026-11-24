using SanjeshP.Payeh.Entities.User;

namespace SanjeshP.Payeh.Services.Contracts
{
    public record UserQuery(
        int PageNumber,
        int PageSize,
        string? Search,
        bool? IsActive,
        int? RoleId,
        string? SortBy,
        string? SortDirection);

    public record PagedUsers(
        IReadOnlyList<User> Items,
        int TotalCount,
        int PageNumber,
        int PageSize);

    public interface IUserService
    {
        Task<User?> ValidateCredentialsAsync(
            string userName,
            string password,
            CancellationToken cancellationToken);

        Task<PagedUsers> GetPagedAsync(
            UserQuery query,
            CancellationToken cancellationToken);

        Task<User> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken);

        Task<User> CreateAsync(
            User user,
            string? plainPassword,
            IEnumerable<int> roleIds,
            UserProfile profile,
            Guid createdBy,
            CancellationToken cancellationToken);

        Task<User> UpdateAsync(
            Guid id,
            Action<User> applyChanges,
            Action<UserProfile> applyProfileChanges,
            IEnumerable<int>? roleIds,
            CancellationToken cancellationToken);

        Task SoftDeleteAsync(
            Guid id,
            CancellationToken cancellationToken);

        Task SetPasswordAsync(
            Guid id,
            string newPassword,
            CancellationToken cancellationToken);
    }
}