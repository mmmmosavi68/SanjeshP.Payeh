using SanjeshP.Payeh.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Entities.Audit;

/// <summary>
/// رویدادهای ممیزی سامانه؛ مستقل از BaseEntity است تا رویدادهای کاربران ناشناس نیز ثبت شوند.
/// هیچ رابطه‌ی اجباری با User ندارد تا حذف کاربر باعث حذف یا اختلال در تاریخچه نشود.
/// </summary>
public sealed class AuditLog : IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? UserId { get; set; }

    [MaxLength(100)]
    public string? UserName { get; set; }

    /// <summary>Request, Activity, Security</summary>
    [Required, MaxLength(20)]
    public string EventType { get; set; } = "Activity";

    [Required, MaxLength(150)]
    public string Action { get; set; } = null!;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(150)]
    public string? EntityName { get; set; }

    [MaxLength(100)]
    public string? EntityId { get; set; }

    [MaxLength(10)]
    public string? HttpMethod { get; set; }

    [MaxLength(500)]
    public string? Path { get; set; }

    public int? StatusCode { get; set; }

    public long? DurationMs { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    [MaxLength(100)]
    public string? CorrelationId { get; set; }

    /// <summary>JSON اختیاری؛ اطلاعات حساس مانند رمز و توکن نباید در آن قرار گیرد.</summary>
    public string? OldValuesJson { get; set; }

    /// <summary>JSON اختیاری؛ اطلاعات حساس مانند رمز و توکن نباید در آن قرار گیرد.</summary>
    public string? NewValuesJson { get; set; }

    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}
