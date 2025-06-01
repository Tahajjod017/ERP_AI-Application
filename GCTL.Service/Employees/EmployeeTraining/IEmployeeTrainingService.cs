using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.Employee.EmployeeEducational;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeTraining;

namespace GCTL.Service.Employees.EmployeeTraining
{
    public interface IEmployeeTrainingService
    {
        Task<CommonReturnViewModel> DeleteAsync(int id);
        Task<List<EmployeeTrainingGetViewModel>> GetEmployeeTrainingByIdAsync(int id);
        Task<EmployeeTrainingPostViewModel> GetEmployeeEduData(int id);
        Task<CommonReturnViewModel> SaveAsync(EmployeeTrainingPostViewModel model);
        Task<CommonReturnViewModel> UpdateAsync(EmployeeTrainingPostViewModel model);
    }
}
