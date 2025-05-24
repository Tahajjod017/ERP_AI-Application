using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeOfficial
{
    public class EmployeeOfficialPostViewModel : BaseViewModel
    {
        public int EmployeePersonalId { get; set; }
        public string PersonalPhone { get; set; }
        public string PersonalEmail { get; set; }
    }
}
