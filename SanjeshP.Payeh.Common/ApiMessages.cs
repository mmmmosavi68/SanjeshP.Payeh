namespace SanjeshP.Payeh.Common;

/// <summary>مرکز پیام‌های API. هر پیام ErrorCode یکتا دارد (فرمت AREA_NNN) و بخش‌های هر حوزه در ApiMessages.Generated.cs قرار دارد.</summary>
public static partial class ApiMessages
{
    public static class Common
    {
        public static readonly ApiMessage Unauthorized = new("COMMON_401", "احراز هویت نامعتبر است. لطفاً دوباره وارد شوید.");
        public static readonly ApiMessage Forbidden = new("COMMON_403", "شما دسترسی انجام این عملیات را ندارید.");
        public static readonly ApiMessage Conflict = new("COMMON_409", "رکوردی با این مشخصات از قبل وجود دارد.");
        public static readonly ApiMessage TooManyRequests = new("COMMON_429", "تعداد درخواست‌ها بیش از حد مجاز است؛ کمی بعد دوباره تلاش کنید.");
        public static readonly ApiMessage CaptchaInvalid = new("COMMON_CAPTCHA", "کپچا نامعتبر است. لطفاً دوباره تلاش کنید.");
        public static readonly ApiMessage InvalidCredentials = new("COMMON_CREDENTIALS", "نام کاربری یا رمز عبور نادرست است.");
        public static readonly ApiMessage InvalidOtp = new("COMMON_OTP", "کد تأیید نادرست یا منقضی است.");
        public static readonly ApiMessage DataConflict = new("COMMON_DB_CONFLICT", "ذخیره‌سازی با محدودیت‌های داده‌ای در تعارض است (رکورد تکراری یا وابسته).");
        public static readonly ApiMessage BadHttpRequest = new("COMMON_BAD_REQUEST", "درخواست ارسالی معتبر نیست.");
        public static readonly ApiMessage Cancelled = new("COMMON_CANCELLED", "درخواست لغو شد.");
        public static readonly ApiMessage ServerError = new("COMMON_500", "خطای غیرمنتظره‌ای در سرور رخ داده است. لطفاً بعداً دوباره تلاش کنید.");
        public static readonly ApiMessage MappingError = new("COMMON_MAPPING", "خطا در تبدیل داده‌ها. لطفاً با پشتیبانی تماس بگیرید.");
        public static readonly ApiMessage InvalidModel = new("COMMON_INVALID_MODEL", "مقدار ارسالی معتبر نیست");
    }

    public static class Lookup
    {
        public static readonly ApiMessage CodeRequired = new("LOOKUP_001", "شناسه یا کد اصلی و کد فرعی الزامی است.");
        public static readonly ApiMessage CodeNotFound = new("LOOKUP_002", "کد موردنظر پیدا نشد.");
        public static readonly ApiMessage ExamTypeInvalid = new("LOOKUP_003", "نوع آزمون انتخاب‌شده معتبر نیست.");
        public static readonly ApiMessage ExamModeInvalid = new("LOOKUP_004", "نحوه برگزاری انتخاب‌شده معتبر نیست.");
        public static readonly ApiMessage UniversityTypeInvalid = new("LOOKUP_005", "نوع دانشگاه انتخاب‌شده معتبر نیست.");
        public static readonly ApiMessage GenderInvalid = new("LOOKUP_006", "جنسیت انتخاب‌شده معتبر نیست.");
        public static readonly ApiMessage ParentCodeInvalid = new("LOOKUP_007", "والد انتخاب‌شده معتبر نیست.");
    }

    public static class Authorization
    {
        public static readonly ApiMessage SelfAccessChange = new("AUTHZ_001", "تغییر دسترسی‌ها یا عضویت گروهی حساب کاربری خودتان مجاز نیست.");
        public static readonly ApiMessage MenuNotGrantable = new("AUTHZ_002", "فقط می‌توانید منوهایی را اعطا کنید که خودتان به آن‌ها دسترسی دارید.");
    }
}
