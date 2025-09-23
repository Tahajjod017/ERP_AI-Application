using GCTL.Core.ViewModels.MasterSetup.Organizations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.Shift
{
    public class ShiftUpdateSetupVM : BaseViewModel
    {
        public int UpdateShiftID { get; set; }

        [Required(ErrorMessage = "{0} is required."), DisplayName("Shift Name")]
        public string UpdateShiftName { get; set; }

        public int? UpdateOrganizationID { get; set; }

        public string? UpdateOrganizationName { get; set; }

        public TimeOnly? UpdateStartTime { get; set; }

        public TimeOnly? UpdateEndTime { get; set; }

        public bool UpdateIsLateCount { get; set; }

        public bool UpdateIsAutomaticORManualBreakTime { get; set; }

        public bool UpdateIsMBCompulsaryOrComplementaryDeductWithShift { get; set; }

        public bool UpdateIsAllowStartAndEndTime { get; set; }

        public string? UpdateMealBreakStartTime { get; set; }

        public string? UpdateMealBreakEndTime { get; set; }

        public bool UpdateIsAllowOvertime { get; set; }

        public int? UpdateGraceTime { get; set; }

        public int? UpdateMinimumWorkingTime { get; set; }

        public int? UpdateMinimumRequiredOvertime { get; set; }

        public int? UpdateMaximumAllowedOvertime { get; set; }

        public int? UpdateMealBreakTime { get; set; }

        public bool UpdateIsFlexibleInTime { get; set; }

        public int? UpdateEarlyInTimeHour { get; set; }

        public int? UpdateEarlyInTimeMinute { get; set; }

        public bool UpdateIsFlexibleOutTime { get; set; }

        public int? UpdateEarlyOutTimeHour { get; set; }

        public int? UpdateEarlyOutTimeMinute { get; set; }


        public IList<OrganizationsVM>? OrganizationsVMs { get; set; } = new List<OrganizationsVM>();
    }
}
