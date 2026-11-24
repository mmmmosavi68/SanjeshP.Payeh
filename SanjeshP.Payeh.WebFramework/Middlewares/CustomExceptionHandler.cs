using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.Common.Utilities;
using SanjeshP.Payeh.WebFramework.Api;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SanjeshP.Payeh.WebFramework.Middlewares
{
    /// <summary>
    /// مدیریت مرکزی خطا با IExceptionHandler (الگوی بومی ASP.NET Core).
    /// اصلاحات نسبت به نسخه‌ی قبل:
    /// - کد ApiResult از خود AppException خوانده می‌شود (قبلاً همیشه ServerError بود).
    /// - استثناهای شناخته‌شده (DbUpdate، UnauthorizedAccess و ...) نگاشت درست دارند.
    /// - جزئیات فنی (StackTrace / پیام دیتابیس) هرگز به کلاینت نمی‌رسد؛ فقط در لاگ.
    /// - خروجی camelCase و بدون null، هم‌راستا با خروجی MVC؛ شناسه‌ی رهگیری در هدر X-Trace-Id.
    /// </summary>
    public class CustomExceptionHandler : IExceptionHandler
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            // برای خوانا ماندن پیام فارسی در JSON
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private readonly ILogger<CustomExceptionHandler> _logger;

        public CustomExceptionHandler(ILogger<CustomExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (httpStatusCode, apiStatusCode, message, errorCode) = MapException(exception);

            // خطاهای مورد انتظار (۴xx) با سطح Warning و بقیه با Error لاگ می‌شوند.
            if ((int)httpStatusCode >= 500)
                _logger.LogError(exception, "خطای پردازش نشده در {Path}", httpContext.Request.Path);
            else
                _logger.LogWarning("خطای دامنه‌ای در {Path}: {Message}", httpContext.Request.Path, exception.Message);

            httpContext.Response.StatusCode = (int)httpStatusCode;
            httpContext.Response.ContentType = "application/json; charset=utf-8";
            httpContext.Response.Headers["X-Trace-Id"] = httpContext.TraceIdentifier;

            var result = new ApiResult(false, apiStatusCode, message) { ErrorCode = errorCode };
            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(result, SerializerOptions), cancellationToken);
            return true;
        }

        /// <summary>نگاشت استثنا به کد HTTP، کد ApiResult و پیام امن فارسی.</summary>
        private static (HttpStatusCode Http, ApiResultStatusCode Api, string Message, string? ErrorCode) MapException(Exception exception) => exception switch
        {
            AppException appException => (appException.HttpStatusCode, appException.ApiStatusCode, appException.Message, appException.ErrorCode),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ApiResultStatusCode.UnAuthorized, ApiResultStatusCode.UnAuthorized.ToDisplay(), ApiMessages.Common.Unauthorized.Code),
            DbUpdateException => (HttpStatusCode.Conflict, ApiResultStatusCode.Conflict, ApiMessages.Common.DataConflict.Text, ApiMessages.Common.DataConflict.Code),
            BadHttpRequestException => (HttpStatusCode.BadRequest, ApiResultStatusCode.BadRequest, ApiMessages.Common.BadHttpRequest.Text, ApiMessages.Common.BadHttpRequest.Code),
            OperationCanceledException => (HttpStatusCode.BadRequest, ApiResultStatusCode.BadRequest, ApiMessages.Common.Cancelled.Text, ApiMessages.Common.Cancelled.Code),
            ArgumentNullException { StackTrace: not null } nullEx when nullEx.StackTrace.Contains("Mapping.ApiMapper") => (HttpStatusCode.InternalServerError, ApiResultStatusCode.ServerError, ApiMessages.Common.MappingError.Text, ApiMessages.Common.MappingError.Code),
            _ => (HttpStatusCode.InternalServerError, ApiResultStatusCode.ServerError, ApiMessages.Common.ServerError.Text, ApiMessages.Common.ServerError.Code)
        };
    }
}
