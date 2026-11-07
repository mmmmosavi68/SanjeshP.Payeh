using SanjeshP.Payeh.Entities.Audit;

namespace SanjeshP.Payeh.Data.Contracts;

/// <summary>پارامترهای جستجوی تاریخچه ممیزی؛ تاریخ‌ها باید UTC باشند.</summary>
public sealed record AuditLogQuery(
    int PageNumber = 1,
    int PageSize = 20,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null,
    Guid? UserId = null,
    string? UserName = null,
    string? IpAddress = null,
    string? EventType = null,
    string? Action = null,
    string? EntityName = null,
    string? EntityId = null,
    int? StatusCode = null,
    string? Search = null);

/// <summary>نتیجه صفحه‌بندی‌شده جستجوی ممیزی.</summary>
public sealed record PagedAuditLogs(
    IReadOnlyList<AuditLog> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);

/// <summary>جستجوی فقط‌خواندنی سوابق ممیزی.</summary>
public interface IAuditLogQueryService
{
    Task<PagedAuditLogs> SearchAsync(AuditLogQuery query, CancellationToken cancellationToken);
}
