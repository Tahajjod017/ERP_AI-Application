using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AdminSettingsVM
{
    public class WeekendSettingVM:BaseViewModel
    {
        public int? WeekendSettingID { get; set; }
        public int? WeekendDayID { get; set; }
        public int? OrganizationID { get; set; }
        public int? OrganizationBranchID { get; set; }
        public List<int>? WeekendDays { get; set; }
        public string? WeekendTitle { get; set; }
        public string? OrganizationName { get; set; }
        public string? OrganizationBranchName { get; set; }
        public List<string>? WeekendDaysLabel { get; set; } = new();




    }
}
