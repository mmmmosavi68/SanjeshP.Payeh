using SanjeshP.Payeh.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanjeshP.Payeh.Entities.Menu
{
    public class Code : BaseEntity<int>
    {
        [Required(ErrorMessage = "عنوان الزامی است")]
        [MaxLength(100, ErrorMessage = "عنوان نباید بیش از 100 کاراکتر باشد")]
        [Display(Name = "عنوان")]
        public string Title { get; set; }

        [Display(Name = "شناسه والد")]
        [ForeignKey("Parent")]
        public int? ParentId { get; set; }

        [Display(Name = "کد استاندارد")]
        public int? StandardCode { get; set; }

        // تغییر نام "کد اصلی" به "کد اصلی"
        [Display(Name = "کد اصلی")]
        public int? PrimaryCode { get; set; }

        // تغییر نام "کد فرعی" به "کد فرعی"
        [Display(Name = "کد فرعی")]
        public int? SubCode { get; set; }

        [MaxLength(200, ErrorMessage = "اطلاعات اضافی نباید بیش از 200 کاراکتر باشد")]
        [Display(Name = "اطلاعات اضافی")]
        public string AdditionalInformation { get; set; }

        [Display(Name = "والد")]
        public virtual Code Parent { get; set; }

        [Display(Name = "فرزندان")]
        public virtual ICollection<Code> InverseParent { get; set; } = new List<Code>();

    }
}
