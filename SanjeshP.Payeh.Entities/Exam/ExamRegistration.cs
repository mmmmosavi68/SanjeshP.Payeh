using SanjeshP.Payeh.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanjeshP.Payeh.Entities.Exam
{
    public class ExamRegistration : BaseEntity
    {
        [Required(ErrorMessage = "شناسه آزمون الزامی است")]
        [Display(Name = "آزمون")]
        public Guid ExamId { get; set; }

        [ForeignKey(nameof(ExamId))]
        [Display(Name = "آزمون")]
        public virtual Exam Exam { get; set; }

        [Required(ErrorMessage = "دانشگاه داوطلب الزامی است")]
        [Display(Name = "دانشگاه محل تحصیل")]
        public int UniversityId { get; set; }

        [Required(ErrorMessage = "دانشگاه محل آزمون الزامی است")]
        [Display(Name = "دانشگاه محل آزمون")]
        public int ExamVenueUniversityId { get; set; }

        [Required(ErrorMessage = "شناسه تخصیص ظرفیت الزامی است")]
        [Display(Name = "تخصیص ظرفیت")]
        public Guid UniversityExamCapacityId { get; set; }

        [ForeignKey(nameof(UniversityExamCapacityId))]
        [Display(Name = "تخصیص ظرفیت")]
        public virtual UniversityExamCapacity UniversityExamCapacity { get; set; }

        [Display(Name = "ظرفیت دانشگاه مهمان")]
        public Guid? UniversityExamGuestCapacityId { get; set; }

        [ForeignKey(nameof(UniversityExamGuestCapacityId))]
        [Display(Name = "ظرفیت دانشگاه مهمان")]
        public virtual UniversityExamGuestCapacity? UniversityExamGuestCapacity { get; set; }

        [Required(ErrorMessage = "نام الزامی است")]
        [MaxLength(50, ErrorMessage = "نام نباید بیش از 50 کاراکتر باشد")]
        [Display(Name = "نام")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        [MaxLength(50, ErrorMessage = "نام خانوادگی نباید بیش از 50 کاراکتر باشد")]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(50, ErrorMessage = "نام پدر نباید بیش از 50 کاراکتر باشد")]
        [Display(Name = "نام پدر")]
        public string? FatherName { get; set; }

        [Required(ErrorMessage = "کد ملی الزامی است")]
        [MaxLength(10, ErrorMessage = "کد ملی نباید بیش از 10 کاراکتر باشد")]
        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; } = string.Empty;

        [Display(Name = "جنسیت")]
        public int? GenderId { get; set; }

        [MaxLength(20, ErrorMessage = "شماره شناسنامه نباید بیش از 20 کاراکتر باشد")]
        [Display(Name = "شماره شناسنامه")]
        public string? BirthCertificateNumber { get; set; }

        [Display(Name = "تاریخ تولد")]
        public DateTime? BirthDate { get; set; }

        [MaxLength(30, ErrorMessage = "شماره دانشجویی نباید بیش از 30 کاراکتر باشد")]
        [Display(Name = "شماره دانشجویی")]
        public string? StudentNumber { get; set; }

        [Display(Name = "سال ورود")]
        public int? EntryYear { get; set; }

        [MaxLength(20, ErrorMessage = "شماره تلفن نباید بیش از 20 کاراکتر باشد")]
        [Display(Name = "شماره تلفن")]
        public string? PhoneNumber { get; set; }

        [MaxLength(20, ErrorMessage = "شماره تلفن ضروری نباید بیش از 20 کاراکتر باشد")]
        [Display(Name = "تلفن ضروری")]
        public string? EmergencyPhoneNumber { get; set; }

        [Display(Name = "مقادیر فیلدهای اختصاصی")]
        public virtual ICollection<ExamRegistrationCustomFieldValue> CustomFieldValues { get; set; }
            = new List<ExamRegistrationCustomFieldValue>();
    }
}