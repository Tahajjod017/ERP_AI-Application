using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.Employee.EmployeeFamily;
using GCTL.Core.ViewModels;

namespace GCTL.Service.Employees.EmployeeFamily
{
    public interface IEmployeeFamilyService
    {
        Task<CommonReturnViewModel> SaveAsync(EmployeeFamilyPostViewModel model);
        Task<CommonReturnViewModel> DeleteAsync(int id);
        Task<List<EmployeeFamilyGetViewModel>> GetEmployeeFamilyByIdAsync(int id);
        Task<EmployeeFamilyPostViewModel> GetEmployeeFamilyData(int id);
        Task<CommonReturnViewModel> UpdateAsync(EmployeeFamilyPostViewModel model);
    }
}
