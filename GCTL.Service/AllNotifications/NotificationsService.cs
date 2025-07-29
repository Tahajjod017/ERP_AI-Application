using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AllNotificastios;
using GCTL.Data.Models;
using GCTL.Service.MasterSetup.Gender;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AllNotifications
{
    public class NotificationsService : AppService<AlertForEmployee>, INotificationsService
    {
        private readonly IGenericRepository<AlertForEmployee> alertForEmp;
        private readonly AppDbContext appDb;
        public NotificationsService(IGenericRepository<AlertForEmployee> alertForEmp, AppDbContext appDb = null) : base(alertForEmp)
        {
            this.alertForEmp = alertForEmp;
            this.appDb = appDb;
        }

        public async Task<CommonReturnViewModel> GetAllNotificationsAsync(string url,string userId)
        {
            try
            {
                var employeeId = await appDb.Users.Where(u => u.Id == userId).Select(e => e.EmployeeId).FirstOrDefaultAsync();
                var result = await alertForEmp.AllActive().Include(x=>x.Alert).Include(x=>x.CreatedByNavigation).Where(x=>x.EmployeeID==employeeId).OrderByDescending(x=>x.AlertForEmployeeID).ToListAsync();

                var data = result.Select(x => new NotificationsGetVM
                {
                    AlertForEmployeeID = x.AlertForEmployeeID,
                    AlertID = x.AlertID,
                    EmployeeID = x.EmployeeID,
                    IsChecked = x.IsChecked,
                    CreatedBy = x.CreatedBy,
                    CreatedAt = x.CreatedAt,
                    EmployeeName = x.CreatedByNavigation != null ? $"{x.CreatedByNavigation.FirstName ?? ""} {x.CreatedByNavigation.LastName ?? ""}".Trim() : "",
                    EmployeeImage = x.CreatedByNavigation != null && !string.IsNullOrEmpty(x.CreatedByNavigation.EmployeeImageFileName) ? url + x.CreatedByNavigation.EmployeeImageFileName : "",
                    AlertNote=x.Alert !=null ? x.Alert.AlertNote :"",
                    AlertTitle= x.Alert != null ? x.Alert.AlertTitle : ""
                }).ToList();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<CommonReturnViewModel> IsCheckedAsync(IsCheckedVM entityVM)
        {
            var returnModel = new CommonReturnViewModel();

            try
            {
                if (entityVM.AlertForEmployeeID == null)
                {
                    returnModel.Success = false;
                    returnModel.Message = "Data Not Found.";
                    return returnModel;
                }

                // Get the alert to update (use your actual key logic)
                var alert = await alertForEmp.GetByIdAsync(entityVM.AlertForEmployeeID); 

                if (alert == null)
                {
                    returnModel.Success = false;
                    returnModel.Message = "Alert not found for the employee.";
                    return returnModel;
                }

                alert.IsChecked = entityVM.IsChecked;
                alert.LMAC = entityVM.LMAC;
                alert.LIP = entityVM.LIP;
                alert.UpdatedAt=DateTime.Now;
                alert.UpdatedBy = entityVM.UpdatedBy;
                await alertForEmp.UpdateAsync(alert);
                returnModel.Success = true;
                returnModel.Message = "Alert check status updated successfully.";
            }
            catch (Exception ex)
            {
                returnModel.Success = false;
                returnModel.Message = $"Error: {ex.Message}";
            }

            return returnModel;
        }

    }
}
