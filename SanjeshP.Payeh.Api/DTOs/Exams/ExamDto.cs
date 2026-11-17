namespace SanjeshP.Payeh.Api.DTOs.Exams;

public sealed class ExamDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;

    public Guid ExamSessionId { get; set; }
    public string? ExamSessionTitle { get; set; }
    public DateTime ExamDate { get; set; }

    public int ExamTypeId { get; set; }
    public string? ExamTypeTitle { get; set; }

    public int ExamModeId { get; set; }
    public string? ExamModeTitle { get; set; }

    public DateTime RegistrationStartDate { get; set; }
    public DateTime RegistrationEndDate { get; set; }
    public DateTime? EditStartDate { get; set; }
    public DateTime? EditEndDate { get; set; }

    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}