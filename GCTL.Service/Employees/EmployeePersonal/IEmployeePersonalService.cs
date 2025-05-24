using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeePersonal;

namespace GCTL.Service.Employees.EmployeePersonal
{
    public interface IEmployeePersonalService
    {
        Task<CommonReturnViewModel> SaveEmployeePersonalInfo(EmployeePersonalPostViewModel model);
    }
}
