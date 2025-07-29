using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Increment;

namespace GCTL.Service.Employees.EmployeeStatus.Increment
{
    public interface IincrementService
    {
        Task<CommonReturnViewModel> SaveSalaryChange(SalaryChangeViewModel model);
    }
}
