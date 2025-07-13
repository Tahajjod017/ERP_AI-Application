using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeReport
{
    public class CompanyData
    {
        public CompanyInfo Company { get; set; }
        public List<DepartmentGroup> DepartmentGroups { get; set; }
    }

    public class CompanyInfo
    {
        public string OrganizationName { get; set; }
        public string Address { get; set; }
    }

    public class DepartmentGroup
    {
        public string Department { get; set; }
        public List<EmployeeInfo> Employees { get; set; }
    }

    public class EmployeeInfo
    {
        public int EmployeeID { get; set; }
        public string DepartmentName { get; set; }
        public string DesignationName { get; set; }
        public DateOnly? JoiningDate { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public decimal GrossSalary { get; set; }
        public string EmployeeCode { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string Gender { get; set; }
    }
}
