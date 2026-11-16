using SanjeshP.Payeh.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanjeshP.Payeh.Entities.Exam
{
    public class Exam : BaseEntity
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
        public virtual ExamSession ExamSession { get; set; }

        [Display(Name = "نوع آزمون")]
        public int? ExamTypeId { get; set; }

        [Required(ErrorMessage = "نحوه برگزاری آزمون الزامی است")]
        [Display(Name = "نحوه برگزاری")]
        public int ExamMode { get; set; }

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
    }
}