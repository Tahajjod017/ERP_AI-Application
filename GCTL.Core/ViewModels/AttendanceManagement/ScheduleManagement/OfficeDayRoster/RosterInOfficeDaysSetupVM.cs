using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OfficeDayRoster
{
    public class RosterInOfficeDaysSetupVM : BaseViewModel
    {
        public int RosterInOfficeDayID { get; set; }

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

        [Required(ErrorMessage = "Start date is required!")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "End date is required!")]
        public DateTime? EndDate { get; set; }

        public string? TimeRange { get; set; }

        public List<int>? ExcludedEmployeeIDs { get; set; }

        public virtual ICollection<RosterInOfficeDaysOverrideSetupVM>? RosterInOfficeDaysOverrideSetupVMs { get; set; } = new List<RosterInOfficeDaysOverrideSetupVM>();
    }


    public class RosterDelVM : BaseViewModel
    {
        public int? Id { get; set; }
        public DateTime? OverrideDate { get; set; }
    }
}
