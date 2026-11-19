namespace SanjeshP.Payeh.Api.DTOs.ExamCapacities;

public sealed class UniversityExamGuestCapacityDto
{
    public Guid Id { get; set; }

    public Guid UniversityExamCapacityId { get; set; }

    public int GuestUniversityId { get; set; }

    public string GuestUniversityTitle { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}