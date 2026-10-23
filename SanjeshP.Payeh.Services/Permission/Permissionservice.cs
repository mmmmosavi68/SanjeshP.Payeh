using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Data;
using SanjeshP.Payeh.Entities.Group;
using SanjeshP.Payeh.Entities.Menu;

namespace SanjeshP.Payeh.Services.Permissions
{
    public class PermissionService : IPermissionService, IScopedDependency
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        // «نسل» کش: با لغو این توکن، کش همه‌ی کاربران یک‌جا باطل می‌شود.
        private static CancellationTokenSource _epoch = new();

        private readonly ApplicationDbContext _db;
        private readonly IMemoryCache _cache;

        public PermissionService(ApplicationDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<bool> HasAccessAsync(Guid userId, string? area, string controller, string action, CancellationToken cancellationToken)
        {
            var permissions = await GetPermissionsAsync(userId, cancellationToken);
            return permissions.Contains(BuildKey(area, controller, action));
        }

        public void Invalidate(Guid userId) => _cache.Remove(CacheKey(userId));

        public void InvalidateAll()
        {
            var old = Interlocked.Exchange(ref _epoch, new CancellationTokenSource());
            old.Cancel();
            old.Dispose();
        }

        private async Task<HashSet<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken)
        {
            // اگر در کش نبود، از دیتابیس خوانده می‌شود (هرگز «اجازه‌ی عبور» نیست).
            var permissions = await _cache.GetOrCreateAsync(CacheKey(userId), async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                entry.AddExpirationToken(new CancellationChangeToken(_epoch.Token));
                return await LoadFromDatabaseAsync(userId, cancellationToken);
            });

            return permissions ?? new HashSet<string>();
        }

        private async Task<HashSet<string>> LoadFromDatabaseAsync(Guid userId, CancellationToken cancellationToken)
        {
            // دسترسی مستقیم کاربر
            var direct = _db.Set<UserAccessMenus>()
                .Where(a => a.UserId == userId && a.IsActive && !a.IsDeleted)
                .Select(a => a.Menu);

            // دسترسی از طریق گروه‌های فعال کاربر
            var viaGroups = from gu in _db.Set<GroupUsers>()
                            where gu.UserId == userId && gu.IsActive && !gu.IsDeleted
                                  && gu.Group.IsActive && !gu.Group.IsDeleted
                            join ga in _db.Set<GroupAccessMenus>() on gu.GroupId equals ga.GroupId
                            where ga.IsActive && !ga.IsDeleted
                            select ga.Menu;

            var rows = await direct.Union(viaGroups)
                .Where(m => m.IsActive && !m.IsDeleted && m.ControllerName != null && m.ActionName != null)
                .Select(m => new { m.Area, m.ControllerName, m.ActionName })
                .Distinct()
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return rows
                .Select(r => BuildKey(r.Area, r.ControllerName, r.ActionName))
                .ToHashSet(StringComparer.Ordinal);
        }

        private static string CacheKey(Guid userId) => $"perm:{userId:N}";

        private static string BuildKey(string? area, string controller, string action)
            => $"{area?.Trim().ToLowerInvariant() ?? string.Empty}|{controller.Trim().ToLowerInvariant()}|{action.Trim().ToLowerInvariant()}";
    }
}