using SanjeshP.Payeh.Entities.Common;
using SanjeshP.Payeh.Entities.Menu;
using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Entities.User
{
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
        public string UserName { get; set; }

        [MaxLength(50, ErrorMessage = "نام کاربری نرمال نباید بیش از 50 کاراکتر باشد")]
        [Display(Name = "نام کاربری نرمال")]
        public string NormalizedUserName { get; set; }

        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
        [MaxLength(100, ErrorMessage = "ایمیل نباید بیش از 100 کاراکتر باشد")]
        [Display(Name = "ایمیل")]
        public string EmailAddress { get; set; }

        [MaxLength(100, ErrorMessage = "ایمیل نرمال نباید بیش از 100 کاراکتر باشد")]
        [Display(Name = "ایمیل نرمال")]
        public string NormalizedEmailAddress { get; set; }

        [Display(Name = "ایمیل تأیید شده")]
        public bool? EmailAddressConfirmed { get; set; }

        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string PasswordHash { get; set; }

        public Guid? SecurityStamp { get; set; }
        public Guid? ConcurrencyStamp { get; set; }

        [Display(Name = "شماره تلفن")]
        [Phone(ErrorMessage = "شماره تلفن معتبر نیست")]
        public string PhoneNumber { get; set; }

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
        public virtual ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();
        public virtual ICollection<UserAccessMenus> MenusAccesses { get; set; } = new List<UserAccessMenus>();
        public virtual ICollection<Group.Group> Groups { get; set; } = new List<Group.Group>();

    }
}
