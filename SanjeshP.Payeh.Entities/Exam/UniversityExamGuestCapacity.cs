using SanjeshP.Payeh.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanjeshP.Payeh.Entities.Exam
{
    public class UniversityExamGuestCapacity : BaseEntity
    {
        [Required(ErrorMessage = "شناسه تخصیص ظرفیت الزامی است")]
        [Display(Name = "تخصیص ظرفیت آزمون")]
        public Guid UniversityExamCapacityId { get; set; }

        [ForeignKey(nameof(UniversityExamCapacityId))]
        [Display(Name = "تخصیص ظرفیت آزمون")]
        public virtual UniversityExamCapacity UniversityExamCapacity { get; set; }

        [Required(ErrorMessage = "شناسه دانشگاه مهمان الزامی است")]
        [Display(Name = "دانشگاه مهمان")]
        public int GuestUniversityId { get; set; }

        [Required(ErrorMessage = "ظرفیت دانشگاه مهمان الزامی است")]
        [Range(1, int.MaxValue, ErrorMessage = "ظرفیت دانشگاه مهمان باید حداقل 1 باشد")]
        [Display(Name = "ظرفیت دانشگاه مهمان")]
        public int Capacity { get; set; }
    }
}