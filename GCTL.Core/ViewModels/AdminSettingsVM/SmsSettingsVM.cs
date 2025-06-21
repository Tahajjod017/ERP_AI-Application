using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AdminSettingsVM
{
    public class SmsSettingsVM:BaseViewModel
    {
        public int SMSSettingID { get; set; }

        public int? OrganizationID { get; set; }
        public string? OrganizationName { get; set; }

        public string? ServerName { get; set; }

        public string? UserName { get; set; }

        public string? Password { get; set; }

        public string? Gateway { get; set; }

        public string? API { get; set; }

        public string? MobileNumber { get; set; }

        public int? PriorityIndex { get; set; }

        public bool IsActive { get; set; }
    }
}
