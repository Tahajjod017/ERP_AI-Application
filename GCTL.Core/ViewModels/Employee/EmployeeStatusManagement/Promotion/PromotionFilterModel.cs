using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Promotion
{
    public class PromotionFilterModel
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string PromotionType { get; set; }
        public string Status { get; set; }
        public string SortBy { get; set; }
        public string DateRange { get; set; }
        public string Department { get; set; }
        public string Employee { get; set; }

        public string SortColumn { get; set; }
        public string SortDirection { get; set; }
        public string SearchInput { get; set; }
    }

    
}
