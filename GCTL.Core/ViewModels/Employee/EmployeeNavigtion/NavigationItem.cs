using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeNavigtion
{
    public class NavigationItem
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public bool IsActive { get; set; }
        public string AriaControls { get; set; }
        public List<NavigationItem> SubItems { get; set; } = new List<NavigationItem>();
        public bool HasSubItems => SubItems != null && SubItems.Count > 0;
    }
}
