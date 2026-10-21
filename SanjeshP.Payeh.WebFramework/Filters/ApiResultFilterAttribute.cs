using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.WebFramework.Api;

// اصلاح Namespace: WebFramework.Filters → SanjeshP.Payeh.WebFramework.Filters
namespace SanjeshP.Payeh.WebFramework.Filters
{
    /// <summary>
    /// همه‌ی خروجی اکشن‌ها را به قالب ApiResult می‌پوشاند.
    /// بازنویسی کامل نسبت به نسخه‌ی قبل:
    /// - همه‌ی کدهای وضعیت (201/204/401/403/409/422/429/...) پوشش داده می‌شوند، نه فقط 200/400/404.
    /// - اگر خروجی از قبل ApiResult باشد، دوباره پوشانده نمی‌شود (رفع Double-Wrap).
    /// - خطاهای اعتبارسنجی ([ApiController] → ValidationProblemDetails) به پیام فارسی تجمیع می‌شوند.
    /// - پاسخ‌های فایلی/جریانی دست‌نخورده باقی می‌مانند.
    /// </summary>
    public class ApiResultFilterAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            // پاسخ‌های فایلی و جریانی نباید JSON شوند.
            if (context.Result is FileResult)
            {
                base.OnResultExecuting(context);
                return;
            }

            switch (context.Result)
            {
                case ObjectResult objectResult:
                    if (objectResult.Value is ApiResult)
                        break;

                    var statusCode = objectResult.StatusCode
                        ?? (objectResult.Value is not null
                            ? StatusCodes.Status200OK
                            : StatusCodes.Status204NoContent);

                    var message = ExtractErrorMessage(
                        objectResult.Value, statusCode);

                    var data = statusCode is >= 200 and < 300
                        ? objectResult.Value
                        : null;

                    context.Result = Wrap(statusCode, data, message);
                    break;

                case EmptyResult:
                    context.Result = Wrap(
                        StatusCodes.Status204NoContent, null, null);
                    break;

                case UnauthorizedResult:
                    context.Result = Wrap(
                        StatusCodes.Status401Unauthorized, null, null);
                    break;

                case ForbidResult:
                    context.Result = Wrap(
                        StatusCodes.Status403Forbidden, null, null);
                    break;

                case StatusCodeResult statusCodeResult:
                    context.Result = Wrap(
                        statusCodeResult.StatusCode, null, null);
                    break;

                case ContentResult contentResult:
                    context.Result = Wrap(
                        contentResult.StatusCode ?? StatusCodes.Status200OK,
                        null,
                        contentResult.Content);
                    break;
            }

            base.OnResultExecuting(context);
        }

        /// <summary>ساخت JsonResult با قالب ApiResult و کد HTTP اصلی.</summary>
        private static JsonResult Wrap(int httpStatusCode, object? data, string? message)
        {
            var apiStatusCode = MapStatusCode(httpStatusCode);
            var isSuccess = httpStatusCode is >= 200 and < 300;

            ApiResult apiResult = data is null
                ? new ApiResult(isSuccess, apiStatusCode, message)
                : new ApiResult<object>(isSuccess, apiStatusCode, data, message);

            return new JsonResult(apiResult) { StatusCode = httpStatusCode };
        }

        /// <summary>نگاشت کد HTTP به کد معنایی ApiResult.</summary>
        private static ApiResultStatusCode MapStatusCode(int httpStatusCode) => httpStatusCode switch
        {
            StatusCodes.Status204NoContent => ApiResultStatusCode.NoContent,
            >= 200 and < 300 => ApiResultStatusCode.Success,
            StatusCodes.Status400BadRequest => ApiResultStatusCode.BadRequest,
            StatusCodes.Status401Unauthorized => ApiResultStatusCode.UnAuthorized,
            StatusCodes.Status403Forbidden => ApiResultStatusCode.Forbidden,
            StatusCodes.Status404NotFound => ApiResultStatusCode.NotFound,
            StatusCodes.Status409Conflict => ApiResultStatusCode.Conflict,
            StatusCodes.Status422UnprocessableEntity => ApiResultStatusCode.UnprocessableEntity,
            StatusCodes.Status429TooManyRequests => ApiResultStatusCode.TooManyRequests,
            _ => ApiResultStatusCode.ServerError
        };

        /// <summary>استخراج پیام خطای اعتبارسنجی از ValidationProblemDetails / SerializableError.</summary>
        private static string? ExtractErrorMessage(object? value, int statusCode)
        {
            if (statusCode is >= 200 and < 300 || value is null or ProblemDetails and not ValidationProblemDetails)
                return null;

            return value switch
            {
                ValidationProblemDetails validationProblemDetails =>
                    string.Join(" | ", validationProblemDetails.Errors.SelectMany(p => p.Value).Distinct()),
                SerializableError errors =>
                    string.Join(" | ", errors.SelectMany(p => (string[])p.Value!).Distinct()),
                ProblemDetails problemDetails => problemDetails.Title,
                string s => s,
                _ => null
            };
        }
    }
}