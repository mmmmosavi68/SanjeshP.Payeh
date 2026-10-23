using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Services.Permissions;
using SanjeshP.Payeh.WebFramework.Api;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SanjeshP.Payeh.WebFramework.Middlewares
{
    /// <summary>
    /// اکشن/کنترلرِ علامت‌خورده فقط «ورود» می‌خواهد، نه دسترسی منو
    /// (مثل logout، sessions، پروفایل خود کاربر).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class SkipMenuPermissionAttribute : Attribute { }

    /// <summary>
    /// کنترل دسترسی بر پایه‌ی منو (Area/Controller/Action) برای کاربرِ احرازشده با JWT.
    /// جای UseAuthorization قرار نمی‌گیرد؛ باید بعد از آن بیاید.
    /// اعتبار نشست (session_id) را خود JwtBearer در OnTokenValidated چک کرده است.
    /// </summary>
    public class MenuAuthorizationMiddleware
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private readonly RequestDelegate _next;

        public MenuAuthorizationMiddleware(RequestDelegate next) => _next = next;

        // سرویس Scoped است ⇒ باید از طریق پارامتر InvokeAsync تزریق شود، نه سازنده.
        public async Task InvokeAsync(HttpContext context, IPermissionService permissionService)
        {
            var endpoint = context.GetEndpoint();

            // فقط اکشن‌های کنترلر؛ اندپوینت‌های دیگر (مثل مستندات Scalar/OpenAPI) دست‌نخورده می‌مانند.
            var descriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (endpoint is null || descriptor is null)
            {
                await _next(context);
                return;
            }

            if (endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            {
                await _next(context);
                return;
            }

            // ورود لازم است (JwtBearer قبلاً توکن و نشست را اعتبارسنجی کرده).
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (context.User.Identity?.IsAuthenticated != true || !Guid.TryParse(userIdClaim, out var userId))
            {
                await WriteAsync(context, StatusCodes.Status401Unauthorized, ApiResultStatusCode.UnAuthorized);
                return;
            }

            // اکشن‌هایی که فقط «ورود» می‌خواهند.
            if (endpoint.Metadata.GetMetadata<SkipMenuPermissionAttribute>() is not null)
            {
                await _next(context);
                return;
            }

            descriptor.RouteValues.TryGetValue("area", out var area);

            var hasAccess = await permissionService.HasAccessAsync(
                userId, area, descriptor.ControllerName, descriptor.ActionName, context.RequestAborted);

            if (!hasAccess)
            {
                await WriteAsync(context, StatusCodes.Status403Forbidden, ApiResultStatusCode.Forbidden,
                    "شما مجوز دسترسی به این بخش را ندارید.");
                return;
            }

            await _next(context);
        }

        private static async Task WriteAsync(HttpContext context, int httpStatus, ApiResultStatusCode apiStatus, string? message = null)
        {
            context.Response.Clear();
            context.Response.StatusCode = httpStatus;
            await context.Response.WriteAsJsonAsync(new ApiResult(false, apiStatus, message), JsonOptions);
        }
    }

    public static class MenuAuthorizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseMenuAuthorization(this IApplicationBuilder app)
            => app.UseMiddleware<MenuAuthorizationMiddleware>();
    }
}