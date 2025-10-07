using Dapper;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.APIViewModels;
using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.Attendances
{
    public class AppsAttendanceService : AppService<Attendance>, IAppsAttendanceService
    {
        #region Repositories
        private readonly IGenericRepository<Attendance> _genericRepository;
        private readonly IDbConnection _dbConnection;

        public AppsAttendanceService(IGenericRepository<Attendance> genericRepository, IDbConnection dbConnection) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _dbConnection = dbConnection;
        }
        #endregion


        #region AttendanceFromApps
        public async Task<PunchResultVM> AttendanceFromApps(PunchDataRequestVM model)
        {
            try
            {
                if (model == null)
                    return null;

                var parameters = new DynamicParameters();
                parameters.Add("@enroll_id", model.EmployeeId);
                parameters.Add("@CHECKTIME", DateTime.UtcNow);
                parameters.Add("@DeviceSN", model.DeviceInfo);
                parameters.Add("@SourceType", "Apps");
                parameters.Add("@Latitude", model.Latitude);
                parameters.Add("@Longitude", model.Longitude);

                //var result = await _dbConnection.ExecuteAsync("[HRM_DB_GCTL].[dbo].[sp_ProcessPunch]", parameters, commandType: CommandType.StoredProcedure);

                //return result > 0;
                //var result = await _dbConnection.QuerySingleOrDefaultAsync<PunchResultVM>("[HRM_DB_GCTL].[dbo].[sp_ProcessPunch]", parameters, commandType: CommandType.StoredProcedure);

                //return result;

                using var multi = await _dbConnection.QueryMultipleAsync("[HRM_DB_GCTL].[dbo].[sp_ProcessPunch]", parameters, commandType: CommandType.StoredProcedure );

                var punchResult = await multi.ReadFirstOrDefaultAsync<PunchResultVM>();
                if (punchResult == null) return null;

                var attendanceList = (await multi.ReadAsync<AttendenceListVM>()).ToList();
                punchResult.AttendenceListVMs = attendanceList;

                return punchResult;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing attendance: {ex.Message}", ex);
            }
        }
        #endregion
    }
}
