using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.FieldServices
{
    public class EmployeeAdvancedVM : BaseViewModel
    {
        public int EmployeeAdvanceID { get; set; }

        [Required(ErrorMessage = "{0} is required"), Display(Name = "Client Name")]
        public int? CustomerID2 { get; set; }

        [Required(ErrorMessage = "{0} is required"), Display(Name = "Job Name")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid job")]
        public int? JobID { get; set; }

        public string? JobTitle { get; set; }
      
        [Required(ErrorMessage = "{0} is required"), Display(Name = "Job Type")]
        [JsonIgnore]
        public List<int>? RequestedByUserID { get; set; } // Job Type

        [Required(ErrorMessage = "{0} is required"), Display(Name = "Amount")]
        public decimal? AmountRequested { get; set; }

        public DateTime? StartDate { get; set; }

        
        public DateTime? EndDate { get; set; }

        public List<int>? GroupEmployeeID { get; set; }

        public List<string>? GroupEmployeeName { get; set; }

        public int? EmployeeID { get; set; }

        public bool? IsGroupAdvance { get; set; }

        public int? ApprovalStatusID { get; set; }

        [Required(ErrorMessage = "{0} is required"), Display(Name = "Approved By User ID")]
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
