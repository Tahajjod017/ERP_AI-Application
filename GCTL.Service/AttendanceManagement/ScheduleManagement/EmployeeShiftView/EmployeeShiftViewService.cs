using Dapper;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.EmployeeShiftView;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Mathematics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.EmployeeShiftView
{
    public class EmployeeShiftViewService : AppService<DefaultShifts>, IEmployeeShiftViewService
    {
        #region Repositories
        private readonly IGenericRepository<DefaultShifts> _genericRepository;
        private readonly IDbConnection _dbConnection;

        public EmployeeShiftViewService(IGenericRepository<DefaultShifts> genericRepository, IDbConnection dbConnection) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _dbConnection = dbConnection;
        }
        #endregion


        #region GetAllSp
        public async Task<PaginatedShiftResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", int daysToShow = 7, DateTime? startDate = null)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@pageNumber", pageNumber);
                parameters.Add("@pageSize", pageSize);
                parameters.Add("@searchTerm", searchTerm ?? string.Empty);
                parameters.Add("@daysToShow", daysToShow);
                parameters.Add("@startDate", startDate);

                using var multi = await _dbConnection.QueryMultipleAsync("sp_GetAllEmployeeShifts", parameters, commandType: CommandType.StoredProcedure);

                var items = await multi.ReadAsync<EmployeeShiftViewSetupVM>();
                var pagination = await multi.ReadFirstAsync<dynamic>();
                var holidays = await multi.ReadAsync<HolidayVM>();
                var leaves = await multi.ReadAsync<LeaveApplicationsVM>();

                return new PaginatedShiftResult
                {
                    Items = items,
                    TotalItems = pagination.TotalItems,
                    StartItem = pagination.StartItem,
                    EndItem = pagination.EndItem,
                    PageNumber = pagination.PageNumber,
                    CurrentPage = pagination.CurrentPage,
                    TotalPages = (int)pagination.TotalPages,
                    Holidays = holidays.ToList(),
                    LeaveApplications = leaves.ToList()
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing stored procedure", ex);
            }
        }
        #endregion
    }
}
