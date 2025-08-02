using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.LeadSource;

namespace GCTL_App.ViewModels.MasterSetup.LeadSource
{
    public class LeadSourcePageVM : BaseViewModel
    {
        public LeadSourceVM Setup { get; set; } = new LeadSourceVM();
        public List<LeadSourceVM> List { get; set; } = new List<LeadSourceVM>();
    }
}