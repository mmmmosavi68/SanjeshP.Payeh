using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SanjeshP.Payeh.Data.Contracts;

namespace SanjeshP.Payeh.WebFramework.Middlewares;

/// <summary>
/// ثبت خودکار درخواست‌های API در جدول AuditLogs.
/// بدنه‌ی درخواست/پاسخ ثبت نمی‌شود تا رمز، OTP، توکن و داده‌های حساس افشا نشوند.
/// </summary>
public sealed class AuditRequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditRequestLoggingMiddleware> _logger;

    public AuditRequestLoggingMiddleware(RequestDelegate next, ILogger<AuditRequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogService)
    {
        // فقط مسیرهای API ثبت شوند؛ مستندات و فایل‌های استاتیک وارد Audit نشوند.
        if (!context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        context.Response.OnCompleted(async () =>
        {
            stopwatch.Stop();
            try
            {
                await auditLogService.LogRequestAsync(
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    CancellationToken.None);
            }
            catch (Exception exception)
            {
                // خطای Audit نباید پاسخ تکمیل‌شده‌ی API را خراب کند.
                _logger.LogError(exception, "ثبت خودکار درخواست در Audit ناموفق بود. TraceId={TraceId}", context.TraceIdentifier);
            }
        });

        await _next(context);
    }
}
