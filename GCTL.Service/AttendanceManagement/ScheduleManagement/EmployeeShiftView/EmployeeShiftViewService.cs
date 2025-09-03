using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OfficeDayRoster;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.EmployeeShiftView
{
    public class EmployeeShiftViewService
    {


        #region GetAllAsync
        //public async Task<SeparatePaginationResult<EmployeeShiftViewSetupVM>> GetAllAsync(
        //    int pageNumber = 1,
        //    int pageSize = 5,
        //    string searchTerm = "",
        //    int daysToShow = 7,
        //    DateTime? startDate = null)
        //{
        //    startDate ??= DateTime.Today;
        //    var endDate = startDate.Value.AddDays(daysToShow);

        //    // Base query with projection to lightweight DTO
        //    var baseQuery = _genericRepository.AllActive()
        //        .AsNoTracking()
        //        .Where(x => x.DeletedAt == null && x.DayDate >= startDate && x.DayDate < endDate)
        //        .Select(x => new
        //        {
        //            x.EmployeeID,
        //            EmployeeCode = x.Employee.EmployeeCode,
        //            EmployeeFirstName = x.Employee.FirstName,
        //            EmployeeLastName = x.Employee.LastName,
        //            x.OrganizationID,
        //            OrganizationName = x.Organization.OrganizationName,
        //            WeekendSettings = x.Organization.WeekendSettings.SelectMany(ws => ws.WeekendDays).Select(wd => (int)wd.WeekdayNumber).Distinct(),
        //            Holidays = x.Organization.Holidays.Where(h => h.StartDate <= endDate && h.EndDate >= startDate).Select(h => new { h.StartDate, h.EndDate, h.HolidayTitle }),
        //            Leaves = x.Employee.LeaveApplicationsEmployee.Where(l => l.IsFinalApproved == true)
        //                .Select(l => new
        //                {
        //                    l.LeaveApplicationID,
        //                    l.FromDate,
        //                    l.ToDate,
        //                    l.IsFullDay,
        //                    l.PartialFromTime,
        //                    l.PartialToTime,
        //                    LeaveTypeName = l.LeaveType != null ? l.LeaveType.LeaveTypeName : null
        //                }),
        //            x.DepartmentID,
        //            DepartmentName = x.Department.DepartmentName,
        //            x.ShiftID,
        //            ShiftName = x.Shift.ShiftName,
        //            ShiftStartTime = x.Shift.StartTime,
        //            ShiftEndTime = x.Shift.EndTime,
        //            x.DayDate,
        //            x.RosterInOfficeDayID
        //        });

        //    // Apply search filter
        //    if (!string.IsNullOrWhiteSpace(searchTerm))
        //    {
        //        searchTerm = Regex.Replace(searchTerm, @"\s+", " ").Trim().ToLower();
        //        baseQuery = baseQuery.Where(x =>
        //            EF.Functions.Like(x.ShiftName, $"%{searchTerm}%") ||
        //            EF.Functions.Like(x.OrganizationName, $"%{searchTerm}%") ||
        //            EF.Functions.Like(x.EmployeeFirstName, $"%{searchTerm}%") ||
        //            EF.Functions.Like(x.EmployeeLastName, $"%{searchTerm}%") ||
        //            EF.Functions.Like(x.EmployeeCode, $"%{searchTerm}%") ||
        //            EF.Functions.Like(x.DepartmentName, $"%{searchTerm}%")
        //        );
        //    }

        //    // Get total count for pagination
        //    var totalCount = await baseQuery.Select(x => x.EmployeeID).Distinct().CountAsync();

        //    // Fetch paged EmployeeIDs
        //    var pagedEmployeeIds = await baseQuery.Select(x => x.EmployeeID).Distinct().OrderBy(id => id)
        //        .Skip((pageNumber - 1) * pageSize).Take(pageSize)
        //        .ToListAsync();

        //    if (!pagedEmployeeIds.Any())
        //        return new SeparatePaginationResult<EmployeeShiftViewSetupVM>
        //        {
        //            Data = new List<EmployeeShiftViewSetupVM>(),
        //            TotalCount = 0,
        //            SeparatePaginationInfo = new SeparatePaginationInfo
        //            {
        //                StartItem = 0,
        //                EndItem = 0,
        //                TotalItems = 0,
        //                CurrentPage = pageNumber,
        //                TotalPages = 0,
        //                PageNumbers = new List<int>()
        //            }
        //        };

        //    // Fetch roster data for selected employees
        //    var rosterData = await baseQuery.Where(x => pagedEmployeeIds.Contains(x.EmployeeID)).ToListAsync();

        //    // Group in memory
        //    var grouped = rosterData.GroupBy(x => x.EmployeeID);

        //    var result = grouped.Select(g =>
        //    {
        //        var first = g.First();

        //        // Assigned dates
        //        var assignedDates = g.Select(x =>
        //            $"{x.DayDate:yyyy-MM-dd}|{x.ShiftName ?? "-"}|{(x.ShiftName != null ? $"{x.ShiftStartTime:hh\\:mm} - {x.ShiftEndTime:hh\\:mm}" : "-")}"
        //        );

        //        // Weekend numbers
        //        var weekendNumbers = g.SelectMany(x => x.WeekendSettings).Distinct();

        //        // Expand holidays
        //        var holidayEntries = g.SelectMany(x => x.Holidays)
        //            .SelectMany(h =>
        //            {
        //                var list = new List<(DateTime Date, string Title)>();
        //                for (var d = h.StartDate.Value; d <= h.EndDate.Value; d = d.AddDays(1))
        //                    if (d >= startDate && d < endDate)
        //                        list.Add((d, h.HolidayTitle));
        //                return list;
        //            }).ToList();

        //        // Expand leaves
        //        var leaveEntries = g.SelectMany(x => x.Leaves)
        //            .SelectMany(l =>
        //            {
        //                var list = new List<(DateTime Date, string LeaveInfo)>();
        //                var from = l.FromDate.ToDateTime(TimeOnly.MinValue);
        //                var to = l.ToDate.ToDateTime(TimeOnly.MinValue);

        //                for (var d = from; d <= to; d = d.AddDays(1))
        //                {
        //                    if (d >= startDate && d < endDate)
        //                    {
        //                        string leaveInfo;

        //                        if (l.IsFullDay)
        //                        {
        //                            leaveInfo = l.LeaveTypeName ?? "-";
        //                        }
        //                        else
        //                        {
        //                            // Include partial time for partial-day leave
        //                            var fromTime = l.PartialFromTime?.ToString("hh\\:mm") ?? "-";
        //                            var toTime = l.PartialToTime?.ToString("hh\\:mm") ?? "-";
        //                            leaveInfo = $"{l.LeaveTypeName ?? "-"} ({fromTime} - {toTime})";
        //                        }

        //                        list.Add((d, leaveInfo));
        //                    }
        //                }
        //                return list;
        //            }).OrderBy(l => l.Date)
        //            .ToList();

        //        // Make sure leaveEntries are ordered by Date
        //        //var orderedLeaveEntries = leaveEntries.OrderBy(l => l.Date).Distinct().ToList();

        //        // Map to LeaveDates and LeaveTypeName in same order
        //        var orderedLeaveDates = leaveEntries.Select(l => l.Date.ToString("yyyy-MM-dd"));
        //        var orderedLeaveTypes = leaveEntries.Select(l => l.LeaveInfo);

        //        return new EmployeeShiftViewSetupVM
        //        {
        //            RosterInOfficeDayID = first.RosterInOfficeDayID,
        //            OrganizationID = first.OrganizationID,
        //            OrganizationName = first.OrganizationName ?? "-",
        //            DepartmentID = first.DepartmentID,
        //            DepartmentName = first.DepartmentName ?? "-",
        //            EmployeeID = first.EmployeeID ?? 0,
        //            EmployeeName = $"{first.EmployeeFirstName} {first.EmployeeLastName} ({first.EmployeeCode})",
        //            ShiftID = first.ShiftID ?? 0,
        //            AssignedDates = string.Join(",", assignedDates),
        //            WeekdayNumber = string.Join(",", weekendNumbers),
        //            HolidayDates = string.Join(",", holidayEntries.Select(h => h.Date.ToString("yyyy-MM-dd")).Distinct()),
        //            HolidayTitle = string.Join(",", holidayEntries.Select(h => h.Title).Distinct()),
        //            //LeaveDates = string.Join(",", orderedLeaveEntries.Select(l => l.Date.ToString("yyyy-MM-dd"))),
        //            //LeaveTypeName = string.Join(",", orderedLeaveEntries.Select(l => l.LeaveTypeName ?? "-"))
        //            LeaveDates = string.Join(",", orderedLeaveDates),
        //            LeaveTypeName = string.Join(",", orderedLeaveTypes)
        //        };
        //    }).ToList();

        //    return new SeparatePaginationResult<EmployeeShiftViewSetupVM>
        //    {
        //        Data = result,
        //        TotalCount = totalCount,
        //        SeparatePaginationInfo = new SeparatePaginationInfo
        //        {
        //            StartItem = (pageNumber - 1) * pageSize + 1,
        //            EndItem = Math.Min(pageNumber * pageSize, totalCount),
        //            TotalItems = totalCount,
        //            CurrentPage = pageNumber,
        //            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        //            PageNumbers = Enumerable.Range(1, (int)Math.Ceiling(totalCount / (double)pageSize)).ToList()
        //        }
        //    };
        //}
        #endregion
    }
}
