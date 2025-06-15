using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveSettings
{
    public class GetLeaveTypesByIdVM
    {
        public int LeaveTypeID { get; set; }
        public string? LeaveTypeName { get; set; }
        public int? OrganizationID { get; set; }

        public bool IsPaid { get; set; }

        public decimal? LeaveDays { get; set; }
        public string? Code { get; set; }
        public int? EffectiveFrom { get; set; }

        public string? EffectiveFromMonthYear { get; set; }

        public string? EffectiveAfter { get; set; }

        public int? MinimumDaysRequiredEncashement { get; set; }

        public int? MaximumDaysAllowedEncashement { get; set; }
    }
}
