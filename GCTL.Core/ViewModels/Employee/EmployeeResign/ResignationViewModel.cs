using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeResign
{
    public class ResignationViewModel 
    {
        public int ResigId { get; set; }
        public string REmpName { get; set; }
        public string REmpDept { get; set; }
        public string ResignResons { get; set; }
        public string ResNoticeDate { get; set; }
        //public DateTime? ResNoticeDateDT { get; set; }
        //public DateTime? ResinDateDT { get; set; }
        public string ResinDate { get; set; }
        public int? EmployeeId { get; set; }
        public int? CompanyId { get; set; }
        public string Image { get; set; }
        public string REmpCode { get; set; }
        public DateTime? ResinDateRaw { get; set; }
        public DateTime? ResNoticeDateRaw { get; set; }
    }
}
