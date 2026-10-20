using SanjeshP.Payeh.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanjeshP.Payeh.Entities.Menu
{
    [NotMapped]
    //[IgnoreMigrationAttribute]
    public class View_UserMenubar : BaseEntity /*IIgnoreDependency*/
    {
        public string MenuTitle { get; set; }
        public Guid? ParentId { get; set; }
        public int? PageCode { get; set; }
        public string Area { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public bool? ShowMenu { get; set; }
        public string CssClass { get; set; }
        public string Icon { get; set; }
        public string Link { get; set; }
        public bool CurentUser_Person_Checkecd { get; set; }
        public bool CurentUser_Group_Checkecd { get; set; }
        public int IsParent { get; set; }
        public bool Person_Checkecd { get; set; }
        public bool Group_Checkecd { get; set; }
        public bool disabled { get; set; }

        [NotMapped]
        public DateTime CreatedDate { get; set; }
        [NotMapped]
        public DateTime? UpdatedDate { get; set; }
        [NotMapped]
        public bool IsActive { get; set; }
        [NotMapped]
        public bool IsDeleted { get; set; }
        [NotMapped]
        public Guid CreatedBy { get; set; }
        [NotMapped]
        public string HostIp { get; set; }
    }
}
