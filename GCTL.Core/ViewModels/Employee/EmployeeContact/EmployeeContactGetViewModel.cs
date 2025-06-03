using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeContact
{
    public class EmployeeContactGetViewModel
    {
        public int EmployeeEmeContactID { get; set; }
        public int EmployeePersonalId { get; set; }
        public string ContactName { get; set; }
        public string ContactNumber { get; set; }
        public string ContactEmail { get; set; }
        public string PersonalEmail { get; set; }
        public string PersonalPhone { get; set; }
        public bool IsActive { get; set; }
        public string Relationship { get; set; }
    }
}
