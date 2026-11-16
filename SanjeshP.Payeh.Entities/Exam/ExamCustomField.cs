using SanjeshP.Payeh.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Entities.Exam
{
    public class ExamCustomField : BaseEntity
    {
        [Required(ErrorMessage = "شناسه آزمون الزامی است")]
        [Display(Name = "آزمون")]
        public Guid ExamId { get; set; }

        [Required(ErrorMessage = "نام فیلد الزامی است")]
        [MaxLength(100, ErrorMessage = "نام فیلد نباید بیش از 100 کاراکتر باشد")]
        [Display(Name = "نام فیلد")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "عنوان فیلد الزامی است")]
        [MaxLength(150, ErrorMessage = "عنوان فیلد نباید بیش از 150 کاراکتر باشد")]
        [Display(Name = "عنوان فیلد")]
        public string Label { get; set; } = string.Empty;

        [Required(ErrorMessage = "نوع فیلد الزامی است")]
        [Display(Name = "نوع فیلد")]
        public int FieldTypeId { get; set; }

        [Required(ErrorMessage = "وضعیت اجباری بودن فیلد الزامی است")]
        [Display(Name = "اجباری")]
        public bool IsRequired { get; set; }

        [Display(Name = "گزینه‌های فیلد")]
        public string? OptionsJson { get; set; }

        [Display(Name = "قوانین اعتبارسنجی")]
        public string? ValidationJson { get; set; }

        [Required(ErrorMessage = "ترتیب نمایش الزامی است")]
        [Range(0, int.MaxValue, ErrorMessage = "ترتیب نمایش نمی‌تواند منفی باشد")]
        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }

        [Display(Name = "آزمون")]
        public virtual Exam Exam { get; set; }

        [Display(Name = "مقادیر ثبت‌شده")]
        public virtual ICollection<ExamRegistrationCustomFieldValue> RegistrationValues { get; set; }
            = new List<ExamRegistrationCustomFieldValue>();
    }
}