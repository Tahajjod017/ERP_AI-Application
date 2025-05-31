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

        public string? UpdateStartTime { get; set; }

        public string? UpdateEndTime { get; set; }

        public bool UpdateIsLateCount { get; set; }

        public bool UpdateIsAutomaticORManualBreakTime { get; set; }

        public bool UpdateIsMBCompulsaryOrComplementaryDeductWithShift { get; set; }

        public bool UpdateIsAllowStartAndEndTime { get; set; }

        public string? UpdateMealBreakStartTime { get; set; }

        public string? UpdateMealBreakEndTime { get; set; }

        public bool UpdateIsAllowOvertime { get; set; }

        public TimeOnly? UpdateGraceTime { get; set; }

        public TimeOnly? UpdateMinimumWorkingTime { get; set; }

        public TimeOnly? UpdateMinimumRequiredOvertime { get; set; }

        public TimeOnly? UpdateMaximumAllowedOvertime { get; set; }

        public TimeOnly? UpdateMealBreakTime { get; set; }


        public IList<OrganizationsVM>? OrganizationsVMs { get; set; } = new List<OrganizationsVM>();
    }
}
