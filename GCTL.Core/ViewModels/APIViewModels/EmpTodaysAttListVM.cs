using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.APIViewModels
{
    public class EmpTodaysAttListVM
    {
        public int? EmployeeID { get; set; }
        public string? EmployeeName { get; set; }
        public DateOnly? AttendanceDate { get; set; }
        public string? StatusName { get; set; }
        public string? ShiftName { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        public IList<AttendenceListVM> AttendenceListVMs { get; set; } = new List<AttendenceListVM>();
    }
}
