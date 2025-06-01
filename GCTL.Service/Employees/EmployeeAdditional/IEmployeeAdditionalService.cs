using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeAdditional;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.Service.Employees.EmployeeAdditional
{
    public interface IEmployeeAdditionalService
    {
        Task<EmployeeAdditionalPostViewModel> GetEmployeeAdditionalByIdAsync(int employeeId);
        Task<CommonReturnViewModel> SubmitAsync(EmployeeAdditionalPostViewModel model);
    }
}
