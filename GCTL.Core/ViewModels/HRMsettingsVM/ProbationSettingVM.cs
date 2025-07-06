using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.HRMsettingsVM
{
    public class ProbationSettingVM :BaseViewModel
    {
        public int? ProbetionPeriodSettingID { get; set; }

        public int? OrganizationID { get; set; }
        public string? OrganizationName { get; set; }

        public int? Period { get; set; }

        public string? PeriodType { get; set; }
    }
}
