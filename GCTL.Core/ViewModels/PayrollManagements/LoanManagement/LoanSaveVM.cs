using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.LoanManagement
{
    public class LoanSaveVM:BaseViewModel
    {
        public int LoanID { get; set; }
        public int? EmployeeID { get; set; }
        public decimal? LoanAmount { get; set; }
        public int? LoanInstallmentPeriodID { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? StartDate { get; set; }
        public int? ApprovalPersonID { get; set; }
        public bool? IsFinalApproved { get; set; }
        public bool? IsDecline { get; set; }
        public int? ApprovalStage { get; set; }
        public int? OrganizationIDs { get; set; }
        public int? DepartmentID { get; set; }

    }
}
