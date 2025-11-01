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
    public class GetByIdShiftVM
    {
        public int? UpdateShiftID { get; set; }

        public string? UpdateShiftName { get; set; }

        public int? UpdateOrganizationID { get; set; }

        public string? UpdateOrganizationName { get; set; }

        public string? UpdateStartTime { get; set; }

        public string? UpdateEndTime { get; set; }

        public bool UpdateIsLateCount { get; set; }

        public bool UpdateIsAutomaticORManualBreakTime { get; set; }

        public bool UpdateIsMBCompulsaryOrComplementaryDeductWithShift { get; set; }

        public bool UpdateIsAllowStartAndEndTime { get; set; }

        public string? UpdateMealBreakStartTime { get; set; }

        public string? UpdateMealBreakEndTime { get; set; }

        public bool UpdateIsAllowOvertime { get; set; }

        public int? UpdateGraceTimeHour { get; set; }

        public int? UpdateGraceTimeMinute { get; set; }

        public int? UpdateMinimumWorkingTimeHour { get; set; }

        public int? UpdateMinimumWorkingTimeMinute { get; set; }

        public int? UpdateMinimumRequiredOvertimeHour { get; set; }

        public int? UpdateMinimumRequiredOvertimeMinute { get; set; }

        public int? UpdateMaximumAllowedOvertimeHour { get; set; }

        public int? UpdateMaximumAllowedOvertimeMinute { get; set; }

        public int? UpdateMealBreakTimeHour { get; set; }

        public int? UpdateMealBreakTimeMinute { get; set; }

        public bool UpdateIsRestrictFlexibleInTime { get; set; }

        public int? UpdateEarlyInTimeHour { get; set; }

        public int? UpdateEarlyInTimeMinute { get; set; }

        public bool UpdateIsRestrictFlexibleOutTime { get; set; }

        public int? UpdateEarlyOutTimeHour { get; set; }

        public int? UpdateEarlyOutTimeMinute { get; set; }


        public IList<OrganizationsVM>? OrganizationsVMs { get; set; } = new List<OrganizationsVM>();
    }
}
