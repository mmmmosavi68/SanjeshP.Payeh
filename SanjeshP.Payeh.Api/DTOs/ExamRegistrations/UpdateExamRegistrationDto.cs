
using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Api.DTOs.ExamRegistrations;

public sealed class UpdateExamRegistrationDto
{
    [Range(1, int.MaxValue)]
    public int UniversityId { get; set; }

    [Range(1, int.MaxValue)]
    public int ExamVenueUniversityId { get; set; }

    [Required]
    public Guid UniversityExamCapacityId { get; set; }

    public Guid? UniversityExamGuestCapacityId { get; set; }

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? FatherName { get; set; }

    [Required]
    [MaxLength(10)]
    public string NationalCode { get; set; } = string.Empty;

    public int? GenderId { get; set; }
    public int? GenderPrimaryCode { get; set; }
    public int? GenderSubCode { get; set; }

    [MaxLength(20)]
    public string? BirthCertificateNumber { get; set; }

    public DateTime? BirthDate { get; set; }

    [MaxLength(30)]
    public string? StudentNumber { get; set; }

    public int? EntryYear { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(20)]
    public string? EmergencyPhoneNumber { get; set; }

    public List<ExamRegistrationCustomFieldValueDto> CustomFieldValues { get; set; } = [];
}
