using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Login
{
    public class UserListVM
    {
        public string Id { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } // You can later load real role here
        public string Status { get; set; }
        public string IsActive { get; set; }
        public string DefaultPass { get; set; }
    }

}
