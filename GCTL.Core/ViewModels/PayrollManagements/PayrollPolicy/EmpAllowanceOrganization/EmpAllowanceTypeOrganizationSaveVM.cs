using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmpAllowanceOrganization
{
    public class EmpAllowanceTypeOrganizationSaveVM:BaseViewModel
    {
        public int EmployeeAllowanceTypeID { get; set; }

        [Required(ErrorMessage = "Enter AllowanceType Name")]
        public string? EmployeeAllowanceTypeName { get; set; }
        [Required(ErrorMessage ="Select Organization")]
        public int? OrganizationID { get; set; }
    }
}
