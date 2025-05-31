using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeEducational;
using GCTL.Data.Models;

namespace GCTL.Service.Employees.EmployeeEducational
{
    public interface IEmployeeEducationalService
    {
        Task<CommonReturnViewModel> DeleteAsync(int id);
        Task<List<EmployeeEducationGetViewModel>> GetEmployeeAdditionalByIdAsync(int id);
        Task<EmployeeEducationalPostViewModel> GetEmployeeEduData(int id);
        Task<CommonReturnViewModel> SubmitAsync(EmployeeEducationalPostViewModel model);
    }
}
