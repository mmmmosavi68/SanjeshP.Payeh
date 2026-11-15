using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.Data;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Entities.User;
using System.Net.Mail;

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

        public UserService(IEFRepository<User> userRepository, ApplicationDbContext dbContext, IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
        }

        public async Task<User?> ValidateCredentialsAsync(string userName, string password, CancellationToken cancellationToken)
        {
            var normalized = userName.Trim().ToUpperInvariant();

            // نقش‌ها لازم‌اند چون JwtService در ساخت توکن از user.UserRoles می‌خواند
            var user = await _userRepository.Table
                .Include(x => x.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(x => x.NormalizedUserName == normalized && !x.IsDeleted && x.IsActive, cancellationToken);

            if (user is null || string.IsNullOrEmpty(user.PasswordHash))
            {
                // هزینه‌ی زمانی یکسان تا از روی سرعت پاسخ نتوان وجود کاربر را فهمید
                _passwordHasher.HashPassword(new User(), password);
                return null;
            }

            if (user.LockoutEnd is { } lockEnd && lockEnd > DateTimeOffset.UtcNow)
                return null;

            PasswordVerificationResult result;
            try { result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password); }
            catch (FormatException) { result = PasswordVerificationResult.Failed; } // هش با فرمت دیگر (مثل BCrypt)

            if (result == PasswordVerificationResult.Failed)
            {
                user.AccessFailedCount = (user.AccessFailedCount ?? 0) + 1;
                if (user.AccessFailedCount >= 5)
                {
                    user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(15);
                    user.AccessFailedCount = 0;
                }
                await _dbContext.SaveChangesAsync(cancellationToken);
                return null;
            }

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
                user.PasswordHash = _passwordHasher.HashPassword(user, password);

            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
            user.LastLoginDate = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return user;
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

        /// <inheritdoc />
        public async Task SetPasswordAsync(Guid id, string newPassword, CancellationToken cancellationToken)
        {
            var user = await _userRepository.Table.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken)
                ?? throw new NotFoundException("کاربر مورد نظر یافت نشد.");

            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            user.SecurityStamp = Guid.NewGuid();
            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
            user.UpdatedDate = DateTime.UtcNow;

            // با تغییر رمز، همه‌ی نشست‌های فعال باطل می‌شوند.
            var sessions = await _dbContext.Set<UserSession>()
                .Where(x => x.UserId == id && x.RevocationDate == null)
                .ToListAsync(cancellationToken);
            foreach (var session in sessions)
                session.RevocationDate = DateTime.UtcNow;

            _userRepository.Update(user, saveNow: false);
            await _dbContext.SaveChangesAsync(cancellationToken);
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
        public async Task<User> CreateAsync(
    User user,
    string? plainPassword,
    IEnumerable<int> roleIds,
    UserProfile profile,
    Guid createdBy,
    CancellationToken cancellationToken)
        {
            await EnsureUniqueAsync(
                user.UserName,
                user.PhoneNumber,
                excludeUserId: null,
                cancellationToken);

            user.UserName = user.UserName.Trim();
            user.NormalizedUserName = user.UserName.ToUpperInvariant();

            user.EmailAddress = string.IsNullOrWhiteSpace(user.EmailAddress)
                ? null
                : user.EmailAddress.Trim();

            user.NormalizedEmailAddress = user.EmailAddress?.ToUpperInvariant();

            user.CreatedBy = createdBy;

            if (!string.IsNullOrWhiteSpace(plainPassword))
                user.PasswordHash = _passwordHasher.HashPassword(user, plainPassword);

            if (user.ExpireDate == default)
                user.ExpireDate = DateTime.UtcNow.AddYears(1);

            var roleIdList = roleIds.Distinct().ToList();

            await EnsureRolesExistAsync(roleIdList, cancellationToken);
            await EnsureEmailUniqueAsync(
                user.NormalizedEmailAddress,
                null,
                cancellationToken);

            profile.FirstName = profile.FirstName.Trim();
            profile.LastName = profile.LastName.Trim();
            profile.NationalCode = profile.NationalCode.Trim();

            await EnsureNationalCodeUniqueAsync(
                profile.NationalCode,
                null,
                cancellationToken);

            profile.CreatedBy = createdBy;
            profile.User = user;

            user.UserProfile.Add(profile);

            foreach (var roleId in roleIdList)
            {
                user.UserRoles.Add(new UserRole
                {
                    RoleId = roleId,
                    CreatedBy = createdBy
                });
            }

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(
                    "درخواست قبل از اجرای کوئری لغو شده است.");

            await _userRepository.AddAsync(user, cancellationToken);

            return await GetByIdAsync(user.Id, cancellationToken);
        }

        public async Task<User> UpdateAsync(
    Guid id,
    Action<User> applyChanges,
    Action<UserProfile> applyProfileChanges,
    IEnumerable<int>? roleIds,
    CancellationToken cancellationToken)
        {
            var user = await _userRepository.Table
                .Include(x => x.UserProfile)
                .Include(x => x.UserRoles)
                .FirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDeleted,
                    cancellationToken)
                ?? throw new NotFoundException("کاربر مورد نظر یافت نشد.");

            // تغییر اطلاعات اصلی کاربر
            applyChanges(user);

            user.UserName = user.UserName.Trim();

            // بررسی تکراری نبودن نام کاربری و موبایل
            await EnsureUniqueAsync(
                user.UserName,
                user.PhoneNumber,
                id,
                cancellationToken);

            // نرمال‌سازی ایمیل
            user.EmailAddress = string.IsNullOrWhiteSpace(user.EmailAddress)
                ? null
                : user.EmailAddress.Trim();

            user.NormalizedUserName = user.UserName.ToUpperInvariant();
            user.NormalizedEmailAddress = user.EmailAddress?.ToUpperInvariant();

            // بررسی تکراری نبودن ایمیل
            await EnsureEmailUniqueAsync(
                user.NormalizedEmailAddress,
                id,
                cancellationToken);

            // اعمال تغییرات پروفایل
            var profile = user.UserProfile.FirstOrDefault();

            if (profile is null)
            {
                profile = new UserProfile
                {
                    UserId = user.Id,
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    NationalCode = string.Empty,
                    CreatedBy = user.Id
                };

                user.UserProfile.Add(profile);
            }

            applyProfileChanges(profile);

            profile.FirstName = profile.FirstName.Trim();
            profile.LastName = profile.LastName.Trim();
            profile.NationalCode = profile.NationalCode.Trim();

            await EnsureNationalCodeUniqueAsync(
                profile.NationalCode,
                id,
                cancellationToken);

            profile.UpdatedDate = DateTime.UtcNow;
            user.UpdatedDate = DateTime.UtcNow;

            // جایگزینی نقش‌ها در صورت ارسال
            if (roleIds is not null)
            {
                var roleIdList = roleIds.Distinct().ToList();

                await EnsureRolesExistAsync(roleIdList, cancellationToken);

                _dbContext.RemoveRange(
                    user.UserRoles.Where(
                        ur => !roleIdList.Contains(ur.RoleId)));

                foreach (var roleId in roleIdList.Where(
                    r => user.UserRoles.All(ur => ur.RoleId != r)))
                {
                    user.UserRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = roleId,
                        CreatedBy = user.Id
                    });
                }
            }

            _userRepository.Update(user, saveNow: false);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(id, cancellationToken);
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

            var user = _userRepository.TableNoTracking
                .Where(x =>
                    !x.IsDeleted &&
                    (excludeUserId == null || x.Id != excludeUserId));

            // بررسی تکراری بودن نام کاربری
            var userNameExists = await user.AnyAsync(
                x => x.NormalizedUserName == normalized,
                cancellationToken);

            if (userNameExists)
                throw new ConflictException("این نام کاربری قبلاً ثبت شده است.");

            // بررسی تکراری بودن شماره موبایل
            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                var phoneExists = await user.AnyAsync(
                    x => x.PhoneNumber == phoneNumber,
                    cancellationToken);

                if (phoneExists)
                    throw new ConflictException("این شماره موبایل قبلاً ثبت شده است.");
            }

            var userProfile = _userRepository.TableNoTracking
               .Where(x =>
                   !x.IsDeleted &&
                   (excludeUserId == null || x.Id != excludeUserId));
        }

        private async Task EnsureRolesExistAsync(List<int> roleIds, CancellationToken cancellationToken)
        {
            if (roleIds.Count == 0) return;
            var existingCount = await _dbContext.Set<Role>().CountAsync(r => roleIds.Contains(r.Id) && !r.IsDeleted, cancellationToken);
            if (existingCount != roleIds.Count)
                throw new BadRequestException("یک یا چند نقش ارسالی معتبر نیستند.");
        }

        /// <summary>بررسی تکراری نبودن کد ملی کاربران در ایجاد و ویرایش.</summary>
        private async Task EnsureNationalCodeUniqueAsync(string? nationalCode, Guid? excludeUserId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(nationalCode))
                return;

            var normalizedNationalCode = nationalCode.Trim();

            var exists = await _dbContext.Set<UserProfile>().AnyAsync(
                up => !up.IsDeleted &&
                      up.NationalCode == normalizedNationalCode &&
                      (excludeUserId == null || up.UserId != excludeUserId),
                cancellationToken);

            if (exists)
                throw new ConflictException("این کد ملی قبلاً ثبت شده است.");
        }

        /// <summary>بررسی تکراری نبودن ایمیل کاربران در ایجاد و ویرایش.</summary>
        private async Task EnsureEmailUniqueAsync(string? NormalizedEmailAddress, Guid? excludeUserId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(NormalizedEmailAddress))
                return;

            var exists = await _dbContext.Set<User>().AnyAsync(
                u => !u.IsDeleted &&
                     u.NormalizedEmailAddress == NormalizedEmailAddress &&
                     (excludeUserId == null || u.Id != excludeUserId),
                cancellationToken);

            if (exists)
                throw new ConflictException("این ایمیل قبلاً ثبت شده است.");
        }
    }
}