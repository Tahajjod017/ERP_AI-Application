using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignSpiralPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.AssignSpiralPattern
{
    public interface IAssignSpiralPatternService
    {
        Task<bool> AddAsync(AssignSpiralPatternSetupVM model);
    }
}
