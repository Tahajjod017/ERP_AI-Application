using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.EmployeeShiftView
{
    public class HolidayVM
    {
        public int? OrganizationID { get; set; }
        public int? OrganizationBranchID { get; set; }
        public string? HolidayTitle { get; set; }
        public string? HolidayDescription { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? TotalDays { get; set; }
    }
}
