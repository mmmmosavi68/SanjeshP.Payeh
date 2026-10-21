using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Common
{
    /// <summary>
    /// کدهای وضعیت استاندارد پاسخ API.
    /// مقادیر قبلی (۰ تا ) بدون تغییر باقی مانده‌اند تا قرارداد فعلی نشکند؛
    /// فقط اعضای جدید برای پوشش کدهای HTTP پرکاربرد اضافه شده است.
    /// </summary>
    public enum ApiResultStatusCode
    {
        /// <summary>عملیات موفق (HTTP 200 / 201)</summary>
        [Display(Name = "عملیات با موفقیت انجام شد")]
        Success = 0,

        /// <summary>خطای داخلی سرور (HTTP 500)</summary>
        [Display(Name = "خطایی در سرور رخ داده است")]
        ServerError = 1,

        /// <summary>درخواست نامعتبر (HTTP 400)</summary>
        [Display(Name = "پارامتر های ارسالی معتبر نیستند")]
        BadRequest = 2,

        /// <summary>منبع مورد نظر یافت نشد (HTTP 404)</summary>
        [Display(Name = "یافت نشد")]
        NotFound = 3,

        /// <summary>لیست خالی است (بدون خطا)</summary>
        [Display(Name = "لیست خالی است")]
        ListEmpty = 4,

        /// <summary>خطای منطق کسب‌وکار (HTTP 400)</summary>
        [Display(Name = "خطایی در پردازش رخ داد")]
        LogicError = 5,

        /// <summary>احراز هویت نشده (HTTP 401)</summary>
        [Display(Name = "خطای احراز هویت")]
        UnAuthorized = 6,

        // ===== اعضای جدید (افزودنی — Breaking نیست) =====

        /// <summary>موفق بدون محتوا (HTTP 204)</summary>
        [Display(Name = "عملیات با موفقیت انجام شد")]
        NoContent = 7,

        /// <summary>دسترسی غیرمجاز (HTTP 403)</summary>
        [Display(Name = "شما دسترسی انجام این عملیات را ندارید")]
        Forbidden = 8,

        /// <summary>تعارض با وضعیت فعلی منبع (HTTP 409)</summary>
        [Display(Name = "درخواست با وضعیت فعلی سیستم در تعارض است")]
        Conflict = 9,

        /// <summary>موجودیت غیرقابل پردازش (HTTP 422)</summary>
        [Display(Name = "داده‌های ارسالی قابل پردازش نیستند")]
        UnprocessableEntity = 10,

        /// <summary>درخواست‌های بیش از حد مجاز (HTTP 429)</summary>
        [Display(Name = "تعداد درخواست‌ها بیش از حد مجاز است؛ کمی بعد تلاش کنید")]
        TooManyRequests = 11
    }
}