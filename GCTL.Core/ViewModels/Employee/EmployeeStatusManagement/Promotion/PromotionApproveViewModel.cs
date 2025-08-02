using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Promotion
{
    public class PromotionApproveViewModel
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string CurrentPosition { get; set; }
        public string ProposedPosition { get; set; }
        public string CurrentSalary { get; set; }
        public string ProposedSalary { get; set; }
        public string EffectiveDate { get; set; }
        public string YearsOfExperience { get; set; }
        public string Justification { get; set; }
        public string AvatarUrl { get; set; }
        public string Status { get; set; }
        public DateTime? EffectiveDateRaw { get; set; }
        public object CurrentSalaryNumeric { get; set; }
        public object ProposedSalaryNumeric { get; set; }
    }

  
}
