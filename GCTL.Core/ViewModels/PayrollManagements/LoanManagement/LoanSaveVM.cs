using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.LoanManagement
{
    public class LoanSaveVM:BaseViewModel
    {
        public int LoanID { get; set; }
        [Required(ErrorMessage = "Employee is Required.")]
        [Display(Name = "Employee Name")]
        public List<int>? EmployeeIDs { get; set; }

        [Required(ErrorMessage = "Loan amount is required.")]
        [Range(500, 10000000, ErrorMessage = "Loan amount must be between 500 and 10,000,000.")]
        [Display(Name = "Loan Amount")]
        public decimal? LoanAmount { get; set; }

        [Required(ErrorMessage = "Installment Period is required.")]
        [Display(Name = "Installment Period")]
        public int? LoanInstallmentPeriodID { get; set; }

        [Required(ErrorMessage = "Issue Date is required.")]
        [Display(Name = "Issue Date")]
        public DateTime? IssueDate { get; set; }

        [Required(ErrorMessage = "Installment Start Date is required.")]
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }
        public int? ApprovalPersonID { get; set; }
        public bool? IsFinalApproved { get; set; }
        public bool? IsDecline { get; set; }
        public int? ApprovalStage { get; set; }
        public int? OrganizationID { get; set; }
        public List<int>? DepartmentIDs { get; set; }

    }
}
