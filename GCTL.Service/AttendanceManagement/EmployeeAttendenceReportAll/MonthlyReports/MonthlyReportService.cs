using Dapper;
using GCTL.Core.Enums;
using GCTL.Core.Helpers.AttendenceHelper;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.AttendenceReportAlls;
using GCTL.Core.ViewModels.AttendanceManagement.EmployeeAttendence;
using GCTL.Core.ViewModels.MasterSetup.Statuses;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.AdminSettings.GeneralSettings;
using GCTL.Service.Employees.EmployeeBenifit;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Mvc;
using static GCTL.Service.AdminSettings.GeneralSettings.UtcTimeHelper;

namespace GCTL.Service.AttendanceManagement.EmployeeAttendenceReportAll.MonthlyReports
{
    public class MonthlyReportService:AppService<Attendance>, IMonthlyReportService
    {
        private readonly IUserInfoService _userInfoService;
        private readonly AppDbContext _context;
        private readonly IGenericRepository<Attendance> _genericRepository;
        private readonly IGenericRepository<Shifts> _genericRepositoryShift;
        private readonly HolidayHelper _holidayHelper;
        private readonly WeekendHelper _weekendHelper;
        private readonly LeaveHelper _leaveHelper;
        private readonly IGenericRepository<EmployeeOfficeInfo> _genericEmployeeOfficeInfo;
        private readonly ILocalizationContext _localizationContext;

        public MonthlyReportService(IUserInfoService userInfoService, IGenericRepository<Attendance> genericRepository, HolidayHelper holidayHelper, WeekendHelper weekendHelper, LeaveHelper leaveHelper, IGenericRepository<EmployeeOfficeInfo> genericEmployeeOfficeInfo, ILocalizationContext localizationContext, AppDbContext context) : base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepository = genericRepository;
            _holidayHelper = holidayHelper;
            _weekendHelper = weekendHelper;
            _leaveHelper = leaveHelper;
            _genericEmployeeOfficeInfo = genericEmployeeOfficeInfo;
            _localizationContext = localizationContext;
            _context = context;
        }

        //public async Task<SelectListItem> GetMonthAsync()
        //{
        //    // Get all months from the enum
        //    var months = Enum.GetValues(typeof(Month))
        //                     .Cast<Month>()
        //                     .Select(m => new SelectListItem
        //                     {
        //                         Text = m.ToString(), // Month name (e.g., January)
        //                         Value = ((int)m).ToString() // Month number (e.g., 1 for January)
        //                     })
        //                     .ToList();

        //    return months;
        //}
        public async Task<PaginationService<Attendance, EmployeeAttendanceMonthlyVM>.PaginationResult<EmployeeAttendanceMonthlyVM>>
           GetFilteredPaginatedEmployeeAttendanceAsync(
                                                  int organizationID,
                                                  int departmentID,
                                                  int month,
                                                  int year,
                                                  int pageNumber = 1,
                                                  int pageSize = 5,
                                                  string sortColumn = "EmployeeName",
                                                  string sortOrder = "asc")
        {
            using (var connection = new SqlConnection("_connectionString"))
            {
                var parameters = new
                {
                    OrganizationID = organizationID,
                    DepartmentID = departmentID,
                    Month = month,
                    Year = year,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    SortColumn = sortColumn,
                    SortOrder = sortOrder
                };

                // Execute the stored procedure to get filtered attendance data
                var result = await connection.QueryAsync<EmployeeAttendanceMonthlyVM>(
                    "GetFilteredPaginatedEmployeeAttendance",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                // Execute the stored procedure to get the total count for pagination
                var totalCount = await connection.QuerySingleAsync<int>(
                    "GetFilteredPaginatedEmployeeAttendance",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                var paginationResult = new PaginationService<Attendance, EmployeeAttendanceMonthlyVM>.PaginationResult<EmployeeAttendanceMonthlyVM>
                {
                    Data = result.ToList(),
                    //Pagination = new PaginationMetadata
                    //{
                    //    TotalItems = totalCount,
                    //    CurrentPage = pageNumber,
                    //    PageSize = pageSize,
                    //    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    //}
                };

                return paginationResult;
            }
        }

        public async Task<PaginationService<Attendance, AttendanceEmployeeReportVM>.PaginationResult<AttendanceEmployeeReportVM>> GetMonthlyAttendanceReport(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
                              string sortColumn = "HolidayID", string sortOrder = "desc", int? organizationID = null, List<int>? departmentIds = null, List<int>? employeeIds = null, string? monthyear = null, int? employeeId = null)
         {

            var query = _genericRepository.All()
                         .AsNoTracking()
                         .Include(x => x.Employee)
                         .Include(x => x.Employee.EmployeeOfficeInfoCreatedByNavigation)
                         .Include(x => x.Status)
                         .Include(x => x.Shift)
                         .Where(x => x.DeletedAt == null && x.EmployeeID == employeeId); // Filter by EmployeeID

            var employeeDepartments = await _genericEmployeeOfficeInfo.All()
                                         .Include(e => e.Department) // Ensure the Department is loaded
                                         .Where(e => e.DeletedAt == null)
                                         .Select(e => new
                                         {
                                             EmployeeID = e.EmployeeID,
                                             DepartmentName = e.Department != null && e.Department.DepartmentName != null
                                                              ? e.Department.DepartmentName
                                                              : "-"
                                         })
                                         .ToListAsync();


            var result = await PaginationService<Attendance, AttendanceEmployeeReportVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => x => EF.Functions.Like(x.Status.StatusName, $"%{term}%"),

                x => new AttendanceEmployeeReportVM
                {
                    AttendanceID = x.AttendanceID,

                    EmployeeID = x.EmployeeID,
                    EmployeeName = x.Employee?.FirstName + " " + x.Employee?.LastName ?? "-",
                    JobTitle = employeeDepartments.FirstOrDefault(e => e.EmployeeID == x.EmployeeID)?.DepartmentName ?? "-",
                    ShiftID = x.ShiftID,
                    ShiftName = x.Shift?.ShiftName ?? "-",
                    StatusID = x.StatusID,
                    StatusName = x.Status?.StatusName ?? "-",
                    AttendanceDate = x.AttendanceDate.ToString("yyyy-MM-dd") ?? "-",
                    CheckInTime = x.CheckInTime.HasValue ? TimeConversionHelper.ConvertUtcDateTimeToLocalHHmm(DateTime.SpecifyKind(x.CheckInTime.Value, DateTimeKind.Utc), _localizationContext) : "-",
                    CheckOutTime = x.CheckOutTime.HasValue ? TimeConversionHelper.ConvertUtcDateTimeToLocalHHmm(DateTime.SpecifyKind(x.CheckOutTime.Value, DateTimeKind.Utc), _localizationContext)  // Convert UTC to local
                                    : "-",
                    //LateHour = x.LateTimeMinutes.HasValue ? x.LateTimeMinutes.Value.ToString("F2") : "-",
                    LateHour = FormatTime(x.LateTimeMinutes),
                    //EarlyHour = x.EarlyTimeMinutes.HasValue ? x.EarlyTimeMinutes.Value.ToString("F2") : "-",
                    EarlyHour = FormatTime(x.EarlyTimeMinutes),
                    RegularHour = FormatTime(x.OfficeTimeMinutes),
                    OvertimeHour = FormatTime(x.OvertimeMinutes),
                    WorkingHours = FormatTime(x.WorkingTimeMinutes),
                    Break = FormatTime(x.BreakTimeMinutes),

                    CreatedBy = x.CreatedBy,
                    UpdatedBy = x.UpdatedBy
                });

            return result;
        }

        public async Task<MonthlyAttendanceCalendarVM> GetMonthlyAttendanceCalendarAsync(
        int employeeId,
        string monthYear)
        {
            if (string.IsNullOrWhiteSpace(monthYear))
                monthYear = DateTime.Now.ToString("yyyy-MM");

            var parts = monthYear.Split('-');
            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);

            var result = new MonthlyAttendanceCalendarVM
            {
                Month = monthYear
            };

            using var conn = _context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "dbo.usp_GetMonthlyAttendanceByEmployee";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            var pEmp = cmd.CreateParameter();
            pEmp.ParameterName = "@EmployeeID";
            pEmp.Value = employeeId;
            cmd.Parameters.Add(pEmp);

            var pYear = cmd.CreateParameter();
            pYear.ParameterName = "@Year";
            pYear.Value = year;
            cmd.Parameters.Add(pYear);

            var pMonth = cmd.CreateParameter();
            pMonth.ParameterName = "@Month";
            pMonth.Value = month;
            cmd.Parameters.Add(pMonth);

            using var reader = await cmd.ExecuteReaderAsync();

            // column indexes (to avoid multiple GetOrdinal calls)
            int idxDay = reader.GetOrdinal("DayNumber");
            int idxIn = reader.GetOrdinal("CheckInTime");
            int idxOut = reader.GetOrdinal("CheckOutTime");
            int idxBreak = reader.GetOrdinal("BreakTimeMinutes");
            int idxLate = reader.GetOrdinal("LateTimeMinutes");
            int idxEarly = reader.GetOrdinal("EarlyTimeMinutes");
            int idxOT = reader.GetOrdinal("OvertimeMinutes");
            int idxProd = reader.GetOrdinal("WorkingTimeMinutes");
            int idxStatus = reader.GetOrdinal("StatusName");

            while (await reader.ReadAsync())
            {
                var dayNumber = reader.GetInt32(idxDay);
                var dayKey = dayNumber.ToString("00"); // "01","02",...

                DateTime? checkIn = reader.IsDBNull(idxIn)
                    ? (DateTime?)null
                    : reader.GetDateTime(idxIn);

                DateTime? checkOut = reader.IsDBNull(idxOut)
                    ? (DateTime?)null
                    : reader.GetDateTime(idxOut);

                int? breakMin = reader.IsDBNull(idxBreak) ? (int?)null : reader.GetInt32(idxBreak);
                int? lateMin = reader.IsDBNull(idxLate) ? (int?)null : reader.GetInt32(idxLate);
                int? earlyMin = reader.IsDBNull(idxEarly) ? (int?)null : reader.GetInt32(idxEarly);
                int? otMin = reader.IsDBNull(idxOT) ? (int?)null : reader.GetInt32(idxOT);
                int? prodMin = reader.IsDBNull(idxProd) ? (int?)null : reader.GetInt32(idxProd);

                string? status = reader.IsDBNull(idxStatus) ? null : reader.GetString(idxStatus);

                string? inTime = null;
                if (checkIn.HasValue)
                {
                    var dt = DateTime.SpecifyKind(checkIn.Value, DateTimeKind.Utc);
                    inTime = TimeConversionHelper.ConvertUtcDateTimeToLocalHHmm(dt, _localizationContext);
                }

                string? outTime = null;
                if (checkOut.HasValue)
                {
                    var dt = DateTime.SpecifyKind(checkOut.Value, DateTimeKind.Utc);
                    outTime = TimeConversionHelper.ConvertUtcDateTimeToLocalHHmm(dt, _localizationContext);
                }

                var dayVm = new MonthlyAttendanceDayVM
                {
                    In = inTime,
                    Out = outTime,
                    Break = FormatMinutes(breakMin),
                    Late = FormatMinutes(lateMin),
                    Early = FormatMinutes(earlyMin),
                    Ot = FormatMinutes(otMin),
                    Prod = FormatMinutes(prodMin),
                    Status = status ?? "-"
                };

                // If multiple records in one day → last one wins (or you can decide merge here)
                result.Data[dayKey] = dayVm;
            }

            return result;
        }

        private string? FormatMinutes(int? minutes)
        {
            if (!minutes.HasValue) return null;

            if (minutes < 60)
                return $"{minutes} Min";

            double hours = minutes.Value / 60.0;
            return $"{hours:0.00} H";
        }
        private string FormatTime(int? minutes)
        {
            if (!minutes.HasValue)
                return "-";

            int hours = minutes.Value / 60;
            int remainingMinutes = minutes.Value % 60;

            return $"{hours:D2}:{remainingMinutes:D2}"; // Formats as "HH:mm"
        }
        // Helper method to get the special day label
        private string GetSpecialDayLabel(int day, bool isLeaveDay, bool isWeekend, bool isHoliday)
        {
            if (isLeaveDay)
            {
                return "Leave";
            }
            if (isWeekend)
            {
                return "Weekend";
            }
            if (isHoliday)
            {
                return "Holiday";
            }
            return "Absent";
        }

        //public async Task<IActionResult> GetYearlyAttendanceReport(int? departmentId, int? organizationId, int? employeeId, string year)
        //{

        //    var startDate = DateOnly.ParseExact(year + "-01-01", "yyyy-MM-dd", CultureInfo.InvariantCulture); // Start of the year
        //    var endDate = startDate.AddYears(1).AddDays(-1);  // End of the year (December 31)

        //    // Get the weekend days for the organization and branch
        //    var weekendWeekdays = _weekendHelper.GetWeekendWeekdayNumbers(organizationId.Value, null); // Pass the branch ID if needed

        //    // Get active holidays within the given date range
        //    var holidays = _holidayHelper.GetActiveHolidays(organizationId.Value, startDate.ToDateTime(new TimeOnly(0, 0)), endDate.ToDateTime(new TimeOnly(0, 0)));

        //    // Query the Attendance and Shifts tables with the filters
        //    var query = _genericRepository.All()
        //        .AsNoTracking()  // Disable change tracking for better performance since we are only reading data
        //        .Include(x => x.Employee)  // Include the related Employee data
        //        .Include(x => x.Status)    // Include the related Status data
        //        .Include(x => x.Shift)     // Include the related Shift data
        //        .Where(x => x.DeletedAt == null &&  // Filter for deleted records
        //                   (employeeId == null || x.EmployeeID == employeeId) &&
        //                   (organizationId == null || x.Shift.OrganizationID == organizationId) &&
        //                   //(departmentId == null || x.Employee.DepartmentID == departmentId) &&
        //                   x.AttendanceDate >= startDate && x.AttendanceDate <= endDate)
        //        .Select(x => new
        //        {
        //            x.AttendanceID,
        //            x.EmployeeID,
        //            x.AttendanceDate,
        //            x.StatusID,
        //            x.ShiftID,
        //            x.CheckInTime,
        //            x.CheckOutTime,
        //            x.Remarks,
        //            x.LIP,
        //            x.LMAC,
        //            x.RegularHour,
        //            x.OvertimeHour,
        //            x.LateHour,
        //            x.EarlyHour,
        //            x.WorkingHour,
        //            ShiftName = x.Shift.ShiftName,  // Access ShiftName from the Shift navigation property
        //            OrganizationID = x.Shift.OrganizationID // Access OrganizationID from Shift
        //        });

        //    var attendanceData = await query.ToListAsync();

        //    // Organize the result in a way that matches the format needed by the front-end
        //    var formattedData = attendanceData.Select(item => new
        //    {
        //        Date = item.AttendanceDate.ToString("dd-MM-yyyy"), // Include the full date with day, month, and year
        //        Shift = item.ShiftName,
        //        CheckInTime = item.CheckInTime?.ToString("hh:mm tt"),
        //        CheckOutTime = item.CheckOutTime?.ToString("hh:mm tt"),
        //        RegularHour = item.RegularHour,
        //        OvertimeHour = item.OvertimeHour,
        //        LateHour = item.LateHour,
        //        EarlyHour = item.EarlyHour,
        //        WorkingHour = item.WorkingHour,
        //        Remarks = item.Remarks,
        //        Status = item.StatusID, // You can map this to a specific status name if needed
        //                                // Check if the current AttendanceDate is a holiday or weekend
        //        SpecialDay = GetSpecialDayLabel(item.AttendanceDate, weekendWeekdays, holidays)
        //    }).ToList();

        //    // Returning the formatted data as a JSON response
        //    return new Microsoft.AspNetCore.Mvc.JsonResult(formattedData);
        //}

        //public async Task<byte[]> GenerateJobCardPdfAsync(int employeeId, string monthYear)
        //{
        //    // Parse yyyy-MM
        //    if (string.IsNullOrWhiteSpace(monthYear))
        //        monthYear = DateTime.Now.ToString("yyyy-MM");

        //    var parts = monthYear.Split('-');
        //    var year = int.Parse(parts[0]);
        //    var month = int.Parse(parts[1]);

        //    var fromDate = new DateTime(year, month, 1);
        //    var toDate = fromDate.AddMonths(1).AddDays(-1);

        //    // Load employee info
        //    var employee = await _employeeRepo.All()
        //        .Include(e => e.EmployeeOfficeInfoCreatedByNavigation)
        //        .FirstOrDefaultAsync(e => e.EmployeeID == employeeId);

        //    if (employee == null)
        //        throw new Exception("Employee not found.");

        //    var deptName = employee.EmployeeOfficeInfoCreatedByNavigation?.Department?.DepartmentName ?? "-";
        //    var designation = employee.EmployeeOfficeInfoCreatedByNavigation?.Designation ?? "-";

        //    // Load attendance for the month
        //    var records = await _genericRepository.All()
        //        .AsNoTracking()
        //        .Include(a => a.Status)
        //        .Include(a => a.Shift)
        //        .Where(a =>
        //            a.DeletedAt == null &&
        //            a.EmployeeID == employeeId &&
        //            a.AttendanceDate >= fromDate &&
        //            a.AttendanceDate <= toDate)
        //        .OrderBy(a => a.AttendanceDate)
        //        .ToListAsync();

        //    // Build rows (one per day from 1..end of month)
        //    var rows = new List<JobCardRowVm>();
        //    int daysInMonth = DateTime.DaysInMonth(year, month);

        //    for (int day = 1; day <= daysInMonth; day++)
        //    {
        //        var date = new DateTime(year, month, day);
        //        var rec = records.FirstOrDefault(r => r.AttendanceDate.Date == date.Date);

        //        var vm = new JobCardRowVm
        //        {
        //            Date = date,
        //            DayName = date.ToString("dddd"),
        //            ShiftName = rec?.Shift?.ShiftName ?? "",
        //            InTime = ToLocalTime(rec?.CheckInTime),
        //            OutTime = ToLocalTime(rec?.CheckOutTime),
        //            Late = FormatMinutes(rec?.LateTimeMinutes),
        //            EarlyOut = FormatMinutes(rec?.EarlyTimeMinutes),
        //            WorkHours = FormatMinutes(rec?.WorkingTimeMinutes, asHour: true),
        //            OvertimeHours = FormatMinutes(rec?.OvertimeMinutes, asHour: true),
        //            StatusCode = rec?.Status?.StatusName ?? "-",    // map to P/L/A/W/H etc. if needed
        //            Remarks = rec?.Remarks ?? ""
        //        };

        //        rows.Add(vm);
        //    }

        //    // Summary counters (simple version, adjust as needed)
        //    var totalPresent = rows.Count(r => r.StatusCode == "P");
        //    var totalLate = rows.Count(r => r.StatusCode == "L");
        //    var totalAbsent = rows.Count(r => r.StatusCode == "A");
        //    var totalWeekend = rows.Count(r => r.StatusCode == "W");
        //    var totalHoliday = rows.Count(r => r.StatusCode == "H");
        //    var totalLeave = rows.Count(r =>
        //        r.StatusCode == "CL" || r.StatusCode == "SL" || r.StatusCode == "EL" || r.StatusCode == "UL");

        //    var reportVm = new JobCardReportVm
        //    {
        //        EmployeeCode = employee.EmployeeCode ?? employee.EmployeeID.ToString(),
        //        EmployeeName = $"{employee.FirstName} {employee.LastName}",
        //        Designation = designation,
        //        Department = deptName,
        //        CompanyName = "GCTL Infosys",
        //        FromDate = fromDate,
        //        ToDate = toDate,
        //        Rows = rows,
        //        TotalPresent = totalPresent,
        //        TotalAbsent = totalAbsent,
        //        TotalLate = totalLate,
        //        TotalLeave = totalLeave,
        //        TotalHoliday = totalHoliday,
        //        TotalWeekend = totalWeekend,
        //        TotalAttendance = totalPresent + totalLate // you can refine
        //    };

        //    // Generate PDF with QuestPDF
        //    var pdfBytes = CreateJobCardPdf(reportVm);
        //    return pdfBytes;
        //}

        private byte[] CreateJobCardPdf(JobCardReportVm model)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Element(h =>
                    {
                        h.Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"Emp. ID : {model.EmployeeCode}");
                                    c.Item().Text($"Name : {model.EmployeeName}");
                                    c.Item().Text($"Designation : {model.Designation}");
                                });

                                row.RelativeItem().AlignRight().Column(c =>
                                {
                                    c.Item().Text(model.CompanyName).Bold().FontSize(14);
                                    c.Item().Text("Job Card Report").Bold();
                                    c.Item().Text(
                                        $"Date: {model.FromDate:dd/MM/yyyy} - {model.ToDate:dd/MM/yyyy}");
                                });
                            });

                            col.Item().Text($"Department : {model.Department}");
                            col.Item().LineHorizontal(1);
                        });
                    });

                    page.Content().Element(c =>
                    {
                        c.Table(table =>
                        {
                            // columns
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(65);   // Date
                                cols.ConstantColumn(60);   // Day
                                cols.ConstantColumn(50);   // Shift
                                cols.ConstantColumn(70);   // In Time
                                cols.ConstantColumn(60);   // Late
                                cols.ConstantColumn(70);   // Out Time
                                cols.ConstantColumn(60);   // Early Out
                                cols.ConstantColumn(75);   // W. Hour(s)
                                cols.ConstantColumn(50);   // OT(H)
                                cols.ConstantColumn(45);   // Status
                                cols.RelativeColumn();     // Remarks
                            });

                            // header
                            table.Header(h =>
                            {
                                h.Cell().Text("Date").Bold();
                                h.Cell().Text("Day").Bold();
                                h.Cell().Text("Shift").Bold();
                                h.Cell().Text("In Time").Bold();
                                h.Cell().Text("Late").Bold();
                                h.Cell().Text("Out Time").Bold();
                                h.Cell().Text("Early Out").Bold();
                                h.Cell().Text("W. Hour(s)").Bold();
                                h.Cell().Text("OT(H)").Bold();
                                h.Cell().Text("Status").Bold();
                                h.Cell().Text("Remarks").Bold();
                            });

                            // rows
                            foreach (var r in model.Rows)
                            {
                                table.Cell().Text(r.Date.ToString("dd/MM/yyyy"));
                                table.Cell().Text(r.DayName);
                                table.Cell().Text(r.ShiftName);
                                table.Cell().Text(r.InTime);
                                table.Cell().Text(r.Late);
                                table.Cell().Text(r.OutTime);
                                table.Cell().Text(r.EarlyOut);
                                table.Cell().Text(r.WorkHours);
                                table.Cell().Text(r.OvertimeHours);
                                table.Cell().Text(r.StatusCode);
                                table.Cell().Text(r.Remarks);
                            }
                        });
                    });

                    page.Footer().Element(f =>
                    {
                        f.Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"Total Present : {model.TotalPresent}");
                                    c.Item().Text($"Total Absent : {model.TotalAbsent}");
                                    c.Item().Text($"Total Late : {model.TotalLate}");
                                    c.Item().Text($"Total Leave : {model.TotalLeave}");
                                });

                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"Total Holiday: {model.TotalHoliday}");
                                    c.Item().Text($"Total Att.: {model.TotalAttendance}");
                                    c.Item().Text($"Total Weekend: {model.TotalWeekend}");
                                });
                            });

                            col.Item().Text(
                                "Status Legend: P-Present, L-Late, A-Absent, W-Weekend, H-Holiday, CL-Casual Leave, SL-Sick Leave, UL-Unpaid Leave, ML-Maternity Leave, PL-Paternity Leave, MRL-Marriage Leave, EL-Earn Leave, SPL-Special Leave")
                                .FontSize(7);

                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Prepared by");
                                row.RelativeItem().Text("Checked by");
                                row.RelativeItem().Text("Authorized by");
                            });

                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"Print Datetime: {DateTime.Now:dd/MM/yyyy hh:mm:ss tt}");
                                row.RelativeItem().AlignRight().Text(text =>
                                {
                                    text.Span("Page ");
                                    text.CurrentPageNumber();
                                    text.Span(" of ");
                                    text.TotalPages();
                                });
                            });
                        });
                    });
                });
            });

            return doc.GeneratePdf();
        }

    }
}
