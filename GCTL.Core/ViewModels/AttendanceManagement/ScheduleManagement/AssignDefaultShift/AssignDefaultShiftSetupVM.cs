using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignDefaultShift
{
    public class AssignDefaultShiftSetupVM : BaseViewModel
    {
        public int DefaultShiftID { get; set; }
                
        [Required(ErrorMessage = "Organization is required!")]
        public int? OrganizationID { get; set; }

        [Required(ErrorMessage = "Shift is required!")]
        public int? ShiftID { get; set; }

        public string? ShiftName { get; set; }

        //public List<int>? OrganizationIDs { get; set; }

        public string? OrganizationName { get; set; }

        public int? DepartmentID { get; set; }

        public List<int>? DepartmentIDs { get; set; }

        public string? DepartmentName { get; set; }

        public int? EmployeeID { get; set; }

        public List<int>? EmployeeIDs { get; set; }

        public string? EmployeeName { get; set; }

        public List<int>? ExcludedEmployeeIDs { get; set; }
    }
}
