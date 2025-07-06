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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GCTL.Service.AttendanceManagement.LeaveManagements.LeaveApprovalDecline
{



    public class LeaveApprovalService:AppService<LeaveApplications>, ILeaveApprovalService
    {
        private readonly IGenericRepository<LeaveApplications> leaveRequest;
        private readonly AppDbContext appDb;
        private readonly IGenericRepository<EmployeeOfficeInfo> empoffi;
        private readonly ILeaveRequestService leaveRequestService;
        private readonly IGenericRepository<LeaveBalances> leaveBalance;
        private readonly IGenericRepository<LeaveTypes> leaveTypesRepository;
        private readonly IGenericRepository<LeaveBaseApprovalHistory> leaveBaseAprovalHistory;
        private readonly IGenericRepository<Statuses> status;
        public LeaveApprovalService(IGenericRepository<LeaveApplications> leaveRequest, AppDbContext appDb, IGenericRepository<EmployeeOfficeInfo> empoffi, ILeaveRequestService leaveRequestService, IGenericRepository<LeaveBalances> leaveBalance, IGenericRepository<LeaveTypes> leaveTypesRepository , IGenericRepository<LeaveBaseApprovalHistory> leaveBaseAprovalHistory , IGenericRepository<Statuses> status) : base(leaveRequest)
        {
            this.leaveRequest = leaveRequest;
            this.appDb = appDb;
            this.empoffi = empoffi;
            this.leaveRequestService = leaveRequestService;
            this.leaveBalance = leaveBalance;

            this.leaveTypesRepository = leaveTypesRepository;
            this.leaveBaseAprovalHistory = leaveBaseAprovalHistory;
            this.status = status;
        }





        #region  Get Data All  Leave  Requyest above table 


        public async Task<PaginationService<LeaveApplications, LeaveApplicationsList>.PaginationResult<LeaveApplicationsList>>
    GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "", int? leaveTypeID = null, int? statusID = null, DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            try
            {
                var employeeId = await appDb.Users
                    .Where(u => u.Id == userId)
                    .Select(e => e.EmployeeId)
                    .FirstOrDefaultAsync();

                var roleName = await (from user in appDb.Users
                                      join userRole in appDb.UserRoles on user.Id equals userRole.UserId
                                      join role in appDb.Roles on userRole.RoleId equals role.Id
                                      where user.Id == userId
                                      select role.Name).FirstOrDefaultAsync();

                var query = leaveRequest.AllActive()
                    .Where(x => x.StatusID == null)
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
                if(fromDate.HasValue && toDate.HasValue)
                {
                    query = query.Where(x => x.FromDate >= fromDate.Value && x.ToDate <= toDate.Value);
                }
                if (!string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.EmployeeID == employeeId);
                }

                var leaveBalances = await leaveBalance.AllActive().ToListAsync();
                var leaveTypes = await leaveTypesRepository.AllActive().ToListAsync();
                var employeeDepartments = await empoffi.AllActive()
                    .Include(e => e.Department)
                    .Select(e => new { e.EmployeeID, e.Department.DepartmentName })
                    .ToListAsync();

                var result = await PaginationService<LeaveApplications, LeaveApplicationsList>.GetPaginatedData(
                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    currentSortColumn,
                    currentSortOrder,
                    term => b => EF.Functions.Like(b.LeaveApplicationID.ToString(), $"%{term}%"),
                    b =>
                    {
                        var balance = leaveBalances.FirstOrDefault(lb =>
                            lb.EmployeeID == b.EmployeeID &&
                            lb.LeaveTypeID == b.LeaveTypeID ||
                            lb.ApplicableYear == b.FromDate.Year);

                        var defaultLeaveDays = leaveTypes.FirstOrDefault(x => x.LeaveTypeID == b.LeaveTypeID)?.LeaveDays ?? 0;

                        var availableLeaveDays = balance != null
                            ? (balance.TotalLeave ?? 0) - (balance.Taken ?? 0)
                            : defaultLeaveDays;

                        var department = employeeDepartments.FirstOrDefault(e => e.EmployeeID == b.EmployeeID)?.DepartmentName ?? "";

                        return new LeaveApplicationsList
                        {
                            LeaveApplicationID = b.LeaveApplicationID,
                            StatusName = !string.IsNullOrEmpty(b.Status?.StatusName) ? b.Status.StatusName : "",
                            IsFullDay = b.IsFullDay,
                            LeaveType = b.LeaveType?.LeaveTypeName ?? "",
                            FromDate = DateOnly.FromDateTime(b.FromDate.ToDateTime(TimeOnly.MinValue)).ToString("dd MMM yyyy"),
                            ToDate = DateOnly.FromDateTime(b.ToDate.ToDateTime(TimeOnly.MinValue)).ToString("dd MMM yyyy"),
                            Period = b.IsFullDay
                                ? (b.ToDate.DayNumber - b.FromDate.DayNumber) + 1
                                : b.PartialFromTime.HasValue && b.PartialToTime.HasValue
                                    ? (int)(b.PartialToTime.Value - b.PartialFromTime.Value).TotalHours
                                    : 0,
                            EmployeeName = $"{b.Employee.FirstName} {b.Employee.LastName}",
                            EmployeeImage = !string.IsNullOrEmpty(b.Employee.EmployeeImageFileName) ? url + b.Employee.EmployeeImageFileName : "",
                            EmployeeDepartment = department,
                            AvailableLeaveDays = availableLeaveDays
                        };
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

        #region  Get Data All  Leave  Requyest below table 
        public async Task<PaginationService<LeaveApplications, LeaveApplicationsList>.PaginationResult<LeaveApplicationsList>> GetAllTableBelowAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "", int? leaveTypeID = null, int? statusID = null, DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            try
            {

                var employeeId = await appDb.Users.Where(u => u.Id == userId).Select(e => e.EmployeeId).FirstOrDefaultAsync();
                var roleName = await (from user in appDb.Users
                                      join userRole in appDb.UserRoles on user.Id equals userRole.UserId
                                      join role in appDb.Roles on userRole.RoleId equals role.Id
                                      where user.Id == userId  select role.Name) .FirstOrDefaultAsync();


                //
               

                //
                // 🔹 Step 3: Base query with includes
                var query = leaveRequest.AllActive().Where(x=>x.StatusID !=null)
                    .Include(x => x.Employee)
                    .Include(x => x.Status)
                    .Include(x => x.LeaveType)
                    .OrderByDescending(x => x.LeaveApplicationID).AsQueryable();
                //

             
                if (statusID != null)
                {
                    query = query.Where(x => x.StatusID == statusID);
                }

                if (leaveTypeID != null)
                {
                    query = query.Where(x => x.LeaveTypeID == leaveTypeID);
                }

                if (fromDate.HasValue && toDate.HasValue)
                {
                    query = query.Where(x => x.FromDate >= fromDate.Value && x.ToDate <= toDate.Value);
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
                        EmployeeDepartment = empoffi.AllActive().Where(e => e.EmployeeID == b.EmployeeID).Include(e => e.Department).Select(m => m.Department.DepartmentName).FirstOrDefault(),

                        //
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

                // avilable days 
                int applicableYear = data.FromDate.Year;

                var leaveBalancevaluse = await leaveBalance.AllActive().FirstOrDefaultAsync(x=>x.EmployeeID==data.EmployeeID && x.LeaveTypeID==data.LeaveTypeID || x.ApplicableYear==applicableYear);
                var leaveType = await leaveTypesRepository.AllActive().FirstOrDefaultAsync(x => x.LeaveTypeID == data.LeaveTypeID);
                decimal availableLeaveDays = 0;
                if (leaveBalancevaluse != null && leaveBalancevaluse.TotalLeave.HasValue && leaveBalancevaluse.Taken.HasValue)
                {
                    availableLeaveDays = leaveBalancevaluse.TotalLeave.Value - leaveBalancevaluse.Taken.Value;
                }
                else
                {
                    availableLeaveDays = leaveType?.LeaveDays ?? 0;
                }
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
                    IsWeekendCountedAsLeave = subsequent?.IsWeekendCountedAsLeave ?? false,
                    AvailableLeaveDays=availableLeaveDays,

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

                var leavStatusApproved = await status.AllActive().Where(x => EF.Functions.Like(x.StatusName, "%APPROVED%")).Select(x => x.StatusID).FirstOrDefaultAsync();
                var leavStatusDecline = await status.AllActive().Where(x => EF.Functions.Like(x.StatusName, "%DECLINEED%")).Select(x => x.StatusID).FirstOrDefaultAsync();
                int statusId = entityVM.Approved ? leavStatusApproved : entityVM.Declined ? leavStatusDecline : 0;
                if (statusId == 0)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Approval or Decline must be selected."
                    };
                }
                var leaveBalanceSave = await leaveBalance.AllActive().Select(x => new { x.LeaveTypeID, x.EmployeeID, x.TotalLeave, x.Taken, x.ApplicableYear }).ToListAsync();
                //
                var entity = await leaveRequest.GetByIdAsync(entityVM.LeaveApplicationID);
                if (entity == null)
                    return null;

                // Update full-day or partial-day fields
                entity.IsFullDay = entityVM.IsFullDayEdit;
                if (entityVM.IsFullDayEdit)
                {
                    entity.FromDate = entityVM.FromDateEdit ?? default;
                    entity.ToDate = entityVM.ToDateEdit ?? default;
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

                
                entity.StatusID = statusId;
                // If approved, update or insert leave balance
                if (statusId == leavStatusApproved)
                {
                    var leaveDaysFromConfig = await leaveTypesRepository.AllActive()
                        .Where(x => x.LeaveTypeID == entityVM.LeaveTypeIDEdit)
                        .Select(x => new { x.LeaveDays, x.ApplicableYear })
                        .FirstOrDefaultAsync();

                    var existingBalance = await leaveBalance.AllActive()
                        .FirstOrDefaultAsync(x =>
                            x.EmployeeID == entityVM.EmployeeIDEdit &&
                            x.LeaveTypeID == entityVM.LeaveTypeIDEdit);

                    if (existingBalance != null)
                    {
                        existingBalance.Taken = (existingBalance.Taken ?? 0) + entityVM.TotalAppliedDays;
                        existingBalance.TotalLeave = leaveDaysFromConfig.LeaveDays;
                        existingBalance.ApplicableYear = leaveDaysFromConfig.ApplicableYear;
                        existingBalance.LIP = entityVM.LIP;
                        existingBalance.LMAC = entityVM.LMAC;
                        existingBalance.UpdatedAt = DateTime.Now;
                        existingBalance.UpdatedBy = entityVM.UpdatedBy;

                        await leaveBalance.UpdateAsync(existingBalance);
                    }
                    else
                    {
                        var newBalance = new LeaveBalances
                        {
                            EmployeeID = entityVM.EmployeeIDEdit,
                            LeaveTypeID = entityVM.LeaveTypeIDEdit,
                            Taken = entityVM.TotalAppliedDays,
                            TotalLeave = leaveDaysFromConfig.LeaveDays,
                            ApplicableYear = leaveDaysFromConfig.ApplicableYear,
                            CreatedAt = DateTime.Now,
                            CreatedBy = entityVM.CreatedBy,
                            LIP = entityVM.LIP,
                            LMAC = entityVM.LMAC
                        };

                        await leaveBalance.AddAsync(newBalance);
                    }
                }

                // Common metadata update
                entity.LIP = entityVM.LIP;
                entity.LMAC = entityVM.LMAC;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = entityVM.UpdatedBy;

                await leaveRequest.UpdateAsync(entity);

                //

                // LeaveBaseApprovalHistory
                var leaveBase = new LeaveBaseApprovalHistory
                {
                    LeaveApplicationID = entityVM.LeaveApplicationID,
                    StatusID = statusId,
                    ApproverNote = entityVM.ApprovalNote,
                    CreatedAt = DateTime.Now,
                    CreatedBy = entityVM.CreatedBy,
                    LIP = entityVM.LIP,
                    LMAC = entityVM.LMAC,

                };
                await leaveBaseAprovalHistory.AddAsync(leaveBase);
                //
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

        #region Dispaly LeaveDays 
        public async Task<List<LeaveBalancesDisplayVM>> GetLeaveTypeBalancesForEmployee(string userId)
        {
            // Get employee ID from user ID
            var employeeId = await appDb.Users
                .Where(u => u.Id == userId)
                .Select(e => e.EmployeeId)
                .FirstOrDefaultAsync();

            if (employeeId == null)
                return new List<LeaveBalancesDisplayVM>(); // or throw exception if required

            // Get the role of the user
            var roleName = await (
                from user in appDb.Users
                join userRole in appDb.UserRoles on user.Id equals userRole.UserId
                join role in appDb.Roles on userRole.RoleId equals role.Id
                where user.Id == userId
                select role.Name
            ).FirstOrDefaultAsync();

            // Base query for leave balances
            var baseQuery = from lt in leaveTypesRepository.AllActive()
                            join lb in leaveBalance.AllActive().Where(x => x.EmployeeID == employeeId)
                                on lt.LeaveTypeID equals lb.LeaveTypeID into lbGroup
                            from lb in lbGroup.DefaultIfEmpty()
                            select new LeaveBalancesDisplayVM
                            {
                                LeaveBalanceID = lb != null ? lb.LeaveBalanceID : 0,
                                EmployeeID = employeeId,
                                LeaveTypeID = lt.LeaveTypeID,
                                LeaveTypeName = lt.LeaveTypeName,
                                TotalLeave = lb.TotalLeave ?? lt.LeaveDays,
                                Taken = lb.Taken,
                                ApplicableYear = lb.ApplicableYear,
                                RemainingDays = lb != null
                                    ? (lb.TotalLeave - lb.Taken)
                                    : (lt.LeaveDays ?? 0)
                            };

            // Only restrict by employee ID if the user is not SuperAdmin
            if (!string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                baseQuery = baseQuery.Where(x => x.EmployeeID == employeeId);
            }

            return await baseQuery.ToListAsync();
        }


        #endregion
    }
}
