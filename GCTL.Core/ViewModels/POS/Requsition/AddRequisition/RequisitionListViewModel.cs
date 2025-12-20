using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Requsition.AddRequisition
{
    public class RequisitionListViewModel
    {
        public string SearchTerm { get; set; }
        public int? ProjectFilter { get; set; }
        public int? ProductTypeFilter { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        public List<RequisitionItemViewModel> Requisitions { get; set; } = new List<RequisitionItemViewModel>();
    }
}
