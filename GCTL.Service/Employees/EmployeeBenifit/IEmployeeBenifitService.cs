using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.Employee.EmployeeBenifit;

namespace GCTL.Service.Employees.EmployeeBenifit
{
    public interface IEmployeeBenifitService
    {
        Task<EmployeeBenifitGetViewModel> GetEmployeeBenifitByEmployeeIdAsync(int employeeId);
    }
}
