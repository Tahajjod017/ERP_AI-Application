using GCTL.Core.ViewModels.MasterSetup.Organizations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.Shift
{
    public class ShiftsListVM : BaseViewModel
    {
        public int ShiftID { get; set; }

        public string ShiftName { get; set; }

        public int? OrganizationID { get; set; }

        public string? OrganizationName { get; set; }

        public string? StartTime { get; set; }

        public string? EndTime { get; set; }

        public bool IsLateCount { get; set; }

        public bool IsAutomaticORManualBreakTime { get; set; }

        public bool IsMealBreakCompulsaryOrComplementaryDeductWithShift { get; set; }

        public bool IsAllowStartAndEndTime { get; set; }

        public TimeOnly? MealBreakStartTime { get; set; }

        public TimeOnly? MealBreakEndTime { get; set; }

        public bool IsAllowOvertime { get; set; }

        public int? GraceTimeHour { get; set; }

        public int? GraceTimeMinute { get; set; }

        public int? MinimumWorkingTimeHour { get; set; }

        public int? MinimumWorkingTimeMinute { get; set; }

        public int? MinimumRequiredOvertimeHour { get; set; }

        public int? MinimumRequiredOvertimeMinute { get; set; }

        public int? MaximumAllowedOvertimeHour { get; set; }

        public int? MaximumAllowedOvertimeMinute { get; set; }

        public int? MealBreakTimeHour { get; set; }

        public int? MealBreakTimeMinute { get; set; }

        public bool IsFlexibleInTime { get; set; }

        public int? EarlyInTimeHour { get; set; }

        public int? EarlyInTimeMinute { get; set; }

        public bool IsFlexibleOutTime { get; set; }

        public int? EarlyOutTimeHour { get; set; }

        public int? EarlyOutTimeMinute { get; set; }


        public IList<OrganizationsVM>? OrganizationsVMs { get; set; } = new List<OrganizationsVM>();
    }
}
