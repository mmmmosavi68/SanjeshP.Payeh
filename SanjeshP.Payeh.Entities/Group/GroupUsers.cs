using SanjeshP.Payeh.Entities.Common;
using SanjeshP.Payeh.Entities.Menu;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanjeshP.Payeh.Entities.Group
{
    public class GroupUsers : BaseEntity
    {
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        [Display(Name = "شناسه کاربر")]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "شناسه گروه الزامی است")]
        [Display(Name = "شناسه گروه")]
        [ForeignKey(nameof(Group))]
        public Guid GroupId { get; set; }

        [Display(Name = "کاربر")]
        public virtual User.User User { get; set; }

        [Display(Name = "گروه")]
        public virtual Group Group { get; set; }
        public virtual ICollection<GroupAccessMenus> GroupAccessMenus { get; set; } = new List<GroupAccessMenus>();

    }
}
