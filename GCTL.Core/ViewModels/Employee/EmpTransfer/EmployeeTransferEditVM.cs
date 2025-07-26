using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmpTransfer
{
    public class EmployeeTransferEditVM:BaseViewModel
    {
        public int EmployeeTransferID { get; set; }
        public int? FromOrganizationIDEdit { get; set; }
        public int? FromOrganizationBranchIDEdit { get; set; }
        public int? ToOrganizationIDEdit { get; set; }
        public int? ToOrganizationBranchIDEdit { get; set; }
        public DateTime? TransferDateEdit { get; set; }
        public int? EmployeeIDEdit { get; set; }
        public string? TransferNoteEdit { get; set; }
       

    }
}
