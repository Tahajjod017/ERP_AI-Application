using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.Country;

namespace GCTL_App.ViewModels.MasterSetup.Country
{
    public class CountryPageVM : BaseViewModel
    {
        public CountryVM Setup { get; set; } = new CountryVM();
        public List<CountryVM> List { get; set; } = new List<CountryVM>();
    }
}
