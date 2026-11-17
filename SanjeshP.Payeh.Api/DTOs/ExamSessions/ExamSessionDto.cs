
namespace SanjeshP.Payeh.Api.DTOs.ExamSessions;

public sealed class ExamSessionDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime ExamDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
