
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.FieldServices;
using GCTL.Core.ViewModels.Finance.AddSubAccountVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL.Service.FieldServices.EmployeeAdvanced
{
    public interface IEmployeeAdvanced
    {
        Task<CommonReturnViewModel>AddAsync(EmployeeAdvancedVM emp);
        Task<IEnumerable<CommonSelectVM>>EmployeeDD(); //Modern Dropdown for Employees

        Task<ReturnDataView<SelectListItem>> GetJobTypeAsync(string search, int page, int pageSize, int organizationID);//Modern Dropdown for Job Types

        Task<CommonReturnViewModel> ApproveAsync(int id, int approvedByUserId);

        Task<List<EmployeeAdvancedVM>> GetJobByCusId(int customerId); //Cascading

       
    }
}
