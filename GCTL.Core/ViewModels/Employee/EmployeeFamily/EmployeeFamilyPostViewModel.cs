using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeFamily
{
    public class EmployeeFamilyPostViewModel : EmployeeBaseViewModel
    {
        public int EmployeeFamilyInfoID { get; set; }

       // public int? EmployeeID { get; set; }

        public string FullName { get; set; }

        public string RelationToEmployee { get; set; }

        public string Occupation { get; set; }

        public string ContactNumber { get; set; }

        public string Email { get; set; }

        public string Address { get; set; }
    }
}
