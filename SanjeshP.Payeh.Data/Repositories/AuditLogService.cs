using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Entities.Audit;

namespace SanjeshP.Payeh.Data.Repositories;

/// <summary>
/// سرویس ثبت ممیزی. خطای ثبت Audit نباید باعث شکست عملیات اصلی کاربر شود؛
/// خطای ثبت در لاگ فنی ثبت می‌شود تا قابل پیگیری باشد.
/// </summary>
public sealed class AuditLogService : IAuditLogService, IScopedDependency
{
    private readonly IEFRepository<AuditLog> _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        IEFRepository<AuditLog> repository,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditLogService> logger)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public Task LogActivityAsync(
        string action,
        string? description = null,
        string? entityName = null,
        string? entityId = null,
        string? oldValuesJson = null,
        string? newValuesJson = null,
        string eventType = "Activity",
        CancellationToken cancellationToken = default,
        Guid? actorUserId = null,
        string? actorUserName = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var entry = CreateBaseEntry(httpContext);
        // در ورود، کاربر هنوز احراز هویت نشده است؛ شناسه‌ی کاربرِ احرازشده توسط منطق ورود تزریق می‌شود.
        if (actorUserId.HasValue)
            entry.UserId = actorUserId.Value;
        if (!string.IsNullOrWhiteSpace(actorUserName))
            entry.UserName = Limit(actorUserName, 100);
        entry.EventType = Normalize(eventType, 20, "Activity");
        entry.Action = Normalize(action, 150, "Unknown");
        entry.Description = Limit(description, 1000);
        entry.EntityName = Limit(entityName, 150);
        entry.EntityId = Limit(entityId, 100);
        // از ثبت حجم غیرعادی داده در جدول ممیزی جلوگیری می‌شود.
        entry.OldValuesJson = Limit(oldValuesJson, 16000);
        entry.NewValuesJson = Limit(newValuesJson, 16000);

        return SaveSafelyAsync(entry, cancellationToken);
    }

    public Task LogRequestAsync(int statusCode, long durationMs, CancellationToken cancellationToken = default)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return Task.CompletedTask;

        var entry = CreateBaseEntry(httpContext);
        entry.EventType = "Request";
        entry.Action = "HttpRequest";
        entry.Description = "ثبت خودکار درخواست HTTP";
        entry.HttpMethod = Limit(httpContext.Request.Method, 10);
        entry.Path = Limit(httpContext.Request.Path.Value, 500);
        entry.StatusCode = statusCode;
        entry.DurationMs = Math.Max(0, durationMs);

        return SaveSafelyAsync(entry, cancellationToken);
    }

    private static AuditLog CreateBaseEntry(HttpContext? context)
    {
        var principal = context?.User;
        var userIdValue = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? principal?.FindFirst("sub")?.Value;

        Guid? userId = Guid.TryParse(userIdValue, out var parsedUserId) ? parsedUserId : null;
        var userName = principal?.Identity?.IsAuthenticated == true
            ? principal.Identity.Name ?? principal.FindFirst(ClaimTypes.Name)?.Value
            : null;

        return new AuditLog
        {
            UserId = userId,
            UserName = Limit(userName, 100),
            IpAddress = Limit(context?.Connection.RemoteIpAddress?.ToString(), 45),
            UserAgent = Limit(context?.Request.Headers.UserAgent.ToString(), 500),
            CorrelationId = Limit(context?.TraceIdentifier, 100),
            OccurredAtUtc = DateTime.UtcNow
        };
    }

    private async Task SaveSafelyAsync(AuditLog entry, CancellationToken cancellationToken)
    {
        try
        {
            await _repository.AddAsync(entry, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception,
                "ثبت Audit ناموفق بود. Action={AuditAction}, EventType={AuditEventType}, TraceId={TraceId}",
                entry.Action, entry.EventType, entry.CorrelationId);
        }
    }

    private static string Normalize(string? value, int maxLength, string fallback)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? fallback : Limit(normalized, maxLength)!;
    }

    private static string? Limit(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
