using SanjeshP.Payeh.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanjeshP.Payeh.Entities.Menu
{
    public class Menu : BaseEntity
    {
        [Required(ErrorMessage = "عنوان الزامی است")]
        [MaxLength(100, ErrorMessage = "عنوان نباید بیش از 100 کاراکتر باشد")]
        [Display(Name = "عنوان")]
        public string MenuTitle { get; set; }

        [Display(Name = "شناسه والد")]
        public Guid? ParentId { get; set; }

        [ForeignKey(nameof(ParentId))]
        public virtual Menu Parent { get; set; }

        [Display(Name = "کد صفحه")]
        public int? PageCode { get; set; }

        [MaxLength(200, ErrorMessage = "مسیر نباید بیش از 200 کاراکتر باشد")]
        [Display(Name = "مسیر")]
        public string Path { get; set; }

        [MaxLength(100, ErrorMessage = "ورژن نباید بیش از 100 کاراکتر باشد")]
        [Display(Name = "ورژن")]
        public string version { get; set; }

        [MaxLength(100, ErrorMessage = "منطقه نباید بیش از 100 کاراکتر باشد")]
        [Display(Name = "منطقه")]
        public string Area { get; set; }

        [MaxLength(100, ErrorMessage = "نام کنترلر نباید بیش از 100 کاراکتر باشد")]
        [Display(Name = "نام کنترلر")]
        public string ControllerName { get; set; }

        [MaxLength(100, ErrorMessage = "نام اکشن نباید بیش از 100 کاراکتر باشد")]
        [Display(Name = "نام اکشن")]
        public string ActionName { get; set; }

        [Display(Name = "نمایش منو")]
        public bool? ShowMenu { get; set; }

        [MaxLength(50, ErrorMessage = "کلاس CSS نباید بیش از 50 کاراکتر باشد")]
        [Display(Name = "کلاس CSS")]
        public string CssClass { get; set; }

        [MaxLength(100, ErrorMessage = "آیکن نباید بیش از 100 کاراکتر باشد")]
        [Display(Name = "آیکن")]
        public string Icon { get; set; }

        [MaxLength(200, ErrorMessage = "لینک نباید بیش از 200 کاراکتر باشد")]
        [Display(Name = "لینک")]
        public string Link { get; set; }

        public virtual ICollection<Menu> InverseParent { get; set; } = new List<Menu>();
        public virtual ICollection<UserAccessMenus> MenusAccesses { get; set; } = new List<UserAccessMenus>();
        public virtual ICollection<GroupAccessMenus> MenusGroupAccesses { get; set; } = new List<GroupAccessMenus>();
    }
}
