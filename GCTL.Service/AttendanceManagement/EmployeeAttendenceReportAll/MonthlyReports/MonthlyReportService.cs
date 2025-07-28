using Dapper;
using GCTL.Core.Enums;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.AttendenceReportAlls;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Mvc;

namespace GCTL.Service.AttendanceManagement.EmployeeAttendenceReportAll.MonthlyReports
{
    public class MonthlyReportService:AppService<Attendance>
    {
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Attendance> _genericRepository;
        private readonly IGenericRepository<Shifts> _genericRepositoryShift;

        public MonthlyReportService(IUserInfoService userInfoService, IGenericRepository<Attendance> genericRepository) : base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepository = genericRepository;
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




    }
}
