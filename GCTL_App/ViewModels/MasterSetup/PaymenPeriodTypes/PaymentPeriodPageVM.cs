using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.PaymenPeriodTypes;

namespace GCTL_App.ViewModels.MasterSetup.PaymenPeriodTypes
{
    public class PaymentPeriodPageVM : BaseViewModel
    {
        public PaymentPeriodsVM Setup { get; set; } = new PaymentPeriodsVM();
        public List<PaymentPeriodsVM> List { get; set; } = new List<PaymentPeriodsVM>();
    }
}
