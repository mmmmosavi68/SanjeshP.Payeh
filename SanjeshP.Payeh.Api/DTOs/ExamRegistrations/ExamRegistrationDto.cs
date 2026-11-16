namespace SanjeshP.Payeh.Api.DTOs.ExamRegistrations;

public sealed class ExamRegistrationDto
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public int UniversityId { get; set; }
    public string? UniversityTitle { get; set; }
    public int ExamVenueUniversityId { get; set; }
    public string? ExamVenueUniversityTitle { get; set; }
    public Guid UniversityExamCapacityId { get; set; }
    public Guid? UniversityExamGuestCapacityId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? FatherName { get; set; }
    public string NationalCode { get; set; } = string.Empty;
    public int? GenderId { get; set; }
    public string? BirthCertificateNumber { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? StudentNumber { get; set; }
    public int? EntryYear { get; set; }
    public string? PhoneNumber { get; set; }
    public string? EmergencyPhoneNumber { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<ExamRegistrationCustomFieldValueResultDto> CustomFieldValues { get; set; } = [];
}

public sealed class ExamRegistrationCustomFieldValueResultDto
{
    public Guid ExamCustomFieldId { get; set; }
    public string? FieldLabel { get; set; }
    public string? Value { get; set; }
}
