using System;
using System.Collections.Generic;
using System.Text;

namespace SanjeshP.Payeh.Common
{
    public class UserAccessViewModel
    {
        public string MenuTitle { get; set; }
        public int PageCode { get; set; }
        public bool SowMenu { get; set; }
        public string CssClass { get; set; }
        public string Icon { get; set; }
        public string Area { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public bool Person_Checkecd { get; set; }
        public bool Group_Checkecd { get; set; }
        public bool Disabled { get; set; }
        public int IsParent { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RoleNameFa { get; set; }
    }
}
