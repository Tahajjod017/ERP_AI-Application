using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
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

        Task<CommonReturnViewModel> SoftDeleteLeaveRequest(DeleteRequestVM deleteRequestVM);
        Task<PaginationService<LeaveApplications, LeaveApplicationsList>.PaginationResult<LeaveApplicationsList>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string currentSortColumn = "", string currentSortOrder = "" , string url = "");
    }
}
