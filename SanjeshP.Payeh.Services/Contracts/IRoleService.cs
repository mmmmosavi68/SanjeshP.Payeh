using SanjeshP.Payeh.Entities.User;

namespace SanjeshP.Payeh.Services.Contracts
{
    /// <summary>فهرست نقش‌های فعال (برای انتخاب نقش در فرم کاربر).</summary>
    public interface IRoleService
    {
        Task<IReadOnlyList<Role>> GetActiveRolesAsync(CancellationToken cancellationToken);
    }
}
