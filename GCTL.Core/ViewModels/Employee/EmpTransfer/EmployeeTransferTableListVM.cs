using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmpTransfer
{
    public class EmployeeTransferTableListVM
    {
        public int ? EmployeeTransferID { get; set; }
        public string? EmployeeName { get; set; }
        public string ? FromOrganizationName { get; set; }
        public string? TorganizationName { get; set; }
        public string? FromOrganizationBranchName { get; set; }
         public string? ToOrganizationBranchName { get; set; }
        public string? TransferDate { get;  set; }
        public string? EmployeeDepartment { get; set; }
        public object EmployeeImage { get; set; }
    }
}
