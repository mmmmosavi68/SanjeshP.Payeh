
using SanjeshP.Payeh.Entities.Common;
using SanjeshP.Payeh.Entities.Menu;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanjeshP.Payeh.Entities.Exam
{
    public class Exam : BaseEntity, IValidatableObject
    {
        [Required(ErrorMessage = "عنوان آزمون الزامی است")]
        [MaxLength(200, ErrorMessage = "عنوان آزمون نباید بیش از 200 کاراکتر باشد")]
        [Display(Name = "عنوان آزمون")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "شناسه نوبت آزمون الزامی است")]
        [Display(Name = "نوبت آزمون")]
        public Guid ExamSessionId { get; set; }

        [ForeignKey(nameof(ExamSessionId))]
        [Display(Name = "نوبت آزمون")]
        public virtual ExamSession? ExamSession { get; set; }

        [Required(ErrorMessage = "نوع آزمون الزامی است")]
        [Display(Name = "نوع آزمون")]
        public int ExamTypeId { get; set; }

        [ForeignKey(nameof(ExamTypeId))]
        [Display(Name = "نوع آزمون")]
        public virtual Code? ExamType { get; set; }

        [Required(ErrorMessage = "نحوه برگزاری آزمون الزامی است")]
        [Display(Name = "نحوه برگزاری")]
        public int ExamModeId { get; set; }

        [ForeignKey(nameof(ExamModeId))]
        [Display(Name = "نحوه برگزاری")]
        public virtual Code? ExamMode { get; set; }

        [Required(ErrorMessage = "تاریخ شروع ثبت‌نام الزامی است")]
        [Display(Name = "شروع ثبت‌نام")]
        public DateTime RegistrationStartDate { get; set; }

        [Required(ErrorMessage = "تاریخ پایان ثبت‌نام الزامی است")]
        [Display(Name = "پایان ثبت‌نام")]
        public DateTime RegistrationEndDate { get; set; }

        [Display(Name = "شروع ویرایش اطلاعات")]
        public DateTime? EditStartDate { get; set; }

        [Display(Name = "پایان ویرایش اطلاعات")]
        public DateTime? EditEndDate { get; set; }

        [MaxLength(1000, ErrorMessage = "توضیحات نباید بیش از 1000 کاراکتر باشد")]
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Display(Name = "ظرفیت‌های دانشگاه‌ها")]
        public virtual ICollection<UniversityExamCapacity> UniversityExamCapacities { get; set; }
            = new List<UniversityExamCapacity>();

        [Display(Name = "ثبت‌نام داوطلبان")]
        public virtual ICollection<ExamRegistration> Registrations { get; set; }
            = new List<ExamRegistration>();

        [Display(Name = "فیلدهای اختصاصی")]
        public virtual ICollection<ExamCustomField> CustomFields { get; set; }
            = new List<ExamCustomField>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (RegistrationEndDate < RegistrationStartDate)
            {
                yield return new ValidationResult(
                    "تاریخ پایان ثبت‌نام نمی‌تواند قبل از تاریخ شروع ثبت‌نام باشد",
                    new[] { nameof(RegistrationEndDate) });
            }

            if (EditStartDate.HasValue && !EditEndDate.HasValue)
            {
                yield return new ValidationResult(
                    "با تعیین شروع ویرایش، تعیین پایان ویرایش نیز الزامی است",
                    new[] { nameof(EditEndDate) });
            }

            if (!EditStartDate.HasValue && EditEndDate.HasValue)
            {
                yield return new ValidationResult(
                    "برای تعیین پایان ویرایش، ابتدا باید شروع ویرایش مشخص شود",
                    new[] { nameof(EditStartDate) });
            }

            if (EditStartDate.HasValue &&
                EditEndDate.HasValue &&
                EditEndDate.Value < EditStartDate.Value)
            {
                yield return new ValidationResult(
                    "پایان ویرایش نمی‌تواند قبل از شروع ویرایش باشد",
                    new[] { nameof(EditEndDate) });
            }
        }
    }


}
    