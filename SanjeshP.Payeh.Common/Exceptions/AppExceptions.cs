using System.Net;

namespace SanjeshP.Payeh.Common.Exceptions
{
    /// <summary>
    /// استثناهای دامنه‌ای پرکاربرد. همگی از <see cref="AppException"/> ارث می‌برند
    /// تا مدیریت خطای مرکزی (CustomExceptionHandler) بتواند کد HTTP و کد ApiResult
    /// درست را بدون try/catch پراکنده در کنترلرها تولید کند.
    /// نکته: این کلاس‌ها در یک فایل گروه شده‌اند چون هرکدام فقط یک سازنده‌ی ساده دارند.
    /// </summary>
    public class UnauthorizedException : AppException
    {
        /// <summary>احراز هویت نشده یا توکن نامعتبر است (HTTP 401).</summary>
        public UnauthorizedException(string message = "احراز هویت نامعتبر است. لطفاً دوباره وارد شوید.")
            : base(ApiResultStatusCode.UnAuthorized, message, HttpStatusCode.Unauthorized)
        {
        }
    }

    /// <summary>دسترسی به این عملیات برای کاربر فعلی مجاز نیست (HTTP 403).</summary>
    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message = "شما دسترسی انجام این عملیات را ندارید.")
            : base(ApiResultStatusCode.Forbidden, message, HttpStatusCode.Forbidden)
        {
        }
    }

    /// <summary>درخواست با وضعیت فعلی منبع در تعارض است؛ مثل نام کاربری تکراری (HTTP 409).</summary>
    public class ConflictException : AppException
    {
        public ConflictException(string message = "رکوردی با این مشخصات از قبل وجود دارد.")
            : base(ApiResultStatusCode.Conflict, message, HttpStatusCode.Conflict)
        {
        }
    }

    /// <summary>داده از نظر ساختاری درست است ولی از نظر قواعد کسب‌وکار قابل پردازش نیست (HTTP 422).</summary>
    public class UnprocessableEntityException : AppException
    {
        public UnprocessableEntityException(string message)
            : base(ApiResultStatusCode.UnprocessableEntity, message, HttpStatusCode.UnprocessableEntity)
        {
        }
    }

    /// <summary>محدودیت نرخ درخواست نقض شده است؛ مثل ارسال مجدد زودهنگام کد پیامکی (HTTP 429).</summary>
    public class TooManyRequestsException : AppException
    {
        public TooManyRequestsException(string message = "تعداد درخواست‌ها بیش از حد مجاز است؛ کمی بعد دوباره تلاش کنید.")
            : base(ApiResultStatusCode.TooManyRequests, message, HttpStatusCode.TooManyRequests)
        {
        }
    }
}