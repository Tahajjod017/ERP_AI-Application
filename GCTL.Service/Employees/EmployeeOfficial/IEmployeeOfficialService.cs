using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.Employee.EmployeePersonal;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeOfficial;

namespace GCTL.Service.Employees.EmployeeOfficial
{
    public interface IEmployeeOfficialService
    {
        Task<CommonReturnViewModel> SaveEmployeeOfficialInfo(EmployeeOfficialPostViewModel model);
        Task<CommonReturnViewModel> UpdateEmployeeOfficialInfo(EmployeeOfficialPostViewModel model);
        Task<CommonReturnViewModel> CheckValidEmployeeInfo(EmployeeOfficialPostViewModel model);
        Task<EmployeeOfficialPostViewModel> GetEmployeeOfficalDetails(int id);
        Task<EmployeeOfficialGetViewModel> GetFullEmployeeOfficalDetails(int id);
        Task<IEnumerable<EmployeeOfficialGetViewModel>> GetAllEmployeeOfficialDetailsByCompanyAsync(int compId);
        Task<(List<SupervisorDto> data, int totalItem)> GetPagedSupervisorsAsync(string search, int page, int pageSize, int organizationId, int departmentId, string roleType);
    }
}
