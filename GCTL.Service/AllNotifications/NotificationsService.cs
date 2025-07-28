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

        public NotificationsService(IGenericRepository<AlertForEmployee> alertForEmp):base(alertForEmp) 
        {
            this.alertForEmp = alertForEmp;
        }

        public async Task<CommonReturnViewModel> GetAllNotificationsAsync(string url,string userId)
        {
            try
            {
                var result = await alertForEmp.AllActive().Include(x=>x.Employee).ToListAsync();

                var data = result.Select(x => new NotificationsGetVM
                {
                    AlertForEmployeeID = x.AlertForEmployeeID,
                    AlertID = x.AlertID,
                    EmployeeID = x.EmployeeID,
                    IsChecked = x.IsChecked,
                    CreatedBy = x.CreatedBy,
                    CreatedAt = x.CreatedAt,
                    EmployeeName = x.Employee != null ? $"{x.Employee.FirstName ?? ""} {x.Employee.LastName ?? ""}".Trim() : "",
                    EmployeeImage = x.Employee != null && !string.IsNullOrEmpty(x.Employee.EmployeeImageFileName) ? url + x.Employee.EmployeeImageFileName : "",
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

    }
}
