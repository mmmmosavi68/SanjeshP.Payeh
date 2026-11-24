namespace SanjeshP.Payeh.Services.Contracts;

/// <summary>ثبت رویدادهای فعالیت کاربران و رویدادهای ممیزی.</summary>
public interface IAuditLogService
{
    Task LogActivityAsync(
        string action,
        string? description = null,
        string? entityName = null,
        string? entityId = null,
        string? oldValuesJson = null,
        string? newValuesJson = null,
        string eventType = "Activity",
        CancellationToken cancellationToken = default,
        Guid? actorUserId = null,
        string? actorUserName = null);

    Task LogRequestAsync(int statusCode, long durationMs, CancellationToken cancellationToken = default);
}
