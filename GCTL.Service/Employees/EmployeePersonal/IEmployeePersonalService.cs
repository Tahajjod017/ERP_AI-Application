using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeePersonal;
using GCTL.Data.Models;

namespace GCTL.Service.Employees.EmployeePersonal
{
    public interface IEmployeePersonalService
    {
        Task<CommonReturnViewModel> SaveEmployeePersonalInfo(EmployeePersonalPostViewModel model);
        Task<CommonReturnViewModel> CheckValidEmployeeInfo(EmployeePersonalPostViewModel model);
        Task<EmployeePersonalGetViewModel> GetEmployeePersonalById(int id);
    }
}
