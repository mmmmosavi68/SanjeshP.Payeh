
namespace SanjeshP.Payeh.Api.DTOs.ExamCapacities;

public sealed class UniversityExamCapacityExamDto
{
    public Guid ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;

    public Guid ExamSessionId { get; set; }
    public string? ExamSessionTitle { get; set; }
    public DateTime ExamDate { get; set; }

    public int ExamModeId { get; set; }
    public string? ExamModeTitle { get; set; }

    public bool IsCapacityDefined { get; set; }
    public Guid? CapacityId { get; set; }

    public int? AllocatedCapacity { get; set; }
    public int? OwnCapacity { get; set; }

    public bool? CapacityIsActive { get; set; }

    /// <summary>تعداد کل ثبت‌نام‌های این ظرفیت</summary>
    public int RegisteredCount { get; set; }

    /// <summary>ثبت‌نام‌های داوطلبان خود دانشگاه میزبان</summary>
    public int OwnRegisteredCount { get; set; }

    /// <summary>ثبت‌نام‌های داوطلبان دانشگاه‌های مهمان</summary>
    public int GuestRegisteredCount { get; set; }

    /// <summary>مجموع ظرفیت دانشگاه‌های مهمان (فعال)</summary>
    public int TotalGuestCapacity { get; set; }
}
