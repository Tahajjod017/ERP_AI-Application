using GCTL.Core.ViewModels.MasterSetup.LeadActivityType;

namespace GCTL_App.ViewModels.MasterSetup.LeadActivityType
{
    public class LeadActivityTypePageVM
    {
        public LeadActivityTypeVM Setup { get; set; } = new LeadActivityTypeVM();
        public List<LeadActivityTypeVM> List { get; set; } = new List<LeadActivityTypeVM>();
    }
}
