using GCTL.Core.ViewModels.MasterSetup.Organizations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.Shift
{
    public class ShiftsSetupVM : BaseViewModel
    {
        public int ShiftID { get; set; }

        [Required(ErrorMessage = "{0} is required."), DisplayName("Shift Name")]
        public string ShiftName { get; set; }

        public int? OrganizationID { get; set; }

        public string? OrganizationName { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        public bool IsLateCount { get; set; }

        public bool IsAutomaticORManualBreakTime { get; set; }

        public bool IsMealBreakCompulsaryOrComplementaryDeductWithShift { get; set; }

        public bool IsAllowStartAndEndTime { get; set; }

        public TimeOnly? MealBreakStartTime { get; set; }

        public TimeOnly? MealBreakEndTime { get; set; }

        public bool IsAllowOvertime { get; set; }

        public TimeOnly? GraceTime { get; set; }

        public TimeOnly? MinimumWorkingTime { get; set; }

        public TimeOnly? MinimumRequiredOvertime { get; set; }

        public TimeOnly? MaximumAllowedOvertime { get; set; }

        public TimeOnly? MealBreakTime { get; set; }


        public IList<OrganizationsVM>? OrganizationsVMs { get; set; } = new List<OrganizationsVM>();
    }
}
