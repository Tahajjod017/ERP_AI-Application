using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeBulkRegister
{
    public class EmployeeExcelVM
    {
        public string EmployeeCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string OfficialEmail { get; set; }
        public string PhoneNumber { get; set; }
        public string Branch { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string Comment { get; set; }
        public string Designation { get; set; }
        public string DepartmentName { get; set; }
        public string ImmediateSupervisorName { get; set; }
        public string DepartmentHeadName { get; set; }
    }


    public class EmpPagedResult<T>
    {
        public List<T> Data { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling((double)TotalRecords / PageSize);
    }


    public class EmployeeUpdateRequest
    {
        public int Index { get; set; }
        public string EmployeeCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string OfficialEmail { get; set; }
        public string PhoneNumber { get; set; }
        public string Branch { get; set; }
        public string JoiningDate { get; set; }
        public string Comment { get; set; }
        public string Designation { get; set; }
        public string DepartmentName { get; set; }
        public string ImmediateSupervisorName { get; set; }
        public string DepartmentHeadName { get; set; }
    }


    public class DuplicateCheckResult
    {
        public List<string> DuplicateEmployeeCodes { get; set; }
        public List<string> DuplicateEmails { get; set; }
        public bool HasDuplicates { get; set; }
    }



}
