using SanjeshP.Payeh.Api.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Api.Models.Auth
{
    /// <summary>تنظیمات عمومی ورود برای فرانت (بدون هیچ مقدار محرمانه).</summary>
    public class AuthConfigDto
    {
        public bool CaptchaEnabled { get; set; }
        public string? CaptchaProvider { get; set; }
        /// <summary>کلید عمومی ویجت کپچا؛ فقط وقتی کپچا فعال است مقدار دارد.</summary>
        public string? CaptchaSiteKey { get; set; }
        public bool MobileLoginEnabled { get; set; }
        public bool PasswordLoginEnabled { get; set; }
        /// <summary>اگر true باشد بعد از رمز صحیح برای همه کد پیامکی هم لازم است.</summary>
        public bool PasswordLoginRequiresOtp { get; set; }
        public int OtpLength { get; set; }
    }

    /// <summary>درخواست ارسال کد یک‌بارمصرف به شماره‌ی موبایل.</summary>
    public class RequestOtpDto
    {
        [Required(ErrorMessage = "شماره موبایل الزامی است")]
        [RegularExpression(@"^(\+98|0098|98|0)?9\d{9}$", ErrorMessage = "شماره موبایل معتبر نیست")]
        [Display(Name = "شماره موبایل")]
        public string MobileNumber { get; set; } = null!;

        /// <summary>توکن کپچا؛ فقط وقتی کپچا فعال است الزامی است.</summary>
        [MaxLength(4096)]
        public string? CaptchaToken { get; set; }
    }

    /// <summary>تأیید کد یک‌بارمصرف و ورود با موبایل.</summary>
    public class VerifyOtpDto
    {
        [Required(ErrorMessage = "شماره موبایل الزامی است")]
        [RegularExpression(@"^(\+98|0098|98|0)?9\d{9}$", ErrorMessage = "شماره موبایل معتبر نیست")]
        [Display(Name = "شماره موبایل")]
        public string MobileNumber { get; set; } = null!;

        [Required(ErrorMessage = "کد تأیید الزامی است")]
        [StringLength(10, MinimumLength = 4, ErrorMessage = "کد تأیید باید بین 4 تا 10 رقم باشد")]
        [Display(Name = "کد تأیید")]
        public string Code { get; set; } = null!;

        /// <summary>توکن کپچا؛ فقط وقتی کپچا فعال است الزامی است (هر توکن یک‌بارمصرف است).</summary>
        [MaxLength(4096)]
        public string? CaptchaToken { get; set; }

        [MaxLength(100, ErrorMessage = "نام دستگاه نباید بیش از 100 کاراکتر باشد")]
        [Display(Name = "نام دستگاه")]
        public string? DeviceName { get; set; }
    }

    /// <summary>ورود با نام کاربری و رمز عبور.</summary>
    public class PasswordLoginDto
    {
        [Required(ErrorMessage = "نام کاربری الزامی است")]
        [MaxLength(50, ErrorMessage = "نام کاربری نباید بیش از 50 کاراکتر باشد")]
        [Display(Name = "نام کاربری")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [MaxLength(128, ErrorMessage = "رمز عبور نباید بیش از 128 کاراکتر باشد")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; } = null!;

        [MaxLength(4096)]
        public string? CaptchaToken { get; set; }

        [MaxLength(100, ErrorMessage = "نام دستگاه نباید بیش از 100 کاراکتر باشد")]
        [Display(Name = "نام دستگاه")]
        public string? DeviceName { get; set; }
    }

    /// <summary>اطلاعات مرحله‌ی دوم (کد پیامکی) بعد از رمز صحیح.</summary>
    public class LoginChallengeDto
    {
        /// <summary>شناسه‌ی درخواست ورود؛ در مرحله‌ی بعد همراه کد ارسال می‌شود.</summary>
        public Guid ChallengeId { get; set; }
        /// <summary>شماره‌ی پوشیده، مثل 0937•••1160.</summary>
        public string MaskedMobile { get; set; } = null!;
        public int ExpiresInSeconds { get; set; }
        public int ResendAfterSeconds { get; set; }
        /// <summary>فقط در توسعه (OtpSettings:ReturnCodeInResponse).</summary>
        public string? DevCode { get; set; }
    }

    /// <summary>خروجی ورود با رمز: یا توکن‌ها، یا درخواست کد پیامکی.</summary>
    public class PasswordLoginResponseDto
    {
        public bool RequiresOtp { get; set; }
        public LoginChallengeDto? Challenge { get; set; }
        public AuthResponseDto? Auth { get; set; }
    }

    /// <summary>تأیید کد مرحله‌ی دوم ورود با رمز.</summary>
    public class VerifyLoginChallengeDto
    {
        [Required(ErrorMessage = "شناسه‌ی درخواست ورود الزامی است")]
        public Guid ChallengeId { get; set; }

        [Required(ErrorMessage = "کد تأیید الزامی است")]
        [StringLength(10, MinimumLength = 4, ErrorMessage = "کد تأیید باید بین 4 تا 10 رقم باشد")]
        [Display(Name = "کد تأیید")]
        public string Code { get; set; } = null!;

        [MaxLength(4096)]
        public string? CaptchaToken { get; set; }

        [MaxLength(100, ErrorMessage = "نام دستگاه نباید بیش از 100 کاراکتر باشد")]
        [Display(Name = "نام دستگاه")]
        public string? DeviceName { get; set; }
    }

    /// <summary>ارسال دوباره‌ی کد مرحله‌ی دوم.</summary>
    public class ResendLoginChallengeDto
    {
        [Required(ErrorMessage = "شناسه‌ی درخواست ورود الزامی است")]
        public Guid ChallengeId { get; set; }

        [MaxLength(4096)]
        public string? CaptchaToken { get; set; }
    }

    /// <summary>درخواست توکن جدید با Refresh Token.</summary>
    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "توکن تازه‌سازی الزامی است")]
        [Display(Name = "توکن تازه‌سازی")]
        public string RefreshToken { get; set; } = null!;
    }

    /// <summary>خروجی ورود/Refresh: جفت توکن + اطلاعات نشست.</summary>
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public string TokenType { get; set; } = "Bearer";
        /// <summary>عمر Access Token به ثانیه.</summary>
        public int ExpiresIn { get; set; }
        public Guid SessionId { get; set; }
    }

    /// <summary>کاربر واردشده (برای نمایش در هدر).</summary>
    public class MeDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public List<RoleDto> Roles { get; set; } = new();
    }

    /// <summary>نمایش یک نشست فعال در «دستگاه‌های من».</summary>
    public class SessionDto
    {
        public Guid Id { get; set; }
        public string DeviceName { get; set; } = null!;
        public string? UserAgent { get; set; }
        public string IpAddress { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public DateTime RefreshTokenExpireDate { get; set; }
        /// <summary>آیا این نشست همان دستگاهی است که درخواست از آن می‌آید؟</summary>
        public bool IsCurrent { get; set; }
    }

    /// <summary>خروجی درخواست کد (پیام عمومی؛ ضد افشای عضویت).</summary>
    public class RequestOtpResponseDto
    {
        /// <summary>فقط در توسعه (OtpSettings:ReturnCodeInResponse=true) مقدار دارد.</summary>
        public string? DevCode { get; set; }
        public int ExpiresInSeconds { get; set; }
        public int ResendAfterSeconds { get; set; }
    }
}
