using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Api.DTOs.ExamRegistrations;

public sealed class CreateExamRegistrationDto
{
    [Required(ErrorMessage = "انتخاب آزمون الزامی است")]
    public Guid ExamId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "دانشگاه محل تحصیل معتبر نیست")]
    public int UniversityId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "دانشگاه محل آزمون معتبر نیست")]
    public int ExamVenueUniversityId { get; set; }

    [Required(ErrorMessage = "انتخاب ظرفیت الزامی است")]
    public Guid UniversityExamCapacityId { get; set; }

    public Guid? UniversityExamGuestCapacityId { get; set; }

    [Required(ErrorMessage = "نام داوطلب الزامی است")]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام خانوادگی داوطلب الزامی است")]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? FatherName { get; set; }

    [Required(ErrorMessage = "کد ملی الزامی است")]
    [MaxLength(10)]
    public string NationalCode { get; set; } = string.Empty;

    public int? GenderId { get; set; }

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

public sealed class ExamRegistrationCustomFieldValueDto
{
    [Required]
    public Guid ExamCustomFieldId { get; set; }

    [MaxLength(4000)]
    public string? Value { get; set; }
}
