using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.PaymentModes;

namespace GCTL_App.ViewModels.MasterSetup.PaymentModes
{
    public class PaymentModePageVM : BaseViewModel
    {
        public PaymentModeVM Setup { get; set; } = new PaymentModeVM();
        public List<PaymentModeVM> List { get; set; } = new List<PaymentModeVM>();
    }
}
