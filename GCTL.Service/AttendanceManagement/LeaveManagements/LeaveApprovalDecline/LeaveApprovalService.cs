using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveApprovalDecline;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.LeaveManagements.LeaveApprovalDecline
{



    public class LeaveApprovalService:AppService<LeaveApplications>, ILeaveApprovalService
    {
        private readonly IGenericRepository<LeaveApplications> leaveRequest;
        private readonly AppDbContext appDb;
        private readonly IGenericRepository<EmployeeOfficeInfo> empoffi;
        private readonly ILeaveRequestService leaveRequestService;
        public LeaveApprovalService(IGenericRepository<LeaveApplications> leaveRequest, AppDbContext appDb, IGenericRepository<EmployeeOfficeInfo> empoffi, ILeaveRequestService leaveRequestService) : base(leaveRequest)
        {
            this.leaveRequest = leaveRequest;
            this.appDb = appDb;
            this.empoffi = empoffi;
            this.leaveRequestService = leaveRequestService;
        }





        #region  Get Data All  Leave  Requyest
        public async Task<PaginationService<LeaveApplications, LeaveApplicationsList>.PaginationResult<LeaveApplicationsList>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "", int? leaveTypeID = null, int? statusID = null)
        {
            try
            {

                var employeeId = await appDb.Users.Where(u => u.Id == userId).Select(e => e.EmployeeId).FirstOrDefaultAsync();
                var roleName = await (from user in appDb.Users
                                      join userRole in appDb.UserRoles on user.Id equals userRole.UserId
                                      join role in appDb.Roles on userRole.RoleId equals role.Id
                                      where user.Id == userId
                                      select role.Name)
                                     .FirstOrDefaultAsync();

                // 🔹 Step 3: Base query with includes
                var query = leaveRequest.AllActive().Where(x=>x.StatusID==null)
                    .Include(x => x.Employee)
                    .Include(x => x.Status)
                    .Include(x => x.LeaveType)
                    .OrderByDescending(x => x.LeaveApplicationID)
                    .AsQueryable();
                if (statusID != null)
                {
                    query = query.Where(x => x.StatusID == statusID);
                }

                if (leaveTypeID != null)
                {
                    query = query.Where(x => x.LeaveTypeID == leaveTypeID);
                }


                if (query == null)
                {
                    throw new InvalidOperationException("ActionLogs query source is null.");
                }
                // 🔹 Step 4: Filter if not SuperAdmin
                if (!string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.EmployeeID == employeeId);
                }
                //

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
                        LeaveApplicationID = b.LeaveApplicationID,
                        StatusName = !string.IsNullOrEmpty(b.Status?.StatusName) ? b.Status.StatusName : "",
                        IsFullDay = b.IsFullDay,
                        LeaveType = b.LeaveType != null ? b.LeaveType.LeaveTypeName : "",
                        FromDate = DateOnly.FromDateTime(b.FromDate.ToDateTime(TimeOnly.MinValue)).ToString("dd MMM yyyy"),
                        ToDate = DateOnly.FromDateTime(b.ToDate.ToDateTime(TimeOnly.MinValue)).ToString("dd MMM yyyy"),
                        Period = b.IsFullDay ? (b.ToDate.DayNumber - b.FromDate.DayNumber) + 1 : b.PartialFromTime.HasValue && b.PartialToTime.HasValue ? (int)(b.PartialToTime.Value - b.PartialFromTime.Value).TotalHours : 0,
                        EmployeeName = $"{b.Employee.FirstName} {b.Employee.LastName}",
                        EmployeeImage = !string.IsNullOrEmpty(b.Employee.EmployeeImageFileName) ? url + b.Employee.EmployeeImageFileName : "",
                        EmployeeDepartment = empoffi.AllActive().Where(e => e.EmployeeID == b.EmployeeID).Include(e => e.Department).Select(m => m.Department.DepartmentName).FirstOrDefault()
                       

                    });


                return result;


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");

                return new PaginationService<LeaveApplications, LeaveApplicationsList>.PaginationResult<LeaveApplicationsList>
                {
                    Data = new List<LeaveApplicationsList>(),
                    TotalCount = 0
                };
            }
        }


        #endregion

        #region Get Data By LeaveRequestID


       
        public async Task<LeaveApplicationApprovalModifyVM> GetLeaveRequestByIdAsync(int leaveApplicationID)
        {
            if (leaveApplicationID == 0)
            {
                return null;
            }

            await leaveRequest.BeginTransactionAsync();
            try
            {
                var data = await leaveRequest.GetByIdAsync(leaveApplicationID);
                if (data == null) return null;
                //
                SubsequentVM subsequent = null;
               
                    var fromDateTime = data.FromDate.ToDateTime(TimeOnly.MinValue);
                    var toDateTime = data.ToDate.ToDateTime(TimeOnly.MinValue);

                     subsequent = await leaveRequestService.SubsequentAsynce(fromDateTime, toDateTime);
                
             

                //
                LeaveApplicationApprovalModifyVM entityVM = new LeaveApplicationApprovalModifyVM
                {
                    LeaveApplicationID = data.LeaveApplicationID,
                    EmployeeIDEdit = data.EmployeeID,
                    LeaveTypeIDEdit = data.LeaveTypeID,
                    ReasonEdit = data.Reason,
                    IsFullDayEdit = data.IsFullDay,
                    FromDateEdit = data.FromDate,
                    ToDateEdit = data.ToDate,
                    PartialFromTimeEdit = data.PartialFromTime,
                    PartialToTimeEdit = data.PartialToTime,
                    Period = data.IsFullDay ? (data.ToDate.DayNumber - data.FromDate.DayNumber) + 1 : data.PartialFromTime.HasValue && data.PartialToTime.HasValue ? (int)(data.PartialToTime.Value - data.PartialFromTime.Value).TotalHours : 0,

                    TotalSubsequentDays = subsequent?.TotalSubsequentDays,
                    IsHolidayCountedAsLeave = subsequent?.IsHolidayCountedAsLeave ?? false,
                    IsWeekendCountedAsLeave = subsequent?.IsWeekendCountedAsLeave ?? false

                };
                return entityVM;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

        }

        #endregion

        #region  Update Method
        public async Task<CommonReturnViewModel> UpdateLeaveRequestAsynce(LeaveApplicationApprovalModifyVM entityVM)
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


                var entity = await leaveRequest.GetByIdAsync(entityVM.LeaveApplicationID);
                if (entity == null)
                    return null;

                entity.IsFullDay = entityVM.IsFullDayEdit;

                if (entityVM.IsFullDayEdit)
                {
                    entity.FromDate = entityVM.FromDateEdit ?? default;
                    entity.ToDate = entityVM.ToDateEdit ?? default;

                    // Clear partial day data
                    entity.PartialFromTime = null;
                    entity.PartialToTime = null;
                }
                else
                {
                    if (entityVM.ToDateFromDateCombinedEdit.HasValue)
                    {
                        var dateOnly = DateOnly.FromDateTime(entityVM.ToDateFromDateCombinedEdit.Value);
                        entity.FromDate = dateOnly;
                        entity.ToDate = dateOnly;
                    }

                    entity.PartialFromTime = entityVM.PartialFromTimeEdit;
                    entity.PartialToTime = entityVM.PartialToTimeEdit;
                }
                entity.Reason = entityVM.ReasonEdit ?? string.Empty;
                entity.LIP = entityVM.LIP;
                entity.LMAC = entityVM.LMAC;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = entityVM.UpdatedBy;
                await leaveRequest.UpdateAsync(entity);
                //  await userInfoService.ActionLogAsync("Leave Apply", ActionName.DataAdd, null, entity, entity.LeaveApplicationID, entityVM);
                await leaveRequest.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Updated Successfully."

                };
            }
            catch (Exception ex)
            {

                await leaveRequest.RollbackTransactionAsync();
                Console.WriteLine(ex.Message);
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "An error occurred while saving the leave request Update."
                };
            }
        }

        #endregion
    }
}
