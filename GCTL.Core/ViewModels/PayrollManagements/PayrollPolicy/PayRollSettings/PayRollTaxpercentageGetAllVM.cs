using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollSettings
{
    public class PayRollTaxpercentageGetAllVM
    {
        public int PSettingID { get; set; }

        public string? OrganizationName { get; set; }

        public decimal? TaxPercentage { get; set; }
        public byte? SalaryDay { get; set; }
    }
}
