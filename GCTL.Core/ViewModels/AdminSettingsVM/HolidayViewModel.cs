using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AdminSettingsVM
{
    public class HolidayViewModel:BaseViewModel
    {
        public int? HolidayID { get; set; }

        [Required(ErrorMessage = "{0} is required!"), Display(Name = "Holiday Title")]
        public string? HolidayTitle { get; set; }

        public string? HolidayDescription { get; set; }

        [Required(ErrorMessage = "{0} is required!"), Display(Name = "Start Date")]
        public string? StartDate { get; set; }

        [Required(ErrorMessage = "{0} is required!"), Display(Name = "End Date")]
        public string? EndDate { get; set; }

        [Required(ErrorMessage = "{0} is required!"), Display(Name = "Total Days")]
        public int? TotalDays { get; set; }

        [Required(ErrorMessage = "{0} is required!"), Display(Name = "Status")]
        public int? StatusID { get; set; }
        
        public string? StatusName { get; set; }

        [Required(ErrorMessage = "{0} is required!"), Display(Name = "Organization")]
        public int? OrganizationID { get; set; }

        public string? OrganizationName { get; set; }

        public int? OrganizationBranchID { get; set; }
    }
}
