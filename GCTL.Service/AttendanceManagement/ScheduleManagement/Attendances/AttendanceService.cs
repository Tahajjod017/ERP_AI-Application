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
    public class AttendanceService : AppService<Attendance>, IAttendanceService
    {
        #region Repositories
        private readonly IGenericRepository<Attendance> _genericRepository;
        private readonly IDbConnection _dbConnection;

        public AttendanceService(IGenericRepository<Attendance> genericRepository, IDbConnection dbConnection) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _dbConnection = dbConnection;
        }
        #endregion


        #region AttendanceFromApps
        public async Task<bool> AttendanceFromApps(PunchDataRequestVM model)
        {
            try
            {
                if (model == null)
                    return false;

                var parameters = new DynamicParameters();
                parameters.Add("@enroll_id", model.EmployeeId);
                parameters.Add("@CHECKTIME", model.CheckInTime);
                parameters.Add("@DeviceSN", model.DeviceInfo);
                parameters.Add("@SourceType", model.SourceType);

                var result = await _dbConnection.ExecuteAsync("[HRM_DB_GCTL].[dbo].[sp_ProcessPunch]", parameters, commandType: CommandType.StoredProcedure);

                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing attendance: {ex.Message}", ex);
            }
        }
        #endregion
    }
}
