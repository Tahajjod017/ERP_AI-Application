using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.EmployeeShiftView
{
    public class PaginatedShiftResult
    {
        public IEnumerable<EmployeeShiftViewSetupVM> Items { get; set; }
        public int TotalItems { get; set; }
        public int StartItem { get; set; }
        public int EndItem { get; set; }
        public int PageNumber { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public List<HolidayVM>? Holidays { get; set; }
        public List<LeaveApplicationsVM>? LeaveApplications { get; set; }
    }
}
