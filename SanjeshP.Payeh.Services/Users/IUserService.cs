using SanjeshP.Payeh.Entities.User;

namespace SanjeshP.Payeh.Services.Users
{
    /// <summary>پارامترهای کوئری فهرست کاربران (لایه‌ی سرویس، مستقل از DTO وب).</summary>
    public record UserQuery(
        int PageNumber,
        int PageSize,
        string? Search,
        bool? IsActive,
        int? RoleId,
        string? SortBy,
        string? SortDirection);

    /// <summary>نتیجه‌ی صفحه‌بندی‌شده‌ی کوئری کاربران.</summary>
    public record PagedUsers(IReadOnlyList<User> Items, int TotalCount, int PageNumber, int PageSize);

    /// <summary>
    /// سرویس مدیریت کاربران: CRUD با حذف نرم، صفحه‌بندی، جستجو، فیلتر و مرتب‌سازی امن (Whitelist).
    /// نگاشت Entity ↔ DTO در لایه‌ی وب با AutoMapper انجام می‌شود.
    /// </summary>
    public interface IUserService
    {
        Task<PagedUsers> GetPagedAsync(UserQuery query, CancellationToken cancellationToken);
        Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<User> CreateAsync(User user, string? plainPassword, IEnumerable<int> roleIds, string? firstName, string? lastName, string? nationalCode, Guid createdBy, CancellationToken cancellationToken);
        Task<User> UpdateAsync(Guid id, Action<User> applyChanges, IEnumerable<int>? roleIds, string? firstName, string? lastName, CancellationToken cancellationToken);
        /// <summary>حذف نرم: IsDeleted=true و ابطال همه‌ی نشست‌ها.</summary>
        Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}