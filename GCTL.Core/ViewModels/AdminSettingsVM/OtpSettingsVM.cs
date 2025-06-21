using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AdminSettingsVM
{
    public class OtpSettingsVM :BaseViewModel
    {
        public int? OTPSettingID { get; set; }

        public int? OrganizationID { get; set; }
        public string? OrganizationName { get; set; }

        public string? OTPType { get; set; }

        public int? OTPDigitLimit { get; set; }

        public string? OTPExpireInMin { get; set; }
    }
}
