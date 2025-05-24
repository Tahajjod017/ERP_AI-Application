using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.Currencies;

namespace GCTL_App.ViewModels.MasterSetup.Currencies
{
    public class CurrencyPageVM : BaseViewModel
    {
        public CurrencyVM Setup { get; set; } = new CurrencyVM();
        public List<CurrencyVM> List { get; set; } = new List<CurrencyVM>();
    }
}
