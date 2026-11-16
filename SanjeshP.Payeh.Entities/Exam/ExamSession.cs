using SanjeshP.Payeh.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Entities.Exam
{
    public class ExamSession : BaseEntity
    {
        [Required(ErrorMessage = "عنوان نوبت الزامی است")]
        [MaxLength(100, ErrorMessage = "عنوان نوبت نباید بیش از 100 کاراکتر باشد")]
        [Display(Name = "عنوان نوبت")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاریخ برگزاری الزامی است")]
        [Display(Name = "تاریخ برگزاری")]
        public DateTime ExamDate { get; set; }

        [Required(ErrorMessage = "ساعت شروع الزامی است")]
        [Display(Name = "ساعت شروع")]
        public TimeOnly StartTime { get; set; }

        [Required(ErrorMessage = "ساعت پایان الزامی است")]
        [Display(Name = "ساعت پایان")]
        public TimeOnly EndTime { get; set; }

        [Display(Name = "آزمون‌ها")]
        public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
    }
}