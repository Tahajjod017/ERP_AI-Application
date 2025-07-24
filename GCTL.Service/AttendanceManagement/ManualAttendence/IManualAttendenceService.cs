using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.AttendanceManagement.ManualAttendence;

namespace GCTL.Service.AttendanceManagement.ManualAttendence
{
    public interface IManualAttendenceService
    {
        Task<List<AttendanceRecord>> GetAllDataAsync(string imgTemFolder);
        Task<List<AttendanceRecord>> GetAbnormalPunchDataAsync(string imgTemFolder);
    }
}
