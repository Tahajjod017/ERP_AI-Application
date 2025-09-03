using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.EmployeeShiftView
{
    public class EmployeeShiftViewSetupVM : BaseViewModel
    {
        public int? OrganizationID { get; set; }
        public List<int>? DepartmentIDs { get; set; }
        public List<int>? EmployeeIDs { get; set; }
    }
}
