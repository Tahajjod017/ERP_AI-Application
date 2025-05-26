
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeSalary;

namespace GCTL.Service.Employees.EmployeeSalary
{
    public interface IEmployeeSalaryService
    {
        Task<EmployeeSalaryGetViewModel> GetEmployeeSalaryByEmployeeIdAsync(int employeeId);
        
        Task<CommonReturnViewModel> SaveEmployeeSalaryAsync(EmployeeSalaryPostViewModel model);
        Task<CommonReturnViewModel> UpdateEmployeeSalaryAsync(EmployeeSalaryPostViewModel model);
    }
}
