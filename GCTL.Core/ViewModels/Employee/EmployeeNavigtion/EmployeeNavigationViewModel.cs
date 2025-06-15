using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeNavigtion
{
    public class EmployeeNavigationViewModel
    {
        public List<NavigationItem> MainNavigation { get; set; } = new List<NavigationItem>();
        public List<NavigationItem> SubNavigation { get; set; } = new List<NavigationItem>();
        public string ActiveTab { get; set; }
        public string ActiveSubTab { get; set; }
        public bool ShowSubNavigation => SubNavigation != null && SubNavigation.Count > 0;
    }
}
