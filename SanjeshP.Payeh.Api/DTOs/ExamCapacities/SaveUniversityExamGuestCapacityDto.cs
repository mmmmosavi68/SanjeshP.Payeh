using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Api.DTOs.ExamCapacities;

public sealed class SaveUniversityExamGuestCapacityDto
{
    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "ظرفیت دانشگاه مهمان باید حداقل ۱ باشد")]
    public int Capacity { get; set; }
}