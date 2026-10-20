using SanjeshP.Payeh.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanjeshP.Payeh.Entities.Menu
{
    public class GroupAccessMenus : BaseEntity<int>
    {
        [Required(ErrorMessage = "شناسه گروه الزامی است")]
        [Display(Name = "شناسه گروه")]
        [ForeignKey(nameof(Group))]
        public Guid GroupId { get; set; }

        [Required(ErrorMessage = "شناسه منو الزامی است")]
        [Display(Name = "شناسه منو")]
        [ForeignKey(nameof(Menu))]
        public Guid MenuId { get; set; }

        [Display(Name = "گروه")]
        public virtual Group.Group Group { get; set; }

        [Display(Name = "منو")]
        public virtual Menu Menu { get; set; }


    }
}
