using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Api.Models.Users
{
    /// <summary>خروجی نمایش کاربر (فهرست و جزئیات).</summary>
    public class UserSelectDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = null!;
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? PhoneNumberConfirmed { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTimeOffset? LastLoginDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? NationalCode { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    /// <summary>ورودی ایجاد کاربر. اعتبارسنجی با DataAnnotations و پیام فارسی.</summary>
    public class CreateUserDto
    {
        [Required(ErrorMessage = "نام کاربری الزامی است")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "نام کاربری باید بین 3 تا 50 کاراکتر باشد")]
        [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9._]*$", ErrorMessage = "نام کاربری باید با حرف انگلیسی شروع شود و فقط شامل حروف، عدد، نقطه و زیرخط باشد")]
        [Display(Name = "نام کاربری")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "شماره موبایل الزامی است")]
        [RegularExpression(@"^(\+98|0098|98|0)?9\d{9}$", ErrorMessage = "شماره موبایل معتبر نیست")]
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

        [Required(ErrorMessage = "کد ملی الزامی است")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "کد ملی باید دقیقاً 10 رقم باشد")]
        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; } = null!;

        /// <summary>رمز عبور اختیاری است؛ ورود اصلی با OTP است. در صورت ارسال، هش می‌شود.</summary>
        [MinLength(8, ErrorMessage = "رمز عبور باید حداقل 8 کاراکتر باشد")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string? Password { get; set; }

        /// <summary>شناسه‌ی نقش‌ها (اختیاری).</summary>
        public List<int> RoleIds { get; set; } = new();

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>ورودی ویرایش کاربر؛ فقط فیلدهای ارسالی (غیر null) اعمال می‌شوند.</summary>
    public class UpdateUserDto
    {
        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
        [MaxLength(100, ErrorMessage = "ایمیل نباید بیش از 100 کاراکتر باشد")]
        [Display(Name = "ایمیل")]
        public string? EmailAddress { get; set; }

        [RegularExpression(@"^(\+98|0098|98|0)?9\d{9}$", ErrorMessage = "شماره موبایل معتبر نیست")]
        [Display(Name = "شماره موبایل")]
        public string? PhoneNumber { get; set; }

        [MaxLength(50, ErrorMessage = "نام نباید بیش از 50 کاراکتر باشد")]
        [Display(Name = "نام")]
        public string? FirstName { get; set; }

        [MaxLength(50, ErrorMessage = "نام خانوادگی نباید بیش از 50 کاراکتر باشد")]
        [Display(Name = "نام خانوادگی")]
        public string? LastName { get; set; }

        [Display(Name = "فعال")]
        public bool? IsActive { get; set; }

        /// <summary>در صورت ارسال، جایگزین همه‌ی نقش‌های فعلی می‌شود.</summary>
        public List<int>? RoleIds { get; set; }
    }

    /// <summary>پارامترهای صفحه‌بندی، جستجو، مرتب‌سازی و فیلتر فهرست کاربران.</summary>
    public class UserQueryParameters
    {
        private int _pageNumber = 1;
        private int _pageSize = 10;

        /// <summary>شماره‌ی صفحه (از 1).</summary>
        [Range(1, int.MaxValue, ErrorMessage = "شماره صفحه باید بزرگ‌تر از صفر باشد")]
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        /// <summary>اندازه‌ی صفحه؛ حداکثر 100 برای محافظت از سرور.</summary>
        [Range(1, 100, ErrorMessage = "اندازه صفحه باید بین 1 تا 100 باشد")]
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value is < 1 or > 100 ? 10 : value;
        }

        /// <summary>جستجو در نام کاربری، موبایل، ایمیل، نام و نام خانوادگی.</summary>
        [MaxLength(100, ErrorMessage = "عبارت جستجو نباید بیش از 100 کاراکتر باشد")]
        public string? Search { get; set; }

        /// <summary>فیلتر وضعیت فعال/غیرفعال.</summary>
        public bool? IsActive { get; set; }

        /// <summary>فیلتر بر اساس شناسه‌ی نقش.</summary>
        public int? RoleId { get; set; }

        /// <summary>فیلد مرتب‌سازی: userName | createdDate | lastLoginDate | email (پیش‌فرض createdDate).</summary>
        public string? SortBy { get; set; }

        /// <summary>جهت مرتب‌سازی: asc | desc (پیش‌فرض desc).</summary>
        public string? SortDirection { get; set; }
    }
}