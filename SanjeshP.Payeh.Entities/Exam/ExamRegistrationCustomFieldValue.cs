
using SanjeshP.Payeh.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanjeshP.Payeh.Entities.Exam
{
    public class ExamRegistrationCustomFieldValue : BaseEntity
    {
        [Required(ErrorMessage = "شناسه ثبت‌نام الزامی است")]
        [Display(Name = "ثبت‌نام")]
        public Guid RegistrationId { get; set; }

        [ForeignKey(nameof(RegistrationId))]
        [Display(Name = "ثبت‌نام")]
        public virtual ExamRegistration Registration { get; set; } = null!;

        [Required(ErrorMessage = "شناسه فیلد اختصاصی الزامی است")]
        [Display(Name = "فیلد اختصاصی")]
        public Guid ExamCustomFieldId { get; set; }

        [ForeignKey(nameof(ExamCustomFieldId))]
        [Display(Name = "فیلد اختصاصی")]
        public virtual ExamCustomField ExamCustomField { get; set; } = null!;

        [MaxLength(4000, ErrorMessage = "مقدار واردشده نباید بیش از 4000 کاراکتر باشد")]
        [Display(Name = "مقدار")]
        public string? Value { get; set; }
    }
}
