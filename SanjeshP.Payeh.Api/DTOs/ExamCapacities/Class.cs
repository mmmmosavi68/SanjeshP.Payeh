
namespace SanjeshP.Payeh.Api.DTOs.ExamCapacities;

public sealed class UniversityExamCapacityDto
{
    public Guid Id { get; set; }

    public int UniversityId { get; set; }

    public Guid ExamId { get; set; }
    public string? ExamTitle { get; set; }

    public Guid ExamSessionId { get; set; }
    public string? ExamSessionTitle { get; set; }

    public int AllocatedCapacity { get; set; }
    public int OwnCapacity { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
