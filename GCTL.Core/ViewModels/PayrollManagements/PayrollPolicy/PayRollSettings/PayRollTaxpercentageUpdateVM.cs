using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollSettings
{
    public class PayRollTaxpercentageUpdateVM:BaseViewModel
    {
        public int PSettingID { get; set; }

        public int? OrganizationID { get; set; }

        public decimal? TaxPercentage { get; set; }
    }
}
