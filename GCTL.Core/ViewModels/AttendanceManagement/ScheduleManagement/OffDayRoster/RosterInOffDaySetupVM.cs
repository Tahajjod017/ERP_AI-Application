using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OffDayRoster
{
    public class RosterInOffDaySetupVM : BaseViewModel
    {
        public int RosterInHolyDayID { get; set; }

        [Required(ErrorMessage = "Organization is required!")]
        public int? OrganizationID { get; set; }
        public string? OrganizationName { get; set; }

        public int? BranchID { get; set; }
        public List<int>? BranchIDs { get; set; }
        public string? BranchName { get; set; }

        public int? DepartmentID { get; set; }
        public List<int>? DepartmentIDs { get; set; }
        public string? DepartmentName { get; set; }

        public int? EmployeeID { get; set; }
        public List<int>? EmployeeIDs { get; set; }
        public string? EmployeeName { get; set; }

        [Required(ErrorMessage = "Shift is required!")]
        public int? ShiftID { get; set; }
        public string? ShiftName { get; set; }

        [Required(ErrorMessage = "Date is required!")]
        public List<DateTime>? DayDate { get; set; }

        [Required(ErrorMessage = "Compensation Type is required!")]
        public int? CompensationTypeID { get; set; }

        public List<DateTime>? ExchangeDate { get; set; }
    }
}
