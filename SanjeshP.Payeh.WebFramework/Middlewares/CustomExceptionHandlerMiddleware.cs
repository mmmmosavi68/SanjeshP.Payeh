using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.WebFramework.Api;
using System.Net;
using System.Text.Json;

namespace SanjeshP.Payeh.WebFramework.Middlewares
{
    public class CustomExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<CustomExceptionHandler> _logger;

        public CustomExceptionHandler(ILogger<CustomExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, exception.Message);

            var statusCode = HttpStatusCode.InternalServerError;
            var message = "خطای سرور رخ داده است.";

            if (exception is AppException appException)
            {
                // استفاده از HttpStatusCode یا StatusCode بسته به تعریف کلاس AppException شما
                statusCode = appException.HttpStatusCode;
                message = appException.Message;
            }

            httpContext.Response.StatusCode = (int)statusCode;
            httpContext.Response.ContentType = "application/json";

            var result = new ApiResult(false, ApiResultStatusCode.ServerError, message);
            var json = JsonSerializer.Serialize(result);

            await httpContext.Response.WriteAsync(json, cancellationToken);
            return true;
        }
    }
}