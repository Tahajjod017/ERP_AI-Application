using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeResign
{
    public class ResignationGetViewModel 
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string Reason { get; set; }
        public string NoticeDate { get; set; }
        public string LastWorkingDay { get; set; }
        public string ProcessedDate { get; set; }
        public string Status { get; set; }
        public string ProfileImage { get; set; }
        public string EmployeeId { get; set; }
        public string YearsOfService { get; set; }
        public string NoticePeriod { get; set; }
        public string CurrentSalary { get; set; }
        public string PendingDues { get; set; }
        public string HandoverStatus { get; set; }
        public bool AssetReturned { get; set; }
        public bool ClearanceCompleted { get; set; }
        public bool DocumentsPrepared { get; set; }
    }

    public class ResignationListViewModel
    {
        public List<ResignationGetViewModel> Resignations { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchTerm { get; set; }
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }
    }

}
