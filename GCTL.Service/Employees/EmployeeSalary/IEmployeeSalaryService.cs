
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeOfficial;
using GCTL.Core.ViewModels.Employee.EmployeeSalary;

namespace GCTL.Service.Employees.EmployeeSalary
{
    public interface IEmployeeSalaryService
    {
        Task<IEnumerable<EmployeeSalaryGetViewModel>> GetAllEmployeeSalaryByComapnyAsync(int compId);
        Task<EmployeeSalaryGetViewModel> GetEmployeeSalaryByEmployeeIdAsync(int employeeId);
        Task<EmployeeSalaryPostViewModel> GetEmployeeSalaryByEmployeeIdPostAsync(int employeeId);
        
        Task<CommonReturnViewModel> SaveEmployeeSalaryAsync(EmployeeSalaryPostViewModel model);
        Task<CommonReturnViewModel> UpdateEmployeeSalaryAsync(EmployeeSalaryPostViewModel model);
    }
}
