using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AdminSettingsVM
{
    public class LocalizationViewModel: BaseViewModel
    {
        public int LocalizationID { get; set; }

        public int OrganizationID { get; set; }

        public int LanguageID { get; set; }

        public int TimezoneID { get; set; }

        public int DateFormatID { get; set; }

        public int TimeFormatID { get; set; }

        public int CurrencyID { get; set; }

        public string CurrencySymbol { get; set; }

      
    }

}
