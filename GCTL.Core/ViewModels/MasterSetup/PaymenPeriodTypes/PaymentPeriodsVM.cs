using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.MasterSetup.PaymenPeriodTypes
{
    public class PaymentPeriodsVM : BaseViewModel
    {
        public int PaymentPeriodTypeID { get; set; }

        public string PaymentPeriodTypeName { get; set; }
    }
}
