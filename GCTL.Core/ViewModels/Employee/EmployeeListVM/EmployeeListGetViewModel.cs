using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeListVM
{
    public class EmployeeListGetViewModel
    {
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Department { get; set; }
        public DateOnly? JoiningDate { get; set; }
        public string Status { get; set; }
        public string? Avatar { get; set; }
        public int? CompanyId { get; set; }
        public int DepartmentId { get; set; }
    }
}
