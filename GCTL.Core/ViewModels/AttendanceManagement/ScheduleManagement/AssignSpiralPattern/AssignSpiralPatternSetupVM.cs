using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignSpiralPattern
{
    public class AssignSpiralPatternSetupVM : BaseViewModel
    {
        [Required(ErrorMessage = "{0} is required!"), DisplayName("Organization")]
        public int OrganizationID { get; set; }

        public List<int>? DepartmentIDs { get; set; }

        public List<int>? EmployeeIDs { get; set; }

        [Required(ErrorMessage = "{0} is required!"), DisplayName("Spiral Pattern Type")]
        public int? SpiralPatternTypeID { get; set; }

        [Required(ErrorMessage = "{0} is required!"), DisplayName("Spiral Pattern")]
        public int SpiralPatternID { get; set; }

        [Required(ErrorMessage = "{0} is required!"), DisplayName("Start Date")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "{0} is required!"), DisplayName("End Date")]
        public DateTime? EndDate { get; set; }
    }
}
