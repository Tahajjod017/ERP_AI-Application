using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignSpiralPattern
{
    public class AssignSpiralPatternListVM : BaseViewModel
    {
        public int SpiralPatternAssignListID { get; set; }

        public int? OrganizationID { get; set; }

        public string? OrganizationName { get; set; }
        
        public int? EmployeeID { get; set; }

        public string? EmployeeName { get; set; }

        public int? SpiralPatternTypeID { get; set; }

        public string? SpiralPatternTypeName { get; set; }

        public int? SpiralPatternID { get; set; }

        public string? SpiralPatternName { get; set; }

        public string? StartDate { get; set; }

        public string? EndDate { get; set; }
    }
}
