using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmpTransfer
{
    public class EmpOrganizationOrganizationBranchGetByIdVM
    {
        public int EmployeeID { get; set; }
        public int? FromOrganizationID { get; set; }
        public int? FromOrganizationBranchID { get; set; }
        public int? ToOrganizationID { get; set; }
        public int? ToOrganizationBranchID { get; set; }
    }
}
