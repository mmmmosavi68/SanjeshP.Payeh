namespace SanjeshP.Payeh.Api.DTOs.ExamRegistrations;

public sealed class ExamRegistrationOptionsDto
{
    public List<ExamRegistrationCapacityOptionDto> Capacities { get; set; } = [];
    public List<ExamRegistrationCustomFieldDto> CustomFields { get; set; } = [];
}

public sealed class ExamRegistrationCapacityOptionDto
{
    public Guid UniversityExamCapacityId { get; set; }

    public int ExamVenueUniversityId { get; set; }
    public string ExamVenueUniversityTitle { get; set; } = string.Empty;

    public int? AllocatedCapacity { get; set; }
    public int? OwnCapacity { get; set; }

    public bool IsOwnUniversity { get; set; }

    public Guid? UniversityExamGuestCapacityId { get; set; }
    public int? GuestCapacity { get; set; }
}

public sealed class ExamRegistrationCustomFieldDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int FieldTypeId { get; set; }
    public bool IsRequired { get; set; }
    public string? OptionsJson { get; set; }
    public string? ValidationJson { get; set; }
    public int DisplayOrder { get; set; }
}