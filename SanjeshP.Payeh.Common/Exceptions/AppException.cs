using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SanjeshP.Payeh.Common.Exceptions
{
    public class AppException : Exception
    {
        public HttpStatusCode HttpStatusCode { get; set; }
        public ApiResultStatusCode ApiStatusCode { get; set; }
        public object AdditionalData { get; set; }

        public AppException()
            : this(ApiResultStatusCode.ServerError)
        {
        }

        public AppException(ApiResultStatusCode statusCode)
            : this(statusCode, null)
        {
        }

        public AppException(string message)
            : this(ApiResultStatusCode.ServerError, message)
        {
        }

        public AppException(ApiResultStatusCode statusCode, string message)
            : this(statusCode, message, DefaultHttpStatus(statusCode))
        {
        }

        public AppException(string message, object additionalData)
            : this(ApiResultStatusCode.ServerError, message, additionalData)
        {
        }

        public AppException(ApiResultStatusCode statusCode, object additionalData)
            : this(statusCode, null, additionalData)
        {
        }

        public AppException(ApiResultStatusCode statusCode, string message, object additionalData)
            : this(statusCode, message, DefaultHttpStatus(statusCode), additionalData)
        {
        }

        public AppException(ApiResultStatusCode statusCode, string message, HttpStatusCode httpStatusCode)
            : this(statusCode, message, httpStatusCode, null)
        {
        }

        public AppException(ApiResultStatusCode statusCode, string message, HttpStatusCode httpStatusCode, object additionalData)
            : this(statusCode, message, httpStatusCode, null, additionalData)
        {
        }

        public AppException(string message, Exception exception)
            : this(ApiResultStatusCode.ServerError, message, exception)
        {
        }

        public AppException(string message, Exception exception, object additionalData)
            : this(ApiResultStatusCode.ServerError, message, exception, additionalData)
        {
        }

        public AppException(ApiResultStatusCode statusCode, string message, Exception exception)
            : this(statusCode, message, DefaultHttpStatus(statusCode), exception)
        {
        }

        public AppException(ApiResultStatusCode statusCode, string message, Exception exception, object additionalData)
            : this(statusCode, message, DefaultHttpStatus(statusCode), exception, additionalData)
        {
        }

        public AppException(ApiResultStatusCode statusCode, string message, HttpStatusCode httpStatusCode, Exception exception)
            : this(statusCode, message, httpStatusCode, exception, null)
        {
        }

        public AppException(ApiResultStatusCode statusCode, string message, HttpStatusCode httpStatusCode, Exception exception, object additionalData)
            : base(message, exception)
        {
            ApiStatusCode = statusCode;
            HttpStatusCode = httpStatusCode;
            AdditionalData = additionalData;
        }

        /// <summary>
        /// کد HTTP پیش‌فرض بر اساس ApiResultStatusCode؛ قبلاً NotFound و BadRequest و ... بدون کد صریح
        /// با HTTP 500 برمی‌گشتند.
        /// </summary>
        private static HttpStatusCode DefaultHttpStatus(ApiResultStatusCode statusCode) => statusCode switch
        {
            ApiResultStatusCode.BadRequest => HttpStatusCode.BadRequest,
            ApiResultStatusCode.LogicError => HttpStatusCode.BadRequest,
            ApiResultStatusCode.CaptchaInvalid => HttpStatusCode.BadRequest,
            ApiResultStatusCode.InvalidCredentials => HttpStatusCode.BadRequest,
            ApiResultStatusCode.InvalidOtp => HttpStatusCode.BadRequest,
            ApiResultStatusCode.NotFound => HttpStatusCode.NotFound,
            ApiResultStatusCode.UnAuthorized => HttpStatusCode.Unauthorized,
            ApiResultStatusCode.Forbidden => HttpStatusCode.Forbidden,
            ApiResultStatusCode.Conflict => HttpStatusCode.Conflict,
            ApiResultStatusCode.UnprocessableEntity => HttpStatusCode.UnprocessableEntity,
            ApiResultStatusCode.TooManyRequests => HttpStatusCode.TooManyRequests,
            _ => HttpStatusCode.InternalServerError
        };
    }
}
