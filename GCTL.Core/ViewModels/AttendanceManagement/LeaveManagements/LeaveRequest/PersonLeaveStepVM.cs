using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest
{
    public class PersonLeaveStepVM
    {
        public int LeaveApplicationId { get; set; }

        public int? ApproverStep {  get; set; }

        public string? ApprovarNote { get; set; }
        public string? ApprovarPerson { get; set; }
        public string? StatusName { get; set; }

    }
}
