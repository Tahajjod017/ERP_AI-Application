using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Increment
{
    public class IncrementListItem
    {
        public int EmployeeCareerChangeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DepartmentName { get; set; }
        public int DepartmentID { get; set; }
        public decimal? CurrentSalary { get; set; }
        public decimal? NewSalary { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public int EmployeeActionTypeID { get; set; }
        public string StatusName { get; set; }
        public string EmployeeImageFileName { get; set; }
    }
}
