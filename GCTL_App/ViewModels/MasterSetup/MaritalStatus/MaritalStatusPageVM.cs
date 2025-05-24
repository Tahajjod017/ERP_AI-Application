using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.MaritalStatus;

namespace GCTL_App.ViewModels.MasterSetup.MaritalStatus
{
    public class MaritalStatusPageVM : BaseViewModel
    {
        public MaritalStatusVM Setup { get; set; } = new MaritalStatusVM();
        public List<MaritalStatusVM> List { get; set; } = new List<MaritalStatusVM>();
    }
}
