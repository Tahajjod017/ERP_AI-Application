using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AdminSettingsVM
{
    public class EmailSettingsViewModel : BaseViewModel
    {
        public int? EmailSettingID { get; set; }
        public int? OrganizationID { get; set; }
        public string? OrganizationName { get; set; }
        public string? ServerName { get; set; }
        public int PortNumber { get; set; }
        public bool IsSSLRequired { get; set; }
        public bool IsSMTPAuthenticationRequired { get; set; }
        public int? PriorityIndex { get; set; }
        public string? UserName { get; set; }
        public string? MailFrom { get; set; }
        public string? Password { get; set; }
        public bool IsActive { get; set; }
    }
}
