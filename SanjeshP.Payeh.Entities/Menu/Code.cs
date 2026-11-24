
using SanjeshP.Payeh.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanjeshP.Payeh.Entities.Menu;

public class Code : BaseEntity<int>
{
    [Required(ErrorMessage = "عنوان الزامی است")]
    [MaxLength(200, ErrorMessage = "عنوان نباید بیش از 200 کاراکتر باشد")]
    [Display(Name = "عنوان")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "شناسه والد")]
    [ForeignKey(nameof(Parent))]
    public int? ParentId { get; set; }

    [Display(Name = "کد استاندارد")]
    public int? StandardCode { get; set; }

    [Display(Name = "کد اصلی")]
    public int? PrimaryCode { get; set; }

    [Display(Name = "کد فرعی")]
    public int? SubCode { get; set; }

    [MaxLength(2000, ErrorMessage = "اطلاعات اضافی نباید بیش از 2000 کاراکتر باشد")]
    [Display(Name = "اطلاعات اضافی")]
    public string? AdditionalInformation { get; set; }

    [Display(Name = "والد")]
    public virtual Code? Parent { get; set; }

    [Display(Name = "فرزندان")]
    public virtual ICollection<Code> InverseParent { get; set; } = new List<Code>();
}
