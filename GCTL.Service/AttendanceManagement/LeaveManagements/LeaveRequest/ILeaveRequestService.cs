using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveSettings;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignDefaultShift;
using GCTL.Core.ViewModels.MasterSetup.ActionTakens;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.LeaveManagements.LeaveRequest
{
    public interface ILeaveRequestService
    {
        Task<CommonReturnViewModel> SaveLeaveRequestAsync(LeaveApplicationsRequestVM entityVM);
        Task<CommonReturnViewModel> UpdateLeaveRequestAsynce(LeaveApplicationEditVM entityVM);
        Task<LeaveApplicationEditVM> GetLeaveRequestByIdAsync(int leaveApplicationID);
        Task<CommonReturnViewModel> SoftDeleteLeaveRequest(DeleteRequestVM deleteRequestVM);
        Task<PaginationService<LeaveApplications, LeaveApplicationsList>.PaginationResult<LeaveApplicationsList>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string currentSortColumn = "", string currentSortOrder = "" , string url = "", string userId="",int? leaveTypeID=null,int ? statusID=null, int? organizationId = null,
    List<int> departmentIds = null,
    List<int> employeeIds = null, DateOnly ? fromDate =null, DateOnly ? toDate = null );

        Task<object> GetLeaveTypeTotaldays(int employeeId, int leaveTypeID);
        Task<List<CommonSelectVM>> GetAllEmployee(string userId);

        #region Get Leavepolicy as count or else
        Task<List<GetLeavePolicyConfigurationVM>> GetLeavePolicyIsCountAsync();
        Task<SubsequentVM> SubsequentAsynce(DateTime fromDate, DateTime toDate);
        #endregion

        #region Filtering Company Department Employee
        Task<List<CommonSelectVM>> GetCompanies();
        Task<List<CommonSelectVM>> GetDepartments();
        Task<List<MultiDropDown>> GetGroupedEmployees();

        Task<List<MultiDropDown>> GetDepartmentByCompany(int id);
        Task<List<MultiDropDown>> GetEmployeeByCompany(int id);
        Task<List<MultiDropDown>> GetEmployeeByDepartment(List<int> departmentIds);
      
        #endregion
    }

    
}
