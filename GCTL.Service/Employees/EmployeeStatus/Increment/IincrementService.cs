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
       // Task<CommonReturnViewModel> SaveSalaryChange(SalaryChangeViewModel model);

        Task<CommonReturnViewModel> SaveAsync(SalaryChangeViewModel model);
        Task<List<IncrementApproveViewModel>> GetAllIncrementPendingList();
        Task<object> GetFilteredIncrementsAsync(IncrementFilterModel filter, string imgLink, int? loggedID);
        Task<object> GetFilteredApprovedIncrementsAsync(IncrementFilterModel filter, string imgLink, int? loggedID);
        Task<IncrementApproveViewModel> GetPendingIncrementDetailsByID(int id);
        Task<CommonReturnViewModel> ApproveIncrementAsync(IncrementActionModel action);
    }
}
