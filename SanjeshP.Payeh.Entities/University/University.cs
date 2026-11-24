
using SanjeshP.Payeh.Entities.Common;
using SanjeshP.Payeh.Entities.Menu;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanjeshP.Payeh.Entities.University;

public class University : BaseEntity<int>
{
    [Required(ErrorMessage = "نام دانشگاه الزامی است")]
    [MaxLength(200, ErrorMessage = "نام دانشگاه نباید بیش از 200 کاراکتر باشد")]
    [Display(Name = "نام دانشگاه")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "نوع دانشگاه الزامی است")]
    [Display(Name = "نوع دانشگاه")]
    public int UniversityTypeId { get; set; }

    [ForeignKey(nameof(UniversityTypeId))]
    [Display(Name = "نوع دانشگاه")]
    public virtual Code? UniversityType { get; set; }

    [Range(1, 11, ErrorMessage = "منطقه دانشگاه باید بین ۱ تا ۱۱ باشد")]
    [Display(Name = "منطقه دانشگاه")]
    public int Region { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "ظرفیت مصوب نمی‌تواند منفی باشد")]
    [Display(Name = "ظرفیت مصوب")]
    public int CapacityApproved { get; set; }

    [Display(Name = "شناسه دانشگاه مادر")]
    public int? ParentUniversityId { get; set; }

    [ForeignKey(nameof(ParentUniversityId))]
    [Display(Name = "دانشگاه مادر")]
    public virtual University? ParentUniversity { get; set; }

    [Display(Name = "پردیس‌ها و زیرمجموعه‌ها")]
    public virtual ICollection<University> ChildUniversities { get; set; }
        = new List<University>();
}
