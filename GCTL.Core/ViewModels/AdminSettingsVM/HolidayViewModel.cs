using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AdminSettingsVM
{
    public class HolidayViewModel:BaseViewModel
    {
        public int? HolidayID { get; set; }

        public string? HolidayTitle { get; set; }

        public string? HolidayDescription { get; set; }

        public string? StartDate { get; set; }

        public string? EndDate { get; set; }

        public int? TotalDays { get; set; }

        public int? StatusID { get; set; }
        public string? StatusName { get; set; }
        public int? OrganizationID { get; set; }

        public int? OrganizationBranchID { get; set; }
    }
}
