using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest
{
    public class LeaveApplicationsRequestVM:BaseViewModel
    {
        public int LeaveApplicationID { get; set; }
        [Required(ErrorMessage ="Select Employee")]
        public int? EmployeeID { get; set; }

        public bool IsFullDay { get; set; }

        [Required(ErrorMessage ="Required FromDate ")]
        public DateOnly? FromDate { get; set; }
        [Required(ErrorMessage = "Required ToDate ")]
        public DateOnly? ToDate { get; set; }

        public TimeOnly? PartialFromTime { get; set; }

        public TimeOnly? PartialToTime { get; set; }

        public int? StatusID { get; set; }
        [Required(ErrorMessage = "Select Leave Type")]

        public int? LeaveTypeID { get; set; }
      
        public string? Reason { get; set; }
    }
}
