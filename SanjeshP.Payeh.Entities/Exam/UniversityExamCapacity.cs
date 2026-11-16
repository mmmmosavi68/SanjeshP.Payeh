
using SanjeshP.Payeh.Entities.Common;
using SanjeshP.Payeh.Entities.University;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanjeshP.Payeh.Entities.Exam
{
    public class UniversityExamCapacity : BaseEntity
    {
        [Required(ErrorMessage = "شناسه دانشگاه الزامی است")]
        [Display(Name = "دانشگاه برگزارکننده")]
        public int UniversityId { get; set; }

        [ForeignKey(nameof(UniversityId))]
        [Display(Name = "دانشگاه برگزارکننده")]
        public virtual University.University University { get; set; } = null!;

        [Required(ErrorMessage = "شناسه آزمون الزامی است")]
        [Display(Name = "آزمون")]
        public Guid ExamId { get; set; }

        [ForeignKey(nameof(ExamId))]
        [Display(Name = "آزمون")]
        public virtual Exam Exam { get; set; } = null!;

        [Range(0, int.MaxValue, ErrorMessage = "ظرفیت تخصیص‌یافته نمی‌تواند منفی باشد")]
        [Display(Name = "ظرفیت تخصیص‌یافته")]
        public int? AllocatedCapacity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "ظرفیت داوطلبان دانشگاه نمی‌تواند منفی باشد")]
        [Display(Name = "ظرفیت داوطلبان دانشگاه")]
        public int? OwnCapacity { get; set; }

        [Display(Name = "ظرفیت دانشگاه‌های مهمان")]
        public virtual ICollection<UniversityExamGuestCapacity> GuestCapacities { get; set; }
            = new List<UniversityExamGuestCapacity>();
    }
}
