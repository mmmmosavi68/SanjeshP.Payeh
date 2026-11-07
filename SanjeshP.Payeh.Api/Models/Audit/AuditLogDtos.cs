using System.ComponentModel.DataAnnotations;
using SanjeshP.Payeh.Entities.Audit;

namespace SanjeshP.Payeh.Api.Models.Audit;

/// <summary>پارامترهای فیلتر گزارش ممیزی.</summary>
public sealed class AuditLogQueryParameters
{
    [Range(1, int.MaxValue, ErrorMessage = "شماره صفحه باید بزرگ‌تر از صفر باشد")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "تعداد در هر صفحه باید بین 1 تا 100 باشد")]
    public int PageSize { get; set; } = 20;

    /// <summary>ابتدای بازه به وقت UTC.</summary>
    public DateTime? FromUtc { get; set; }

    /// <summary>انتهای بازه به وقت UTC.</summary>
    public DateTime? ToUtc { get; set; }

    public Guid? UserId { get; set; }
    [MaxLength(100)] public string? UserName { get; set; }
    [MaxLength(45)] public string? IpAddress { get; set; }
    [MaxLength(20)] public string? EventType { get; set; }
    [MaxLength(150)] public string? Action { get; set; }
    [MaxLength(150)] public string? EntityName { get; set; }
    [MaxLength(100)] public string? EntityId { get; set; }
    public int? StatusCode { get; set; }
    [MaxLength(200)] public string? Search { get; set; }
}

/// <summary>اطلاعات یک رویداد ممیزی برای نمایش در پنل مدیریتی.</summary>
public sealed class AuditLogDto
{
    public Guid Id { get; init; }
    public Guid? UserId { get; init; }
    public string? UserName { get; init; }
    public string EventType { get; init; } = null!;
    public string Action { get; init; } = null!;
    public string? Description { get; init; }
    public string? EntityName { get; init; }
    public string? EntityId { get; init; }
    public string? HttpMethod { get; init; }
    public string? Path { get; init; }
    public int? StatusCode { get; init; }
    public long? DurationMs { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? CorrelationId { get; init; }
    public string? OldValuesJson { get; init; }
    public string? NewValuesJson { get; init; }
    public DateTime OccurredAtUtc { get; init; }

    public static AuditLogDto FromEntity(AuditLog entity) => new()
    {
        Id = entity.Id,
        UserId = entity.UserId,
        UserName = entity.UserName,
        EventType = entity.EventType,
        Action = entity.Action,
        Description = entity.Description,
        EntityName = entity.EntityName,
        EntityId = entity.EntityId,
        HttpMethod = entity.HttpMethod,
        Path = entity.Path,
        StatusCode = entity.StatusCode,
        DurationMs = entity.DurationMs,
        IpAddress = entity.IpAddress,
        UserAgent = entity.UserAgent,
        CorrelationId = entity.CorrelationId,
        OldValuesJson = entity.OldValuesJson,
        NewValuesJson = entity.NewValuesJson,
        OccurredAtUtc = entity.OccurredAtUtc
    };
}

/// <summary>خروجی صفحه‌بندی‌شده گزارش ممیزی.</summary>
public sealed record AuditLogPagedDto(
    IReadOnlyList<AuditLogDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);
