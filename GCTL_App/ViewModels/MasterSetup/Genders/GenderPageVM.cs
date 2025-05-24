using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.Genders;

namespace GCTL_App.ViewModels.MasterSetup.Genders
{
    public class GenderPageVM : BaseViewModel
    {
        public GenderVM Setup { get; set; } = new GenderVM();
        public List<GenderVM> List { get; set; } = new List<GenderVM>();
    }
}
