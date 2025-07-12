using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeListVM
{
    public class EmployeeFilterModel
    {
        public string Department { get; set; }
        public string Status { get; set; }
        public string Sort { get; set; }
        public string Search { get; set; }
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }
        public string Company { get; set; }
    }
}
