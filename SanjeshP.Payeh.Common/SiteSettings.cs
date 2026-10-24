namespace SanjeshP.Payeh.Common
{
    /// <summary>
    /// تنظیمات سراسری سایت که از بخش SiteSettings در appsettings بایند می‌شود.
    /// بخش‌های جدید (Otp، Captcha، Sms، RefreshToken، ApiDocs) افزودنی هستند و
    /// مقادیر پیش‌فرض امن دارند؛ بنابراین بدون تغییر appsettings هم برنامه اجرا می‌شود.
    /// </summary>
    public class SiteSettings
    {
        public string ElmahPath { get; set; } = string.Empty;
        public JwtSettings JwtSettings { get; set; } = new();

        // اصلاح نام‌گذاری: identitySettings → IdentitySettings (طبق استاندارد PascalCase).
        // بایندر Configuration نسبت به بزرگی/کوچکی حروف حساس نیست؛ Breaking نیست.
        public IdentitySettings IdentitySettings { get; set; } = new();

        /// <summary>اجازه‌ی ورود هم‌زمان روی چند دستگاه. اگر false شود، هر ورود جدید بقیه‌ی نشست‌ها را باطل می‌کند.</summary>
        public bool AllowMultipleSessions { get; set; } = true;

        public OtpSettings OtpSettings { get; set; } = new();
        public CaptchaSettings CaptchaSettings { get; set; } = new();
        public SmsSettings SmsSettings { get; set; } = new();
        public RefreshTokenSettings RefreshTokenSettings { get; set; } = new();
        public ApiDocsSettings ApiDocsSettings { get; set; } = new();
    }

    public class IdentitySettings
    {
        public bool PasswordRequireDigit { get; set; }
        public int PasswordRequiredLength { get; set; } = 8;
        public bool PasswordRequireNonAlphanumeric { get; set; }
        public bool PasswordRequireUppercase { get; set; }
        public bool PasswordRequireLowercase { get; set; }
        public bool UserRequireUniqueEmail { get; set; }
    }

    public class JwtSettings
    {
        /// <summary>کلید امضای توکن؛ در Production از User-Secrets یا متغیر محیطی خوانده شود، نه appsettings.</summary>
        public string SecretKey { get; set; } = string.Empty;
        public string EncryptKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int NotBeforeMinutes { get; set; }
        /// <summary>عمر Access Token به دقیقه. توصیه: کوتاه‌مدت (۱۵ دقیقه).</summary>
        public int ExpirationMinutes { get; set; } = 15;
    }

    /// <summary>تنظیمات امنیتی کد یک‌بارمصرف پیامکی.</summary>
    public class OtpSettings
    {
        /// <summary>تعداد ارقام کد.</summary>
        public int CodeLength { get; set; } = 6;
        /// <summary>مدت اعتبار کد به ثانیه.</summary>
        public int ExpirationSeconds { get; set; } = 120;
        /// <summary>حداقل فاصله‌ی بین دو ارسال متوالی به ثانیه (ضد اسپم).</summary>
        public int ResendIntervalSeconds { get; set; } = 120;
        /// <summary>حداکثر تلاش ناموفق برای وارد کردن کد؛ پس از آن کد باطل می‌شود.</summary>
        public int MaxAttempts { get; set; } = 5;
        /// <summary>فقط برای توسعه: کد در پاسخ API هم برگردانده شود تا بدون پنل پیامک تست کنید. در Production حتماً false.</summary>
        public bool ReturnCodeInResponse { get; set; }
    }

    /// <summary>کپچا اختیاری است و با Enabled از تنظیمات روشن/خاموش می‌شود.</summary>
    public class CaptchaSettings
    {
        public bool Enabled { get; set; }
        /// <summary>نام ارائه‌دهنده؛ فعلاً Recaptcha. با تعویض پیاده‌سازی ICaptchaService قابل تغییر است.</summary>
        public string Provider { get; set; } = "Recaptcha";
        public string SecretKey { get; set; } = string.Empty;
        public string VerifyUrl { get; set; } = "https://www.google.com/recaptcha/api/siteverify";
    }

    public class SmsSettings
    {
        /// <summary>Mock برای توسعه (فقط لاگ می‌کند)؛ نام ارائه‌دهنده‌ی واقعی را بعداً اینجا بگذارید.</summary>
        public string Provider { get; set; } = "Mock";
    }

    public class RefreshTokenSettings
    {
        /// <summary>عمر Refresh Token به روز.</summary>
        public int ExpirationDays { get; set; } = 7;
    }

    /// <summary>دسترسی به مستندات Scalar؛ به‌صورت پیش‌فرض فقط در Development فعال است.</summary>
    public class ApiDocsSettings
    {
        public bool Enabled { get; set; }
        /// <summary>مسیر نمایش مستندات.</summary>
        public string Path { get; set; } = "/scalar";
    }
}