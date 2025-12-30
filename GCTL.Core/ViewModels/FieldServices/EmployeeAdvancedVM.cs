using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.FieldServices
{
    public class EmployeeAdvancedVM : BaseViewModel
    {
        public int EmployeeAdvanceID { get; set; }

        [Required(ErrorMessage = "Client Name is required")]
        public int? CustomerID2 { get; set; }

        [Required(ErrorMessage = "Job is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid job")]
        public int? JobID { get; set; }

        public string? JobTitle { get; set; }

        [Required(ErrorMessage = "Requested By is required")]
        public List<int>? RequestedByUserID { get; set; }

        [Required(ErrorMessage = "Ammount is required")]
        public decimal AmountRequested { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        public DateTime? EndDate { get; set; }

        public List<int>? GroupEmployeeID { get; set; }

        public List<string>? GroupEmployeeName { get; set; }

        public int? EmployeeID { get; set; }

        public bool? IsGroupAdvance { get; set; }

        public int? ApprovalStatusID { get; set; }

        public int? ApprovedByUserID { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public int? ApprovalSettingID { get; set; }

        public string? StatusName { get; set; } //nEW added

        public string? CustomerName { get; set; } //New Added

        public string? JobTypeName { get; set; } //New Added

        public string? RequestedByUser { get; set; } //New Added

        public string? JobName { get; set; } //New Added







    }
}
