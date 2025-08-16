using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignSpiralPattern
{
    public class AssignSpiralPatternEditVM : BaseViewModel
    {
        public int SpiralPatternAssignListID { get; set; }

        [Required(ErrorMessage = "{0} is required!"), DisplayName("Organization")]
        public int? EditOrganizationID { get; set; }

        public int? EditDepartmentID { get; set; }

        public int? EditEmployeeID { get; set; }

        [Required(ErrorMessage = "{0} is required!"), DisplayName("Spiral Pattern Type")]
        public int? EditSpiralPatternTypeID { get; set; }

        [Required(ErrorMessage = "{0} is required!"), DisplayName("Spiral Pattern")]
        public int? EditSpiralPatternID { get; set; }

        [Required(ErrorMessage = "{0} is required!"), DisplayName("Start Date")]
        public DateTime? EditStartDate { get; set; }

        [Required(ErrorMessage = "{0} is required!"), DisplayName("End Date")]
        public DateTime? EditEndDate { get; set; }
    }
}
