using GCTL.Core.ViewModels.AttendanceManagement.AttendenceReportAlls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.EmployeeAttendenceReportAll.YearlyReports
{
    public interface IYearlyReportService
    {
        Task<List<YearlySpecialDayReportVM>> GetYearlySpecialDaysReport(int? departmentId, int? organizationId, int? employeeId, int year);
    }
}
