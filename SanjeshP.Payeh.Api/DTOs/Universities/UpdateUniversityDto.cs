
using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Api.DTOs.Universities;

public sealed class UpdateUniversityDto
{
    [Required(ErrorMessage = "نام دانشگاه الزامی است")]
    [MaxLength(200, ErrorMessage = "نام دانشگاه نباید بیش از ۲۰۰ کاراکتر باشد")]
    public string Title { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "نوع دانشگاه معتبر نیست")]
    public int UniversityTypeId { get; set; }
    public int? UniversityTypePrimaryCode { get; set; }
    public int? UniversityTypeSubCode { get; set; }

    [Range(1, 11, ErrorMessage = "منطقه دانشگاه باید بین ۱ تا ۱۱ باشد")]
    public int Region { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "ظرفیت مصوب نمی‌تواند منفی باشد")]
    public int CapacityApproved { get; set; }

    public int? ParentUniversityId { get; set; }

    public bool IsActive { get; set; }
}
