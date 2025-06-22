using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AdminSettingsVM
{
    public class DepartmentSettingsVM:BaseViewModel
    {
        public int DepartmentID { get; set; }

        public string? DepartmentName { get; set; }
        public int? OrganizationID { get; set; }

        public bool? IsDepartmentHead { get; set; }
    }
}
