using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Promotion
{
    public class PromotionListFilterViewModel
    {
        public string SearchTerm { get; set; }
        public int? DepartmentId { get; set; }
        public string Status { get; set; }
        public string DateRange { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }
    }

}
