using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.EmployeeShiftView;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.EmployeeShiftView
{
    public interface IEmployeeShiftViewService
    {
        Task<SeparatePaginationResult<EmployeeShiftViewSetupVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", int daysToShow = 7, DateTime? startDate = null);
    }
}
