using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.ActionLogVM;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.LeaveManagements.LeaveRequest
{
    public class LeaveRequestService:AppService<LeaveApplications>, ILeaveRequestService
    {
        private readonly IGenericRepository<LeaveApplications> leaveRequest;
        private readonly IGenericRepository<LeaveTypes> leaveTypes; 
        private readonly IGenericRepository<Statuses> leaveStatuses;
        public LeaveRequestService(IGenericRepository<LeaveApplications> leaveRequest, IGenericRepository<LeaveTypes> leaveTypes, IGenericRepository<Statuses> leaveStatuses) : base(leaveRequest)
        {
            this.leaveRequest = leaveRequest;
            this.leaveTypes = leaveTypes;
            this.leaveStatuses = leaveStatuses;
        }

        public async Task<PaginationService<LeaveApplications, LeaveApplicationsList>.PaginationResult<LeaveApplicationsList>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "")
        {
            try
            {

                var query = leaveRequest.All().OrderByDescending(x=>x.LeaveApplicationID).Include(x => x.Status).Include(x=>x.LeaveType);

                if (query == null)
                {
                    throw new InvalidOperationException("ActionLogs query source is null.");
                }


               


                var result = await PaginationService<LeaveApplications, LeaveApplicationsList>.GetPaginatedData(


                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,

                    currentSortColumn,
                    currentSortOrder,

                    term => b => EF.Functions.Like(b.LeaveApplicationID.ToString(), $"%{term}%")
                                 ,


                    b => new LeaveApplicationsList
                    {
                        //UserType = b.ActionLogID,
                        LeaveApplicationID=b.LeaveApplicationID,
                       // StatusName = b.Status != null ? b.Status.StatusName : "",
                        StatusName = !string.IsNullOrEmpty(b.Status?.StatusName) ? b.Status.StatusName : "",

                        LeaveType = b.LeaveType != null ? b.LeaveType.LeaveTypeName : "",
                        FromDate = DateOnly.FromDateTime(b.FromDate.ToDateTime(TimeOnly.MinValue)).ToString("dd MMM yyyy"),
                        ToDate = DateOnly.FromDateTime(b.ToDate.ToDateTime(TimeOnly.MinValue)).ToString("dd MMM yyyy"),
                        //Period = $"{b.FromDate} - {b.ToDate}",
                        Period = (b.ToDate.DayNumber - b.FromDate.DayNumber) + 1

                        // EmployeeUserName = b.CreatedByNavigation != null ? $"{b.CreatedByNavigation.FirstName} {b.CreatedByNavigation.LastName}" : ""
                    });
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPaginateActionLog: {ex.Message}");

                return new PaginationService<LeaveApplications, LeaveApplicationsList>.PaginationResult<LeaveApplicationsList>
                {
                    Data = new List<LeaveApplicationsList>(),
                    TotalCount = 0
                };
            }
        }

        public async Task<CommonReturnViewModel> SaveLeaveRequestAsync(LeaveApplicationsRequestVM entityVM)
        {
            if (entityVM == null)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Data Can not be null"
                };
            }

            await leaveRequest.BeginTransactionAsync();

            try
            {
                var entity = new LeaveApplications
                {
                    EmployeeID = entityVM.EmployeeID,
                    IsFullDay = entityVM.IsFullDay,
                    FromDate = entityVM.FromDate ?? DateOnly.FromDateTime(DateTime.Today),
                    ToDate = entityVM.ToDate ?? DateOnly.FromDateTime(DateTime.Today),
                    PartialFromTime = entityVM.PartialFromTime,
                    PartialToTime = entityVM.PartialToTime,
                    StatusID = entityVM.StatusID,
                    CreatedAt = DateTime.Now,
                    CreatedBy = entityVM.CreatedBy,
                    LeaveTypeID=entityVM.LeaveTypeID,
                    Reason=entityVM.Reason,
                    LIP=entityVM.LIP,
                    LMAC=entityVM.LMAC
                };

                await leaveRequest.AddAsync(entity);
                await leaveRequest.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Saved Successfully."
                  
                };
            }
            catch (Exception ex)
            {
               
                await leaveRequest.RollbackTransactionAsync();
                Console.WriteLine(ex.Message);
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "An error occurred while saving the leave request."
                };
            }
        }

    }
}
