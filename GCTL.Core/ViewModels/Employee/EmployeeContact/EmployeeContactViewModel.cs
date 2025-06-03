using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeContact
{
    public class EmployeeContactViewModel : EmployeeBaseViewModel
    {
        public int EmployeeEmeContactID { get; set; }

       // public int? EmployeeID { get; set; }

        public string ContactName { get; set; }

        public string Relationship { get; set; }

        public string ContactNumber { get; set; }

        public string ContactEmail { get; set; }
    }
}
