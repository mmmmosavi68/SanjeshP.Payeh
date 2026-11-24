using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Api.DTOs.Exams;

public sealed class CreateExamDto
{
    [Required(ErrorMessage = "عنوان آزمون الزامی است")]
    [MaxLength(200, ErrorMessage = "عنوان آزمون نباید بیش از ۲۰۰ کاراکتر باشد")]
    public string Title { get; set; } = string.Empty;

    [Required]
    public Guid ExamSessionId { get; set; }

    public int ExamTypeId { get; set; }
    public int? ExamTypePrimaryCode { get; set; }
    public int? ExamTypeSubCode { get; set; }

    public int ExamModeId { get; set; }
    public int? ExamModePrimaryCode { get; set; }
    public int? ExamModeSubCode { get; set; }

    public DateTime RegistrationStartDate { get; set; }
    public DateTime RegistrationEndDate { get; set; }
    public DateTime? EditStartDate { get; set; }
    public DateTime? EditEndDate { get; set; }

    [MaxLength(1000, ErrorMessage = "توضیحات نباید بیش از ۱۰۰۰ کاراکتر باشد")]
    public string? Description { get; set; }
}