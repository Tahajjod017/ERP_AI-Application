using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AdminSettingsVM
{
    public class DepartmentSettingsVM:BaseViewModel
    {
       
        public int DepartmentID { get; set; }
        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string? DepartmentName { get; set; }
        [Required(ErrorMessage = "Organization is required")]
        public int? OrganizationID { get; set; }
        public string? OrganizationName { get; set; }

        public int? DepartmentHeadEmpID { get; set; } 
        public string? HeadEmployeeName { get; set; }
    }
}
