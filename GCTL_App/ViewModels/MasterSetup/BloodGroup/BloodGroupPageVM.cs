using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.BloodGroup;

namespace GCTL_App.ViewModels.MasterSetup.BloodGroup
{
    public class BloodGroupPageVM : BaseViewModel
    {
        public BloodGroupVM Setup { get; set; } = new BloodGroupVM();
        public List<BloodGroupVM> List { get; set; } = new List<BloodGroupVM>();
    }
}
