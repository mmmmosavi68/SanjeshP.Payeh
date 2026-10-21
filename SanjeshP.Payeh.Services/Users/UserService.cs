using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.Data;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Entities.User;

namespace SanjeshP.Payeh.Services.Users
{
    /// <summary>
    /// پیاده‌سازی مدیریت کاربران روی مخزن عمومی EF.
    /// مرتب‌سازی فقط از طریق لیست سفید انجام می‌شود تا از تزریق نام ستون جلوگیری شود.
    /// </summary>
    public class UserService : IUserService, IScopedDependency
    {
        // لیست سفید فیلدهای مرتب‌سازی (کلیدِ نرمال‌شده → ستون واقعی)
        private static readonly Dictionary<string, string> SortFields = new(StringComparer.OrdinalIgnoreCase)
        {
            ["username"] = nameof(User.UserName),
            ["createddate"] = nameof(User.CreatedDate),
            ["lastlogindate"] = nameof(User.LastLoginDate),
            ["email"] = nameof(User.EmailAddress)
        };

        private readonly IEFRepository<User> _userRepository;
        private readonly ApplicationDbContext _dbContext;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(
            IEFRepository<User> userRepository,
            ApplicationDbContext dbContext,
            IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
        }

        /// <inheritdoc />
        public async Task<PagedUsers> GetPagedAsync(UserQuery query, CancellationToken cancellationToken)
        {
            var users = _userRepository.TableNoTracking
                .Where(x => !x.IsDeleted);

            // جستجو در نام کاربری، موبایل، ایمیل و پروفایل
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();
                users = users.Where(x =>
                    x.UserName.Contains(search) ||
                    (x.PhoneNumber != null && x.PhoneNumber.Contains(search)) ||
                    (x.EmailAddress != null && x.EmailAddress.Contains(search)) ||
                    x.UserProfile.Any(p => p.FirstName.Contains(search) || p.LastName.Contains(search) || p.NationalCode.Contains(search)));
            }

            if (query.IsActive.HasValue)
                users = users.Where(x => x.IsActive == query.IsActive.Value);

            if (query.RoleId.HasValue)
                users = users.Where(x => x.UserRoles.Any(ur => ur.RoleId == query.RoleId.Value));

            var totalCount = await users.CountAsync(cancellationToken);

            users = ApplySorting(users, query.SortBy, query.SortDirection);

            var items = await users
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Include(x => x.UserProfile)
                .Include(x => x.UserRoles).ThenInclude(ur => ur.Role)
                .ToListAsync(cancellationToken);

            return new PagedUsers(items, totalCount, query.PageNumber, query.PageSize);
        }

        /// <inheritdoc />
        public async Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var user = await _userRepository.TableNoTracking
                .Where(x => x.Id == id && !x.IsDeleted)
                .Include(x => x.UserProfile)
                .Include(x => x.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(cancellationToken);

            return user ?? throw new NotFoundException("کاربر مورد نظر یافت نشد.");
        }

        /// <inheritdoc />
        public async Task<User> CreateAsync(User user, string? plainPassword, IEnumerable<int> roleIds, string? firstName, string? lastName, string? nationalCode, Guid createdBy, CancellationToken cancellationToken)
        {
            await EnsureUniqueAsync(user.UserName, user.PhoneNumber, excludeUserId: null, cancellationToken);

            user.NormalizedUserName = user.UserName.ToUpperInvariant();
            user.NormalizedEmailAddress = user.EmailAddress?.ToUpperInvariant();
            user.CreatedBy = createdBy;
            if (!string.IsNullOrWhiteSpace(plainPassword))
                user.PasswordHash = _passwordHasher.HashPassword(user, plainPassword);

            var roleIdList = roleIds.Distinct().ToList();
            await EnsureRolesExistAsync(roleIdList, cancellationToken);
            foreach (var roleId in roleIdList)
                user.UserRoles.Add(new UserRole { RoleId = roleId, CreatedBy = createdBy });

            if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
            {
                user.UserProfile.Add(new UserProfile
                {
                    FirstName = firstName.Trim(),
                    LastName = lastName.Trim(),
                    NationalCode = nationalCode ?? string.Empty,
                    CreatedBy = createdBy
                });
            }

            await _userRepository.AddAsync(user, cancellationToken);
            return await GetByIdAsync(user.Id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<User> UpdateAsync(Guid id, Action<User> applyChanges, IEnumerable<int>? roleIds, string? firstName, string? lastName, CancellationToken cancellationToken)
        {
            var user = await _userRepository.Table
                .Include(x => x.UserProfile)
                .Include(x => x.UserRoles)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken)
                ?? throw new NotFoundException("کاربر مورد نظر یافت نشد.");

            applyChanges(user);

            if (user.PhoneNumber is not null)
                await EnsureUniqueAsync(user.UserName, user.PhoneNumber, excludeUserId: id, cancellationToken);

            user.NormalizedUserName = user.UserName.ToUpperInvariant();
            user.NormalizedEmailAddress = user.EmailAddress?.ToUpperInvariant();
            user.UpdatedDate = DateTime.UtcNow;

            // به‌روزرسانی پروفایل (اولین رکورد)
            var profile = user.UserProfile.FirstOrDefault();
            if (profile is not null)
            {
                if (!string.IsNullOrWhiteSpace(firstName)) profile.FirstName = firstName.Trim();
                if (!string.IsNullOrWhiteSpace(lastName)) profile.LastName = lastName.Trim();
                profile.UpdatedDate = DateTime.UtcNow;
            }

            // جایگزینی نقش‌ها در صورت ارسال
            if (roleIds is not null)
            {
                var roleIdList = roleIds.Distinct().ToList();
                await EnsureRolesExistAsync(roleIdList, cancellationToken);

                _dbContext.RemoveRange(user.UserRoles.Where(ur => !roleIdList.Contains(ur.RoleId)));
                foreach (var roleId in roleIdList.Where(r => user.UserRoles.All(ur => ur.RoleId != r)))
                    user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId, CreatedBy = user.Id });
            }

            _userRepository.Update(user, saveNow: false);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var user = await _userRepository.Table.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken)
                ?? throw new NotFoundException("کاربر مورد نظر یافت نشد.");

            user.IsDeleted = true;
            user.IsActive = false;
            user.UpdatedDate = DateTime.UtcNow;

            // همه‌ی نشست‌های کاربر حذف‌شده باید فوراً باطل شوند.
            var sessions = await _dbContext.Set<UserSession>()
                .Where(x => x.UserId == id && x.RevocationDate == null)
                .ToListAsync(cancellationToken);
            foreach (var session in sessions)
                session.RevocationDate = DateTime.UtcNow;

            _userRepository.Update(user, saveNow: false);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>مرتب‌سازی امن بر اساس لیست سفید؛ مقدار نامعتبر ⇒ پیش‌فرض createdDate desc.</summary>
        private static IQueryable<User> ApplySorting(IQueryable<User> users, string? sortBy, string? sortDirection)
        {
            var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            var field = sortBy is not null && SortFields.TryGetValue(sortBy.Replace("_", ""), out var mapped)
                ? mapped
                : nameof(User.CreatedDate);

            return (field, descending) switch
            {
                (nameof(User.UserName), false) => users.OrderBy(x => x.UserName),
                (nameof(User.UserName), true) => users.OrderByDescending(x => x.UserName),
                (nameof(User.LastLoginDate), false) => users.OrderBy(x => x.LastLoginDate),
                (nameof(User.LastLoginDate), true) => users.OrderByDescending(x => x.LastLoginDate),
                (nameof(User.EmailAddress), false) => users.OrderBy(x => x.EmailAddress),
                (nameof(User.EmailAddress), true) => users.OrderByDescending(x => x.EmailAddress),
                (_, false) => users.OrderBy(x => x.CreatedDate),
                (_, true) => users.OrderByDescending(x => x.CreatedDate)
            };
        }

        /// <summary>جلوگیری از نام کاربری / موبایل تکراری بین کاربران حذف‌نشده (HTTP 409).</summary>
        private async Task EnsureUniqueAsync(string userName, string? phoneNumber, Guid? excludeUserId, CancellationToken cancellationToken)
        {
            var normalized = userName.ToUpperInvariant();
            var duplicated = await _userRepository.TableNoTracking.AnyAsync(x =>
                !x.IsDeleted &&
                (excludeUserId == null || x.Id != excludeUserId) &&
                (x.NormalizedUserName == normalized || (phoneNumber != null && x.PhoneNumber == phoneNumber)),
                cancellationToken);

            if (duplicated)
                throw new ConflictException("نام کاربری یا شماره موبایل قبلاً ثبت شده است.");
        }

        private async Task EnsureRolesExistAsync(List<int> roleIds, CancellationToken cancellationToken)
        {
            if (roleIds.Count == 0) return;
            var existingCount = await _dbContext.Set<Role>().CountAsync(r => roleIds.Contains(r.Id) && !r.IsDeleted, cancellationToken);
            if (existingCount != roleIds.Count)
                throw new BadRequestException("یک یا چند نقش ارسالی معتبر نیستند.");
        }
    }
}