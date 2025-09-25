using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels
{
    public class UpcomingActivityVM :BaseViewModel
    {
        public int? PageNumber { get; set; }
        public int? ItemPerPage { get; set; }
        public string? Search { get; set; }
        public string? SortColumn { get; set; }
        public string? SortDirection { get; set; }
        public string? DateRange { get; set; }
        public int? CustomerTypeID { get; set; }
        public string? LeadStatusID { get; set; }
        public int? ActivityTypeID { get; set; }
    }
}