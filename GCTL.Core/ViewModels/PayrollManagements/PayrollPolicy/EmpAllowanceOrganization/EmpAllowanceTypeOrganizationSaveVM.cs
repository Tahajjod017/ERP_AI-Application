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

        public string? EmployeeAllowanceTypeName { get; set; }
         public int? OrganizationID { get; set; }
        public List<int>? OrganizationIDs { get; set; } = new List<int>();
        public bool ApplyOnBasicSalary { get; set; }
        public bool ApplyOnGrossSalary { get; set; }
    }
}
