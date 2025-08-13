using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmpTransfer
{
    public class EmployeeTransferApproveOrDecEditVM:BaseViewModel
    {
        public int EmployeeTransferID { get; set; }
        public int? FromOrganizationIDEdit { get; set; }
        public int? FromOrganizationBranchIDEdit { get; set; }
        public int? ToOrganizationIDEdit { get; set; }
        public int? ToOrganizationBranchIDEdit { get; set; }
        public DateTime? TransferDateEdit { get; set; }
        public int? EmployeeIDEdit { get; set; }
        public string? TransferNoteEdit { get; set; }
        public string ? TransferBaseHistoryNoteEdit {  get; set; }
        public int? FromDepartmentIDEdit { get; set; }

        public int? FromDesignationIDEdit { get; set; }

        public int? ToDepartmentIDEdit { get; set; }

        public int? ToDesignationIDEdit { get; set; }

        public string? TransferTypeEdit {get; set; }
        public bool Approved { get; set; }
        public bool Declined { get; set; }
    }
}
