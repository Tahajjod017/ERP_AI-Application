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
   

        Task<bool> SaveOrUpdateEmployeeBenefitsAsync(EmployeeBenifitPostViewModel model);
        Task<EmployeeBenifitPostViewModel> GetEmployeeBenefitsAsync(string employeeId);
    }
}
