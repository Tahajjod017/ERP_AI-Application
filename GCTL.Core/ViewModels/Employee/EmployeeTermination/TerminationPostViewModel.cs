using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeTermination
{
    public class TerminationPostViewModel : BaseViewModel
    {
        public int EmployeeId { get; set; }
        public int TerminationTypeId { get; set; }
        public string NoticeDate { get; set; }
        public string ResignationDate { get; set; }
        public string Reason { get; set; }
    }
}
