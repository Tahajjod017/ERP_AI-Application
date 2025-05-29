using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Data.Models;
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

        public LeaveRequestService(IGenericRepository<LeaveApplications> leaveRequest):base(leaveRequest) 
        {
            this.leaveRequest = leaveRequest;
        }

        public async Task<CommonReturnViewModel> SaveLeaveRequestAsync(LeaveApplicationsRequestVM entityVM)
        {
            if (entityVM == null)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Request payload cannot be null."
                };
            }

            await leaveRequest.BeginTransactionAsync();

            try
            {
                var entity = new LeaveApplications
                {
                    EmployeeID = entityVM.CreatedBy,
                    IsFullDay = entityVM.IsFullDay,
                    FromDate = entityVM.FromDate,
                    ToDate = entityVM.ToDate,
                    PartialFromTime = entityVM.PartialFromTime,
                    PartialToTime = entityVM.PartialToTime,
                    StatusID = entityVM.StatusID,
                    CreatedAt = DateTime.Now,
                    CreatedBy = entityVM.CreatedBy,
                    LeaveTypeID=entityVM.LeaveTypeID
                };

                await leaveRequest.AddAsync(entity);
                await leaveRequest.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Leave request saved successfully.",
                    Data = entity.LeaveApplicationID
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
