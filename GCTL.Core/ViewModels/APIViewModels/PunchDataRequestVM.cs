using GCTL.Core.ViewModels.AttendanceManagement.ManualAttendence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.APIViewModels
{
    public class PunchDataRequestVM : BaseViewModel
    {
        public int EmployeeId { get; set; }

        public string? SourceType { get; set; } = "Apps";

        public string? DeviceInfo { get; set; } = string.Empty;

        public DateTime? CheckInTime { get; set; } = DateTime.Now;

        public string? Latitude { get; set; }

        public string? Longitude { get; set; }
    }
}
