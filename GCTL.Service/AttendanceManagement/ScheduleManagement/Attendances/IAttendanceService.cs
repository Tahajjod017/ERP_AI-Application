using GCTL.Core.ViewModels.APIViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.Attendances
{
    public interface IAttendanceService
    {
        Task<bool> AttendanceFromApps(PunchDataRequestVM model);
    }
}
