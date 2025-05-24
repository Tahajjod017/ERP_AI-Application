using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.MasterSetup.PaymentModes
{
    public class PaymentModeVM : BaseViewModel
    {
        public int PaymentModeID { get; set; }

        public string PaymentModeName { get; set; }
    }
}
