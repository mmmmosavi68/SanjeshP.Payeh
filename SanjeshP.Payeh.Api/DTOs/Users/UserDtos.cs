using SanjeshP.Payeh.Entities.User;
using SanjeshP.Payeh.WebFramework.Api;
using SanjeshP.Payeh.WebFramework.CustomMapping;
using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Api.Models.Users
{
    /// <summary>
    /// خروجی نمایش کاربر در فهرست و جزئیات.
    /// </summary>
    public class UserSelectDto : BaseDto<UserSelectDto, User, Guid>
    {
        public string UserName { get; set; } = null!;
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? PhoneNumberConfirmed { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTimeOffset? LastLoginDate { get; set; }
        public DateTime ExpireDate { get; set; }

        // اطلاعات پروفایل
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FatherName { get; set; }
        public string? NationalCode { get; set; }
        public GenderType? Gender { get; set; }
        public MaritalStatusType? MaritalStatus { get; set; }
        public int? Religion { get; set; }
        public LeftHandedType? LeftHanded { get; set; }
        public int? Nationality { get; set; }

        public List<RoleDto> Roles { get; set; } = new();
    }

    /// <summary>
    /// ورودی ایجاد کاربر.
    /// </summary>
    public class CreateUserDto : BaseDto<CreateUserDto, User, Guid>
    {
        [Required(ErrorMessage = "نام کاربری الزامی است")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "نام کاربری باید بین 3 تا 50 کاراکتر باشد")]
        [RegularExpression(
            @"^[a-zA-Z][a-zA-Z0-9._]*$",
            ErrorMessage = "نام کاربری باید با حرف انگلیسی شروع شود و فقط شامل حروف، عدد، نقطه و زیرخط باشد")]
        [Display(Name = "نام کاربری")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "شماره موبایل الزامی است")]
        [RegularExpression(
            @"^(\+98|0098|98|0)?9\d{9}$",
            ErrorMessage = "شماره موبایل معتبر نیست")]
        [Display(Name = "شماره موبایل")]
        public string PhoneNumber { get; set; } = null!;

        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
        [MaxLength(100, ErrorMessage = "ایمیل نباید بیش از 100 کاراکتر باشد")]
        [Display(Name = "ایمیل")]
        public string? EmailAddress { get; set; }

        [Required(ErrorMessage = "نام الزامی است")]
        [MaxLength(50, ErrorMessage = "نام نباید بیش از 50 کاراکتر باشد")]
        [Display(Name = "نام")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        [MaxLength(50, ErrorMessage = "نام خانوادگی نباید بیش از 50 کاراکتر باشد")]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; } = null!;

        [MaxLength(50, ErrorMessage = "نام پدر نباید بیش از 50 کاراکتر باشد")]
        [Display(Name = "نام پدر")]
        public string? FatherName { get; set; }

        [Required(ErrorMessage = "کد ملی الزامی است")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "کد ملی باید دقیقاً 10 رقم باشد")]
        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; } = null!;

        [Display(Name = "جنسیت")]
        public GenderType? Gender { get; set; }

        [Display(Name = "وضعیت تأهل")]
        public MaritalStatusType? MaritalStatus { get; set; }

        [Display(Name = "دین")]
        public int? Religion { get; set; }

        [Display(Name = "چپ‌دستی")]
        public LeftHandedType? LeftHanded { get; set; }

        [Display(Name = "ملیت")]
        public int? Nationality { get; set; }

        /// <summary>
        /// رمز اولیه اختیاری است؛ در صورت ارسال، در سرویس هش می‌شود.
        /// </summary>
        [MinLength(8, ErrorMessage = "رمز عبور باید حداقل 8 کاراکتر باشد")]
        [MaxLength(128, ErrorMessage = "رمز عبور نباید بیش از 128 کاراکتر باشد")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string? Password { get; set; }

        /// <summary>شناسه نقش‌های کاربر.</summary>
        public List<int> RoleIds { get; set; } = new();

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "تاریخ انقضاء الزامی است")]
        [Display(Name = "تاریخ انقضاء")]
        public DateTime ExpireDate { get; set; } = DateTime.UtcNow.AddYears(1);
    }

    /// <summary>
    /// ورودی ویرایش کاربر.
    /// فیلد null یعنی مقدار فعلی حفظ شود.
    /// </summary>
    public class UpdateUserDto
    {
        [StringLength(50, MinimumLength = 3, ErrorMessage = "نام کاربری باید بین 3 تا 50 کاراکتر باشد")]
        [RegularExpression(
            @"^[a-zA-Z][a-zA-Z0-9._]*$",
            ErrorMessage = "نام کاربری باید با حرف انگلیسی شروع شود و فقط شامل حروف، عدد، نقطه و زیرخط باشد")]
        [Display(Name = "نام کاربری")]
        public string? UserName { get; set; }

        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
        [MaxLength(100, ErrorMessage = "ایمیل نباید بیش از 100 کاراکتر باشد")]
        [Display(Name = "ایمیل")]
        public string? EmailAddress { get; set; }

        [RegularExpression(
            @"^(\+98|0098|98|0)?9\d{9}$",
            ErrorMessage = "شماره موبایل معتبر نیست")]
        [Display(Name = "شماره موبایل")]
        public string? PhoneNumber { get; set; }

        [MaxLength(50, ErrorMessage = "نام نباید بیش از 50 کاراکتر باشد")]
        [Display(Name = "نام")]
        public string? FirstName { get; set; }

        [MaxLength(50, ErrorMessage = "نام خانوادگی نباید بیش از 50 کاراکتر باشد")]
        [Display(Name = "نام خانوادگی")]
        public string? LastName { get; set; }

        [MaxLength(50, ErrorMessage = "نام پدر نباید بیش از 50 کاراکتر باشد")]
        [Display(Name = "نام پدر")]
        public string? FatherName { get; set; }

        [RegularExpression(@"^\d{10}$", ErrorMessage = "کد ملی باید دقیقاً 10 رقم باشد")]
        [Display(Name = "کد ملی")]
        public string? NationalCode { get; set; }

        [Display(Name = "جنسیت")]
        public GenderType? Gender { get; set; }

        [Display(Name = "وضعیت تأهل")]
        public MaritalStatusType? MaritalStatus { get; set; }

        [Display(Name = "دین")]
        public int? Religion { get; set; }

        [Display(Name = "چپ‌دستی")]
        public LeftHandedType? LeftHanded { get; set; }

        [Display(Name = "ملیت")]
        public int? Nationality { get; set; }

        [Display(Name = "فعال")]
        public bool? IsActive { get; set; }

        [Display(Name = "تاریخ انقضاء")]
        public DateTime? ExpireDate { get; set; }

        /// <summary>
        /// در صورت ارسال، جایگزین همه نقش‌های فعلی می‌شود.
        /// </summary>
        public List<int>? RoleIds { get; set; }

        /// <summary>
        /// اختیاری: null یا رشته خالی یعنی رمز فعلی تغییر نکند.
        /// </summary>
        [MinLength(8, ErrorMessage = "رمز عبور باید حداقل 8 کاراکتر باشد")]
        [MaxLength(128, ErrorMessage = "رمز عبور نباید بیش از 128 کاراکتر باشد")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور جدید")]
        public string? Password { get; set; }
    }

    /// <summary>نقش با عنوان فارسی و انگلیسی.</summary>
    public class RoleDto
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = null!;
        public string NameFa { get; set; } = null!;
    }

    /// <summary>پارامترهای فهرست کاربران.</summary>
    public class UserQueryParameters
    {
        private int _pageNumber = 1;
        private int _pageSize = 10;

        [Range(1, int.MaxValue, ErrorMessage = "شماره صفحه باید بزرگ‌تر از صفر باشد")]
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        [Range(1, 100, ErrorMessage = "اندازه صفحه باید بین 1 تا 100 باشد")]
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value is < 1 or > 100 ? 10 : value;
        }

        [MaxLength(100, ErrorMessage = "عبارت جستجو نباید بیش از 100 کاراکتر باشد")]
        public string? Search { get; set; }

        public bool? IsActive { get; set; }
        public int? RoleId { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
    }
}