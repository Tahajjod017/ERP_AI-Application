using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.Statuses;

namespace GCTL_App.ViewModels.MasterSetup.Statuses
{
    public class StatusPageVM : BaseViewModel
    {
        public StatusVM Setup { get; set; } = new StatusVM();
        public List<StatusVM> List { get; set; } = new List<StatusVM>();
    }
}
