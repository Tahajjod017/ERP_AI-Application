using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveApprovalDecline;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.LeaveManagements.LeaveApprovalDecline
{
    public interface ILeaveApprovalService
    {
        Task<LeaveApplicationApprovalModifyVM> GetLeaveRequestByIdAsync(int leaveApplicationID);
        Task<PaginationService<LeaveApplications, LeaveApplicationsList>.PaginationResult<LeaveApplicationsList>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
       string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "", int? leaveTypeID = null, int? statusID = null, DateOnly? fromDate = null, DateOnly? toDate = null);
        Task<CommonReturnViewModel> UpdateLeaveRequestAsynce(LeaveApplicationApprovalModifyVM entityVM);

        // leave request list data of below

        Task<PaginationService<LeaveApplications, LeaveApplicationsList>.PaginationResult<LeaveApplicationsList>> GetAllTableBelowAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
       string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "", int? leaveTypeID = null, int? statusID = null, DateOnly? fromDate = null, DateOnly? toDate = null);
        //
        #region Dispaly LeaveDays 
        Task<List<LeaveBalancesDisplayVM>> GetLeaveTypeBalancesForEmployee(string userId);
        #endregion

    }
}
