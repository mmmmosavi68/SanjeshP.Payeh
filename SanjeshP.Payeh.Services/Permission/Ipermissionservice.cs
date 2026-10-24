namespace SanjeshP.Payeh.Services.Permissions
{
    /// <summary>
    /// بررسی دسترسی کاربر به Area/Controller/Action بر اساس جدول‌های
    /// UserAccessMenus (دسترسی مستقیم) و GroupAccessMenus (دسترسی از طریق گروه).
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>آیا کاربر به این اکشن دسترسی دارد؟ (مقایسه‌ی بدون حساسیت به حروف)</summary>
        Task<bool> HasAccessAsync(Guid userId,string? version,  string controller, string action, CancellationToken cancellationToken);

        /// <summary>پاک‌کردن کش یک کاربر (بعد از تغییر دسترسی یا گروه‌های او).</summary>
        void Invalidate(Guid userId);

        /// <summary>پاک‌کردن کش همه‌ی کاربران (بعد از تغییر دسترسی یک گروه یا تغییر جدول Menu).</summary>
        void InvalidateAll();
    }
}