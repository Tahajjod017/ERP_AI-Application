using GCTL.Core.ViewModels.APIViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.Attendances
{
    public interface IAppsAttendanceService
    {
        Task<PunchResultVM> AttendanceFromApps(PunchDataRequestVM model);
        Task<List<PunchResultVM>> GetTodaysMovement(int empId);
    }
}
