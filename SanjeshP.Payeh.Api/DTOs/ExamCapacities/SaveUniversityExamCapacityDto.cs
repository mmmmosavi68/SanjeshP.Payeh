
using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Api.DTOs.ExamCapacities;

public sealed class SaveUniversityExamCapacityDto
{
    [Range(1, int.MaxValue,
        ErrorMessage = "ظرفیت تخصیص‌یافته باید حداقل ۱ باشد")]
    public int AllocatedCapacity { get; set; }

    [Range(0, int.MaxValue,
        ErrorMessage = "ظرفیت داوطلبان دانشگاه نمی‌تواند منفی باشد")]
    public int OwnCapacity { get; set; }
}
