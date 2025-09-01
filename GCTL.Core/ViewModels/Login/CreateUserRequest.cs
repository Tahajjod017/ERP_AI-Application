using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Login
{
    public class CreateUserRequest
    {
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; } // Even if EmployeeName comes, not needed for user creation
    }
}
