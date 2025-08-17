using GCTL.Core.ViewModels.Employee.EmployeeResign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.CRM
{
    public class LeadsTableVM
    {
        public int LeadId { get; set; }
        public string? LeadName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? ContactName { get; set; }
        public string? CompanyName { get; set; }
        public string? Status { get; set; }
    }

    public class LeadListViewModel
    {
        public List<LeadsTableVM> Leads { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchTerm { get; set; }
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }
    }
}
