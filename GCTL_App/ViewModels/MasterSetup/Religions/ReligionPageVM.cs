using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.Religions;

namespace GCTL_App.ViewModels.MasterSetup.Religions
{
    public class ReligionPageVM : BaseViewModel
    {
        public ReligionVM Setup { get; set; } = new ReligionVM();
        public List<ReligionVM> List { get; set; } = new List<ReligionVM>();
    }
}
