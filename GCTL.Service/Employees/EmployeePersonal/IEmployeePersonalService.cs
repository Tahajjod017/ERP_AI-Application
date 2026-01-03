using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeOfficial;
using GCTL.Core.ViewModels.Employee.EmployeePersonal;
using GCTL.Data.Models;

namespace GCTL.Service.Employees.EmployeePersonal
{
    public interface IEmployeePersonalService
    {
        Task<CommonReturnViewModel> SaveEmployeePersonalInfo(EmployeePersonalPostViewModel model);
        Task<CommonReturnViewModel> CheckValidEmployeeInfo(EmployeePersonalPostViewModel model);
        Task<EmployeePersonalGetViewModel> GetEmployeePersonalById(int id);

        Task<IEnumerable<EmployeePersonalGetViewModel>> GetAllEmployeePersonalByCompanyAsync(int compId);
        Task<PaginatedResult<CommonSelectVM>> GetEmployees(string search, int page = 1, int pageSize = 50, bool hasEmployeePermission = false, int? empId = null);
        Task<CommonSelectVM> GetEmployeeById(int id);
        Task<string> GetEmployeeCode();
    }
}
