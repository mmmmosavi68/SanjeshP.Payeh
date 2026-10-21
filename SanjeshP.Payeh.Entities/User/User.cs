using SanjeshP.Payeh.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Entities.User
{
    /// <summary>
    /// کاربر سیستم.
    /// تغییرات این نسخه:
    /// ۱) PasswordHash اختیاری شد چون ورود اصلی با OTP پیامکی است (Breaking در سطح دیتابیس — ستون NULLپذیر می‌شود).
    /// ۲) مجموعه‌ی UserTokens به UserSessions تغییر نام یافت (هم‌راستا با تغییر نام موجودیت).
    /// ۳) NormalizedUserName/NormalizedEmailAddress در سرویس مقداردهی می‌شوند (Invariant Upper).
    /// </summary>
    public class User : BaseEntity
    {
        public User()
        {
            SecurityStamp = Guid.NewGuid();
            ExpireDate = DateTime.UtcNow.AddYears(1);
        }

        [Required(ErrorMessage = "نام کاربری الزامی است")]
        [MaxLength(50, ErrorMessage = "نام کاربری نباید بیش از 50 کاراکتر باشد")]
        [Display(Name = "نام کاربری")]
        public string UserName { get; set; } = null!;

        [MaxLength(50, ErrorMessage = "نام کاربری نرمال نباید بیش از 50 کاراکتر باشد")]
        [Display(Name = "نام کاربری نرمال")]
        public string? NormalizedUserName { get; set; }

        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
        [MaxLength(100, ErrorMessage = "ایمیل نباید بیش از 100 کاراکتر باشد")]
        [Display(Name = "ایمیل")]
        public string? EmailAddress { get; set; }

        [MaxLength(100, ErrorMessage = "ایمیل نرمال نباید بیش از 100 کاراکتر باشد")]
        [Display(Name = "ایمیل نرمال")]
        public string? NormalizedEmailAddress { get; set; }

        [Display(Name = "ایمیل تأیید شده")]
        public bool? EmailAddressConfirmed { get; set; }

        /// <summary>
        /// هش رمز عبور (الگوریتم PBKDF2 از PasswordHasher).
        /// برای کاربرانی که فقط با OTP وارد می‌شوند، null است.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string? PasswordHash { get; set; }

        public Guid? SecurityStamp { get; set; }
        public Guid? ConcurrencyStamp { get; set; }

        [Display(Name = "شماره تلفن")]
        [Phone(ErrorMessage = "شماره تلفن معتبر نیست")]
        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [Display(Name = "تلفن تأیید شده")]
        public bool? PhoneNumberConfirmed { get; set; }

        [Display(Name = "فعالیت دو مرحله‌ای")]
        public bool? TwoFactorEnabled { get; set; }

        [Display(Name = "تاریخ پایان قفل")]
        public DateTimeOffset? LockoutEnd { get; set; }

        [Display(Name = "قفل فعال است")]
        public bool? LockoutEnabled { get; set; }

        [Display(Name = "تعداد دسترسی‌های ناموفق")]
        public int? AccessFailedCount { get; set; }

        [Display(Name = "تاریخ آخرین ورود")]
        public DateTimeOffset? LastLoginDate { get; set; }

        [Required(ErrorMessage = "تاریخ انقضاء الزامی است")]
        [Display(Name = "تاریخ انقضاء")]
        public DateTime ExpireDate { get; set; }

        public virtual ICollection<UserProfile> UserProfile { get; set; } = new List<UserProfile>();
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
        public virtual ICollection<Menu.UserAccessMenus> MenusAccesses { get; set; } = new List<Menu.UserAccessMenus>();
        public virtual ICollection<Group.Group> Groups { get; set; } = new List<Group.Group>();
    }
}