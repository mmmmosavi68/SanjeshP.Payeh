using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Utilities;

namespace SanjeshP.Payeh.WebFramework.Api
{
    /// <summary>
    /// پوشش استاندارد همه‌ی پاسخ‌های API.
    /// قرارداد خروجی بدون تغییر است: isSuccess / statusCode / message / data.
    /// تولید آن معمولاً خودکار انجام می‌شود:
    /// - اکشن‌ها Ok/NotFound/... برمی‌گردانند و ApiResultFilter آن را می‌پوشاند.
    /// - استثناها توسط CustomExceptionHandler به همین قالب تبدیل می‌شوند.
    /// </summary>
    public class ApiResult
    {
        /// <summary>آیا عملیات موفق بوده است؟ (کدهای 2xx)</summary>
        public bool IsSuccess { get; set; }

        /// <summary>کد وضعیت معناییِ پاسخ (نه کد HTTP).</summary>
        public ApiResultStatusCode StatusCode { get; set; }

        /// <summary>پیام فارسی قابل نمایش به کاربر.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? Message { get; set; }

        public ApiResult(bool isSuccess, ApiResultStatusCode statusCode, string? message = null)
        {
            IsSuccess = isSuccess;
            StatusCode = statusCode;
            Message = message ?? statusCode.ToDisplay();
        }

        #region Implicit Operators
        public static implicit operator ApiResult(OkResult result)
            => new(true, ApiResultStatusCode.Success);

        public static implicit operator ApiResult(BadRequestResult result)
            => new(false, ApiResultStatusCode.BadRequest);

        public static implicit operator ApiResult(BadRequestObjectResult result)
        {
            var message = result.Value?.ToString();
            if (result.Value is SerializableError errors)
            {
                var errorMessages = errors.SelectMany(p => (string[])p.Value!).Distinct();
                message = string.Join(" | ", errorMessages);
            }
            return new ApiResult(false, ApiResultStatusCode.BadRequest, message);
        }

        public static implicit operator ApiResult(ContentResult result)
            => new(true, ApiResultStatusCode.Success, result.Content);

        public static implicit operator ApiResult(NotFoundResult result)
            => new(false, ApiResultStatusCode.NotFound);
        #endregion
    }

    /// <summary>نسخه‌ی داده‌دار پوشش استاندارد پاسخ.</summary>
    /// <typeparam name="TData">نوع داده‌ی خروجی (DTO).</typeparam>
    public class ApiResult<TData> : ApiResult
        where TData : class
    {
        /// <summary>داده‌ی پاسخ؛ در خطاها معمولاً null است.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TData? Data { get; set; }

        public ApiResult(bool isSuccess, ApiResultStatusCode statusCode, TData? data, string? message = null)
            : base(isSuccess, statusCode, message)
        {
            Data = data;
        }

        #region Implicit Operators
        public static implicit operator ApiResult<TData>(TData data)
            => new(true, ApiResultStatusCode.Success, data);

        public static implicit operator ApiResult<TData>(OkResult result)
            => new(true, ApiResultStatusCode.Success, null);

        public static implicit operator ApiResult<TData>(OkObjectResult result)
            => new(true, ApiResultStatusCode.Success, result.Value as TData);

        public static implicit operator ApiResult<TData>(BadRequestResult result)
            => new(false, ApiResultStatusCode.BadRequest, null);

        public static implicit operator ApiResult<TData>(BadRequestObjectResult result)
        {
            var message = result.Value?.ToString();
            if (result.Value is SerializableError errors)
            {
                var errorMessages = errors.SelectMany(p => (string[])p.Value!).Distinct();
                message = string.Join(" | ", errorMessages);
            }
            return new ApiResult<TData>(false, ApiResultStatusCode.BadRequest, null, message);
        }

        public static implicit operator ApiResult<TData>(ContentResult result)
            => new(true, ApiResultStatusCode.Success, null, result.Content);

        public static implicit operator ApiResult<TData>(NotFoundResult result)
            => new(false, ApiResultStatusCode.NotFound, null);

        public static implicit operator ApiResult<TData>(NotFoundObjectResult result)
            => new(false, ApiResultStatusCode.NotFound, result.Value as TData);
        #endregion
    }
}