using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels
{
    public class UpcomingActivityVM :BaseViewModel
    {
        public int? pageNumber { get; set; }
        public int? itemPerPage { get; set; }
        public string? search { get; set; }
        public string? sortColumn { get; set; }
        public string? sortDirection { get; set; }
        public string? dateRange { get; set; }
    }
}