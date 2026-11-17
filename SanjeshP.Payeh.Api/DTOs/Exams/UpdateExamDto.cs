using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Api.DTOs.Exams;

public sealed class UpdateExamDto
{
    [Required(ErrorMessage = "عنوان آزمون الزامی است")]
    [MaxLength(200, ErrorMessage = "عنوان آزمون نباید بیش از ۲۰۰ کاراکتر باشد")]
    public string Title { get; set; } = string.Empty;

    [Required]
    public Guid ExamSessionId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "نوع آزمون معتبر نیست")]
    public int ExamTypeId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "نحوه برگزاری معتبر نیست")]
    public int ExamModeId { get; set; }

    public DateTime RegistrationStartDate { get; set; }
    public DateTime RegistrationEndDate { get; set; }
    public DateTime? EditStartDate { get; set; }
    public DateTime? EditEndDate { get; set; }

    [MaxLength(1000, ErrorMessage = "توضیحات نباید بیش از ۱۰۰۰ کاراکتر باشد")]
    public string? Description { get; set; }

    public bool IsActive { get; set; }
}