using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Increment
{
    public class SalaryChangeViewModel : BaseViewModel
    {
        public int EmployeeId { get; set; }
        public int OrganizationId { get; set; }
        public int DesignationId { get; set; }
        public int DepartmentId { get; set; }

        public string ChangeType { get; set; } // "increment" or "decrement"
        public DateTime EffectiveDate { get; set; }

        public decimal CurrentSalary { get; set; }
        public decimal NewSalary { get; set; }

        public string Remarks { get; set; }
    }

}
