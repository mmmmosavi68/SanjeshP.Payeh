using System;
using System.Collections.Generic;
using System.Text;

namespace SanjeshP.Payeh.Common
{
    public class UserAccessViewModel
    {
        public string MenuTitle { get; set; } = string.Empty;
        public int PageCode { get; set; }
        public bool SowMenu { get; set; }
        public string CssClass { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string ControllerName { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public bool Person_Checkecd { get; set; }
        public bool Group_Checkecd { get; set; }
        public bool Disabled { get; set; }
        public int IsParent { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string RoleNameFa { get; set; } = string.Empty;
    }
}
