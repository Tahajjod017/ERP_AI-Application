using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Settings
{
    public class ApprovalSettingViewModel : BaseViewModel
    {
        public int ApprovalSettingID { get; set; }
        public int OrganizationID { get; set; }
        public int OrganizationBranchID { get; set; }
        public int ApprovalTypeID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool AllowSelfApproval { get; set; }
        public int? SelfExceptionApprovalID { get; set; }
        public List<ApprovalLevelAssignmentViewModel> ApprovalLevels { get; set; } = new List<ApprovalLevelAssignmentViewModel>();
    }

    public class ApprovalLevelAssignmentViewModel
    {
        public int? LevelNumber { get; set; }
        public int ApproverEmployeeID { get; set; }
        public bool? IsEnabled { get; set; } = true;
    }
}
