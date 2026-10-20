using SanjeshP.Payeh.Entities.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanjeshP.Payeh.Entities.User
{
    public class UserRole : BaseEntity<int>
    {
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        [Display(Name = "شناسه کاربر")]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "شناسه نقش الزامی است")]
        [Display(Name = "شناسه نقش")]
        [ForeignKey(nameof(Role))]
        public int RoleId { get; set; }

        [Display(Name = "نقش")]
        public virtual Role Role { get; set; }

        [Display(Name = "کاربر")]
        public virtual User User { get; set; }
    }
}
