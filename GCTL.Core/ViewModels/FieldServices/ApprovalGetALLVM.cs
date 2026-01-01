using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.FieldServices
{
  public  class ApprovalGetALLVM
    {
        public int EmployeeAdvanceID { get; set; }

        public int? CustomerID2 { get; set; }

        public int? JobID { get; set; }

        public string? JobTitle { get; set; }

        public List<int>? RequestedByUserID { get; set; } // Job Type

        public decimal? AmountRequested { get; set; }

        public DateTime? StartDate { get; set; }


       
        public List<int>? GroupEmployeeID { get; set; }
        public List<string>? GroupEmployeeName { get; set; }

        public int? ApprovalStatusID { get; set; }
        public string? StatusName { get; set; } //nEW added

        public string? CustomerName { get; set; } //New Added

        public string? JobTypeName { get; set; } //New Added

        public string? RequestedByUser { get; set; } //New Added

    }
}
