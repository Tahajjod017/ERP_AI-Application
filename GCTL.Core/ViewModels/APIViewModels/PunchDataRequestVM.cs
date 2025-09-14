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
        public string EmployeeId { get; set; }
        public string AttendanceDate { get; set; }
        public List<PunchDataVM> PunchDataVMs { get; set; } = new();
    }
}
