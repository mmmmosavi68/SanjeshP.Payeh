using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Api.Models.Auth
{
    /// <summary>درخواست ارسال کد یک‌بارمصرف به شماره‌ی موبایل.</summary>
    public class RequestOtpDto
    {
        /// <summary>شماره موبایل با فرمت 09xxxxxxxxx (ارقام فارسی و +98 هم پذیرفته می‌شود).</summary>
        [Required(ErrorMessage = "شماره موبایل الزامی است")]
        [RegularExpression(@"^(\+98|0098|98|0)?9\d{9}$", ErrorMessage = "شماره موبایل معتبر نیست")]
        [Display(Name = "شماره موبایل")]
        public string MobileNumber { get; set; } = null!;

        /// <summary>توکن کپچا؛ فقط وقتی CaptchaSettings:Enabled=true باشد الزامی است.</summary>
        public string? CaptchaToken { get; set; }
    }

    /// <summary>تأیید کد یک‌بارمصرف و ورود.</summary>
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

        /// <summary>نام دستگاه برای نمایش در «فهرست نشست‌های من» (اختیاری).</summary>
        [MaxLength(100, ErrorMessage = "نام دستگاه نباید بیش از 100 کاراکتر باشد")]
        [Display(Name = "نام دستگاه")]
        public string? DeviceName { get; set; }
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
        /// <summary>شناسه‌ی نشست فعلی (برای خروج انتخابی).</summary>
        public Guid SessionId { get; set; }
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
        /// <summary>آیا این نشست، نشستِ درخواست‌دهنده‌ی فعلی است؟</summary>
        public bool IsCurrent { get; set; }
    }

    /// <summary>خروجی درخواست کد (پیام عمومی؛ ضد افشای عضویت).</summary>
    public class RequestOtpResponseDto
    {
        /// <summary>فقط در محیط توسعه و با فعال بودن OtpSettings:ReturnCodeInResponse مقدار دارد.</summary>
        public string? DevCode { get; set; }
        public int ExpiresInSeconds { get; set; }
        public int ResendAfterSeconds { get; set; }
    }
}