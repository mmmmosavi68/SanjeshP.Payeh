using SanjeshP.Payeh.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanjeshP.Payeh.Entities.User
{
    public class UserOtp : BaseEntity
    {
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        [Display(Name = "شناسه کاربر")]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "کد یک‌بارمصرف الزامی است")]
        [MaxLength(10)]
        [Display(Name = "کد یک‌بارمصرف")]
        public string Code { get; set; }

        [Required(ErrorMessage = "تاریخ انقضا الزامی است")]
        [Display(Name = "تاریخ انقضا")]
        public DateTime ExpireAt { get; set; }

        [Display(Name = "مصرف شده")]
        public bool IsUsed { get; set; } = false;

        [MaxLength(100)]
        [Display(Name = "مقصد ارسال")]
        public string SentTo { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "هدف کد")]
        public string Purpose { get; set; }

        [Display(Name = "کاربر")]
        public virtual User User { get; set; }
    }
}