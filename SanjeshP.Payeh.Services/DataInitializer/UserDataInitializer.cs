using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Entities.Common;
using SanjeshP.Payeh.Entities.Group;
using SanjeshP.Payeh.Entities.Menu;
using SanjeshP.Payeh.Entities.User;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using GroupEntity = SanjeshP.Payeh.Entities.Group.Group;
using MenuEntity = SanjeshP.Payeh.Entities.Menu.Menu;
// namespace مربوط به IEFRepository<T> را از لایه Data خودتان اضافه کنید، مثلاً:
// using SanjeshP.Payeh.Data.Contracts;

// نام کلاس‌های User / Menu / Group با نام namespace هایشان یکی است؛ برای جلوگیری از ابهام alias می‌گیریم.
using UserEntity = SanjeshP.Payeh.Entities.User.User;

namespace SanjeshP.Payeh.Services.DataInitializer
{
    /// <summary>
    /// ساخت داده‌های پیش‌فرض (کاربر مدیر، نقش، منوها، گروه و سطح دسترسی‌ها) از فایل‌های DefaultData.
    /// ساختار همان نسخه قبلی است (IEFRepository + فایل JSON + بررسی وجود قبل از درج)، با این تغییرها:
    /// - نام فیلدها مطابق BaseEntity جدید: CreatedDate / IsDeleted / CreatedBy (به‌جای CreateDate / IsDelete / Creator).
    /// - UserGroup → GroupUsers، AccessMenusGroup → GroupAccessMenus، AccessMenus → UserAccessMenus.
    /// - Role: RoleNameEn / NormalizedRoleNameEn / RoleNameFa (به‌جای RoleTitleEn / RoleTitleFa).
    /// - تاریخ‌ها UTC هستند (هم‌راستا با BaseEntity).
    /// - منوها به ترتیب درخت (والد قبل از فرزند) درج می‌شوند، نه بر اساس PageCode.
    /// - برای هر جدول فقط یک‌بار کلیدهای موجود خوانده می‌شود، نه یک کوئری به‌ازای هر ردیف.
    /// فرض: متد Add مخزن همان لحظه SaveChanges می‌کند (مثل نسخه قبلی که بعد از Add دوباره نقش را کوئری می‌کرد).
    /// </summary>
    public class UserDataInitializer : IDataInitializer
    {
        private const string AdminNormalizedUserName = "ADMINISTRATOR";
        private const string AdminNormalizedRoleName = "ADMIN";
        private const string DefaultHostIp = "::1";
        private const string DataFolderName = "DefaultData";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        private readonly IEFRepository<Role> eFRoleRepository;
        private readonly IEFRepository<UserEntity> eFUserRepository;
        private readonly IEFRepository<UserProfile> eFUserProfileRepository;
        private readonly IEFRepository<MenuEntity> eFMenuRepository;
        private readonly IEFRepository<GroupEntity> eFGroupRepository;
        private readonly IEFRepository<GroupUsers> eFGroupUsersRepository;
        private readonly IEFRepository<GroupAccessMenus> eFGroupAccessMenusRepository;
        private readonly IEFRepository<UserAccessMenus> eFUserAccessMenusRepository;
        private readonly IEFRepository<UserRole> eFUserRoleRepository;

        public UserDataInitializer(
              IEFRepository<Role> eFRoleRepository
            , IEFRepository<UserEntity> eFUserRepository
            , IEFRepository<UserProfile> eFUserProfileRepository
            , IEFRepository<MenuEntity> eFMenuRepository
            , IEFRepository<GroupEntity> eFGroupRepository
            , IEFRepository<GroupUsers> eFGroupUsersRepository
            , IEFRepository<GroupAccessMenus> eFGroupAccessMenusRepository
            , IEFRepository<UserAccessMenus> eFUserAccessMenusRepository
            , IEFRepository<UserRole> eFUserRoleRepository)
        {
            this.eFRoleRepository = eFRoleRepository;
            this.eFUserRepository = eFUserRepository;
            this.eFUserProfileRepository = eFUserProfileRepository;
            this.eFMenuRepository = eFMenuRepository;
            this.eFGroupRepository = eFGroupRepository;
            this.eFGroupUsersRepository = eFGroupUsersRepository;
            this.eFGroupAccessMenusRepository = eFGroupAccessMenusRepository;
            this.eFUserAccessMenusRepository = eFUserAccessMenusRepository;
            this.eFUserRoleRepository = eFUserRoleRepository;
        }

        public void InitializeData()
        {
            var path = ResolveDataFolder();

            SeedUsers(path);

            // مدیر سیستم سازنده‌ی بقیه رکوردهاست (مثل نسخه قبلی)
            var admin = eFUserRepository.TableNoTracking.IgnoreQueryFilters()
                            .FirstOrDefault(p => p.NormalizedUserName == AdminNormalizedUserName)
                        ?? throw new InvalidOperationException(
                            $"کاربر {AdminNormalizedUserName} در User.json پیدا نشد.");

            SeedUserProfiles(path, admin.Id);
            SeedMenus(path, admin.Id);
            SeedGroups(path, admin.Id);
            SeedGroupUsers(path, admin.Id);
            SeedGroupAccessMenus(path, admin.Id);
            SeedUserAccessMenus(path, admin.Id);
            SeedRolesAndAdminRole(path, admin.Id);
        }

        // ───────────────────────────── User ─────────────────────────────

        private void SeedUsers(string path)
        {
            var existing = eFUserRepository.TableNoTracking.IgnoreQueryFilters()
                .Select(p => p.NormalizedUserName).ToList().ToHashSet();

            foreach (var user in ReadJson<UserEntity>(path, "User.json"))
            {
                user.NormalizedUserName ??= user.UserName.ToUpperInvariant();
                user.NormalizedEmailAddress ??= user.EmailAddress?.ToUpperInvariant();

                if (existing.Contains(user.NormalizedUserName))
                    continue;

                Stamp(user, user.Id);                       // کاربر مدیر خودش سازنده‌ی خودش است
                user.ExpireDate = DateTime.UtcNow.AddYears(20);
                eFUserRepository.Add(user);
            }
        }

        // ──────────────────────────── UserProfile ────────────────────────────

        private void SeedUserProfiles(string path, Guid adminId)
        {
            var existing = eFUserProfileRepository.TableNoTracking.IgnoreQueryFilters()
                .Select(p => p.UserId).ToList().ToHashSet();

            foreach (var profile in ReadJson<UserProfile>(path, "UserProfile.json"))
            {
                if (existing.Contains(profile.UserId))
                    continue;

                Stamp(profile, adminId);
                eFUserProfileRepository.Add(profile);
            }
        }

        // ─────────────────────────────── Menu ───────────────────────────────

        private void SeedMenus(string path, Guid adminId)
        {
            var existing = eFMenuRepository.TableNoTracking.IgnoreQueryFilters()
                .Select(p => p.Id).ToList().ToHashSet();

            var menus = ReadJson<MenuEntity>(path, "Menu.json");

            foreach (var menu in OrderParentsFirst(menus))
            {
                if (existing.Contains(menu.Id))
                    continue;

                // ستون‌های رشته‌ای Menu در Entity غیرقابل‌null هستند (NOT NULL در دیتابیس)،
                // ولی منوهای والد Controller/Action ندارند و Path در همه null است.
                menu.Path ??= string.Empty;
                menu.Area ??= string.Empty;
                menu.ControllerName ??= string.Empty;
                menu.ActionName ??= string.Empty;
                menu.CssClass ??= string.Empty;
                menu.Icon ??= string.Empty;
                menu.Link ??= string.Empty;

                // IsActive / IsDeleted از خود JSON می‌آید (منوهای حذف‌شده‌ی قدیمی همان‌طور حذف‌شده می‌مانند)
                Stamp(menu, adminId);
                eFMenuRepository.Add(menu);
            }
        }

        /// <summary>
        /// والد قبل از فرزند (برای کلید خارجی ParentId). به‌جای مرتب‌سازی بر اساس PageCode،
        /// چون در داده‌ها PageCode تکراری وجود دارد و ترتیب را تضمین نمی‌کند.
        /// </summary>
        private static List<MenuEntity> OrderParentsFirst(List<MenuEntity> menus)
        {
            var byId = menus.ToDictionary(m => m.Id);
            var visited = new HashSet<Guid>();
            var result = new List<MenuEntity>(menus.Count);

            void Visit(MenuEntity menu)
            {
                if (!visited.Add(menu.Id))
                    return;

                if (menu.ParentId is Guid parentId && byId.TryGetValue(parentId, out var parent))
                    Visit(parent);

                result.Add(menu);
            }

            foreach (var menu in menus.OrderBy(m => m.PageCode))
                Visit(menu);

            return result;
        }

        // ─────────────────────────────── Group ───────────────────────────────

        private void SeedGroups(string path, Guid adminId)
        {
            var existing = eFGroupRepository.TableNoTracking.IgnoreQueryFilters()
                .Select(p => p.GroupName).ToList().ToHashSet();

            foreach (var group in ReadJson<GroupEntity>(path, "Group.json"))
            {
                if (existing.Contains(group.GroupName))
                    continue;

                Stamp(group, adminId);
                eFGroupRepository.Add(group);
            }
        }

        // ───────────────────────────── GroupUsers ─────────────────────────────

        private void SeedGroupUsers(string path, Guid adminId)
        {
            var existing = eFGroupUsersRepository.TableNoTracking.IgnoreQueryFilters()
                .Select(p => new { p.UserId, p.GroupId }).ToList()
                .Select(p => (p.UserId, p.GroupId)).ToHashSet();

            foreach (var item in ReadJson<GroupUsers>(path, "GroupUsers.json"))
            {
                if (existing.Contains((item.UserId, item.GroupId)))
                    continue;

                Stamp(item, adminId);
                eFGroupUsersRepository.Add(item);
            }
        }

        // ─────────────────────────── GroupAccessMenus ───────────────────────────

        private void SeedGroupAccessMenus(string path, Guid adminId)
        {
            var existing = eFGroupAccessMenusRepository.TableNoTracking.IgnoreQueryFilters()
                .Select(p => new { p.GroupId, p.MenuId }).ToList()
                .Select(p => (p.GroupId, p.MenuId)).ToHashSet();

            foreach (var item in ReadJson<GroupAccessMenus>(path, "GroupAccessMenus.json"))
            {
                if (existing.Contains((item.GroupId, item.MenuId)))
                    continue;

                Stamp(item, adminId);
                eFGroupAccessMenusRepository.Add(item);
            }
        }

        // ──────────────────────────── UserAccessMenus ────────────────────────────

        private void SeedUserAccessMenus(string path, Guid adminId)
        {
            var existing = eFUserAccessMenusRepository.TableNoTracking.IgnoreQueryFilters()
                .Select(p => new { p.UserId, p.MenuId }).ToList()
                .Select(p => (p.UserId, p.MenuId)).ToHashSet();

            foreach (var item in ReadJson<UserAccessMenus>(path, "UserAccessMenus.json"))
            {
                if (existing.Contains((item.UserId, item.MenuId)))
                    continue;

                Stamp(item, adminId);
                eFUserAccessMenusRepository.Add(item);
            }
        }

        // ──────────────────────────── Role / UserRole ────────────────────────────

        private void SeedRolesAndAdminRole(string path, Guid adminId)
        {
            var existing = eFRoleRepository.TableNoTracking.IgnoreQueryFilters()
                .Select(p => p.NormalizedRoleNameEn).ToList().ToHashSet();

            foreach (var role in ReadJson<Role>(path, "Role.json"))
            {
                role.NormalizedRoleNameEn ??= role.RoleNameEn.ToUpperInvariant();

                if (existing.Contains(role.NormalizedRoleNameEn))
                    continue;

                Stamp(role, adminId);
                eFRoleRepository.Add(role);
            }

            var adminRole = eFRoleRepository.TableNoTracking.IgnoreQueryFilters()
                                .FirstOrDefault(p => p.NormalizedRoleNameEn == AdminNormalizedRoleName)
                            ?? throw new InvalidOperationException(
                                $"نقش {AdminNormalizedRoleName} در Role.json پیدا نشد.");

            var hasRole = eFUserRoleRepository.TableNoTracking.IgnoreQueryFilters()
                .Any(p => p.UserId == adminId && p.RoleId == adminRole.Id);

            if (!hasRole)
            {
                var userRole = new UserRole
                {
                    UserId = adminId,
                    RoleId = adminRole.Id
                };
                Stamp(userRole, adminId);
                eFUserRoleRepository.Add(userRole);
            }
        }

        // ─────────────────────────────── کمکی ───────────────────────────────

        /// <summary>فیلدهای ممیزی BaseEntity (به‌جای CreateDate / Creator / HostIp قبلی).</summary>
        private static void Stamp<TKey>(BaseEntity<TKey> entity, Guid createdBy)
        {
            entity.CreatedBy = createdBy;
            entity.CreatedDate = DateTime.UtcNow;
            entity.HostIp = DefaultHostIp;
        }

        private static List<T> ReadJson<T>(string folder, string fileName)
        {
            var fullPath = Path.Combine(folder, fileName);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"فایل داده پیش‌فرض پیدا نشد: {fileName}", fullPath);

            return JsonSerializer.Deserialize<List<T>>(File.ReadAllText(fullPath), JsonOptions)
                   ?? new List<T>();
        }

        /// <summary>
        /// اول کنار فایل‌های خروجی (BaseDirectory)، بعد پوشه‌ی جاری برنامه (مثل نسخه قبلی).
        /// فایل‌ها باید در خروجی کپی شوند (CopyToOutputDirectory).
        /// </summary>
        private static string ResolveDataFolder()
        {
            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, DataFolderName),
                Path.Combine(Environment.CurrentDirectory, DataFolderName)
            };

            return candidates.FirstOrDefault(Directory.Exists)
                   ?? throw new DirectoryNotFoundException(
                       $"پوشه {DataFolderName} پیدا نشد. مسیرهای بررسی‌شده: {string.Join(" | ", candidates)}");
        }
    }
}