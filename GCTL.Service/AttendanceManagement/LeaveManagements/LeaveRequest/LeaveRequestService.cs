using Azure.Core;
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.ActionLogVM;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveSettings;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.LeaveManagements.LeaveRequest
{
    public class LeaveRequestService:AppService<LeaveApplications>, ILeaveRequestService
    {
        private readonly IGenericRepository<LeaveApplications> leaveRequest;
        private readonly IGenericRepository<LeaveTypes> leaveTypes; 
        private readonly IGenericRepository<Statuses> leaveStatuses;
        private readonly IUserInfoService userInfoService;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> employee;
        private readonly AppDbContext appDb;
 

        public LeaveRequestService(IGenericRepository<LeaveApplications> leaveRequest, IGenericRepository<LeaveTypes> leaveTypes, IGenericRepository<Statuses> leaveStatuses, IUserInfoService userInfoService, IGenericRepository<Data.Models.Employees> employee, AppDbContext appDb):base(leaveRequest)
        {
            this.leaveRequest = leaveRequest;
            this.leaveTypes = leaveTypes;
            this.leaveStatuses = leaveStatuses;
            this.userInfoService = userInfoService;
            this.employee = employee;
            this.appDb = appDb;
        }

        #region  Get Data All  Leave  Requyest
        public async Task<PaginationService<LeaveApplications, LeaveApplicationsList>.PaginationResult<LeaveApplicationsList>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "" , string url = "")
        {
            try
            {

                var query = leaveRequest.AllActive().OrderByDescending(x => x.LeaveApplicationID).Include(x=>x.Employee).Include(x => x.Status).Include(x => x.LeaveType);
               

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
                        LeaveApplicationID = b.LeaveApplicationID,
                        StatusName = !string.IsNullOrEmpty(b.Status?.StatusName) ? b.Status.StatusName : "",
                        IsFullDay = b.IsFullDay,
                        LeaveType = b.LeaveType != null ? b.LeaveType.LeaveTypeName : "",
                        FromDate = DateOnly.FromDateTime(b.FromDate.ToDateTime(TimeOnly.MinValue)).ToString("dd MMM yyyy"),
                        ToDate = DateOnly.FromDateTime(b.ToDate.ToDateTime(TimeOnly.MinValue)).ToString("dd MMM yyyy"),
                        Period = b.IsFullDay ? (b.ToDate.DayNumber - b.FromDate.DayNumber) + 1 : b.PartialFromTime.HasValue && b.PartialToTime.HasValue ? (int)(b.PartialToTime.Value - b.PartialFromTime.Value).TotalHours : 0,
       
                        EmployeeName = $"{b.Employee.FirstName} {b.Employee.LastName}",
                    
                        EmployeeImage = !string.IsNullOrEmpty(b.Employee.EmployeeImageFileName) ? url + b.Employee.EmployeeImageFileName: "",

                        EmployeeDepartment = b.Employee.EmployeeOfficeInfoCreatedByNavigation.Where(x => x.Department != null) .OrderByDescending(x => x.EmployeeOfficeInfoID).Select(x => x.Department.DepartmentName).FirstOrDefault()



                        //EmployeeUserName = b.CreatedByNavigation != null ? $"{b.CreatedByNavigation.FirstName} {b.CreatedByNavigation.LastName}" : ""
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

        #region  Save Leave Reqest
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
                    LeaveTypeID = entityVM.LeaveTypeID,
                    Reason = entityVM.Reason,
                    LIP = entityVM.LIP,
                    LMAC = entityVM.LMAC
                };

                await leaveRequest.AddAsync(entity);
                await userInfoService.ActionLogAsync("Leave Apply", ActionName.DataAdd, null, entity, entity.LeaveApplicationID, entityVM);
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
        #endregion

        #region Delete Leave Request
        public async Task<CommonReturnViewModel> SoftDeleteLeaveRequest(DeleteRequestVM deleteRequestVM)
        {
            await leaveRequest.BeginTransactionAsync();
            try
            {
                var data = await leaveRequest.FindAsync(x => deleteRequestVM.Ids.Contains(x.LeaveApplicationID));
                if (data == null || data.Count == 0)
                {
                    return new CommonReturnViewModel
                    {
                        Success=false,
                        Message = "No data found to delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<AddNewLeaveSave>>(
             JsonConvert.SerializeObject(data, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
                var targetIds = data.Select(x => (int?)x.LeaveApplicationID).ToList();
                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.LIP = deleteRequestVM.LIP;
                    item.LMAC = deleteRequestVM.LMAC;
                    item.DeletedBy = deleteRequestVM.DeletedBy ?? null;
                }

                await leaveRequest.UpdateRangeAsync(data);
                await userInfoService.ActionLogDeleteAsync("Leave Settigs", ActionName.DataDeleted, null, beforeEntity, targetIds, deleteRequestVM);
                await leaveRequest.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success=true,
                    Message = $"Deleted Successfully."
                    
                };
            }
            catch (Exception ex)
            {
                await leaveRequest.RollbackTransactionAsync();
                throw new Exception("Error occurred during the deletion of data.", ex);
            }
        }
        #endregion

        #region Get LeaveType Total Days
        public async Task<object> GetLeaveTypeTotaldays(int leaveTypeID)
        {
            var data = await leaveTypes.AllActive()
         .Where(l => l.LeaveTypeID == leaveTypeID)
         .Select(l => new 
         {
             leaveDays = l.LeaveDays
         }).FirstOrDefaultAsync();

            return data;
        }


        #endregion
        #region Get All Employee or Single
        public async Task<List<CommonSelectVM>> GetAllEmployee()
        {
            var data = await employee.AllActive()
                .Select(x => new CommonSelectVM
                {
                    Id = x.EmployeeID,
                    Name = $"{x.FirstName}{x.LastName}",

                }).ToListAsync();
            return data;
        }




        #endregion

    }
}
