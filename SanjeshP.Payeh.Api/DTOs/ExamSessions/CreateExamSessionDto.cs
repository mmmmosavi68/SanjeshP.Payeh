using System.ComponentModel.DataAnnotations;



namespace SanjeshP.Payeh.Api.DTOs.ExamSessions;

public sealed class CreateExamSessionDto
{
    [Required(ErrorMessage = "عنوان نوبت الزامی است")]
    [MaxLength(100, ErrorMessage = "عنوان نوبت نباید بیشتر از ۱۰۰ کاراکتر باشد")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "تاریخ برگزاری الزامی است")]
    public DateTime ExamDate { get; set; }

    [Required(ErrorMessage = "ساعت شروع الزامی است")]
    public TimeOnly StartTime { get; set; }

    [Required(ErrorMessage = "ساعت پایان الزامی است")]
    public TimeOnly EndTime { get; set; }
}
