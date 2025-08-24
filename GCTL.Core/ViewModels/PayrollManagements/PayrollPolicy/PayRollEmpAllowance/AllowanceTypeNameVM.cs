using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpAllowance
{
    public class AllowanceTypeNameVM
    {
        public int EmployeeAllowanceTypeID { get; set; }
        public string? EmployeeAllowanceTypeName { get; set; }
        public decimal? Value { get; set; }
        public bool IsActive { get; set; }
    }
}
