namespace SanjeshP.Payeh.Api.DTOs.ExamCapacities;

public sealed class UniversityExamCapacityUniversityDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public int UniversityTypeId { get; set; }

    public string? UniversityTypeTitle { get; set; }

    public int Region { get; set; }

    public int CapacityApproved { get; set; }

    public int? ParentUniversityId { get; set; }

    public string? ParentUniversityTitle { get; set; }

    public bool IsCampus => ParentUniversityId.HasValue;
}