using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.LeadStatuses;

namespace GCTL_App.ViewModels.MasterSetup.LeadStatuses
{
    public class LeadStatusPageVM : BaseViewModel
    {
        public LeadStatusVM Setup { get; set; } = new LeadStatusVM();
        public List<LeadStatusVM> List { get; set; } = new List<LeadStatusVM>();
    }
}