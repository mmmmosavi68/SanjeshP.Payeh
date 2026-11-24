
namespace SanjeshP.Payeh.Api.DTOs.Universities;

public sealed class UniversityDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public int UniversityTypeId { get; set; }
    public int? UniversityTypePrimaryCode { get; set; }
    public int? UniversityTypeSubCode { get; set; }

    public string? UniversityTypeTitle { get; set; }

    public int Region { get; set; }

    public int CapacityApproved { get; set; }

    public int? ParentUniversityId { get; set; }

    public string? ParentUniversityTitle { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
