using GCTL.Core.ViewModels.MasterSetup.LeadSource;
using GCTL.Core.ViewModels.MasterSetup.Priority;

namespace GCTL_App.ViewModels.MasterSetup.Priority
{
    public class PriorityPageVM
    {
        public PriorityVM Setup { get; set; } = new PriorityVM();
        public List<PriorityVM> List { get; set; } = new List<PriorityVM>();
    }
}
