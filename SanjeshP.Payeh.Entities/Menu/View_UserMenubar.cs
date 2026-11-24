using SanjeshP.Payeh.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanjeshP.Payeh.Entities.Menu
{
    [NotMapped]
    //[IgnoreMigrationAttribute]
    public class View_UserMenubar : BaseEntity /*IIgnoreDependency*/
    {
        public string MenuTitle { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
        public int? PageCode { get; set; }
        public string Area { get; set; } = string.Empty;
        public string ControllerName { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public bool? ShowMenu { get; set; }
        public string CssClass { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public bool CurentUser_Person_Checkecd { get; set; }
        public bool CurentUser_Group_Checkecd { get; set; }
        public int IsParent { get; set; }
        public bool Person_Checkecd { get; set; }
        public bool Group_Checkecd { get; set; }
        public bool disabled { get; set; }

        [NotMapped]
        public new DateTime CreatedDate { get; set; }
        [NotMapped]
        public new DateTime? UpdatedDate { get; set; }
        [NotMapped]
        public new bool IsActive { get; set; }
        [NotMapped]
        public new bool IsDeleted { get; set; }
        [NotMapped]
        public new Guid CreatedBy { get; set; }
        [NotMapped]
        public new string HostIp { get; set; } = string.Empty;
    }
}
