using Dapper;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.APIViewModels;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
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


        #region GetTodaysAttendance
        public async Task<List<PunchResultVM>> GetTodaysMovement(int empId)
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);

                var attendances = await _genericRepository.All()
                    .Include(x => x.Employee)
                    .Include(x => x.AttendanceLog)
                    .Where(x => Convert.ToInt32(x.Employee.EmployeeCode) == empId
                                && x.AttendanceDate == today)
                    .Select(x => new
                    {
                        x.AttendanceID,
                        x.AttendanceLog
                    }).ToListAsync();

                var result = attendances.Select(att =>
                {
                    var orderedPunches = att.AttendanceLog
                        .OrderBy(p => p.PunchTime) // earliest first
                        .ToList();

                    var attendenceList = new List<AttendenceListVM>();
                    bool isIn = true;

                    for (int i = 0; i < orderedPunches.Count; i++)
                    {
                        attendenceList.Add(new AttendenceListVM
                        {
                            SlNo = i + 1,
                            AttendenceType = isIn ? "IN" : "OUT",
                            PunchTime = orderedPunches[i].PunchTime
                        });

                        isIn = !isIn;
                    }

                    // Reverse the list so the last punch comes first
                    attendenceList.Reverse();

                    // Determine InTime/OutTime flags based on last punch
                    int inTimeFlag = 0;
                    int outTimeFlag = 0;

                    if (orderedPunches.Any())
                    {
                        if (!isIn) // last punch was IN
                            inTimeFlag = 1;
                        else       // last punch was OUT
                            outTimeFlag = 1;
                    }

                    return new PunchResultVM
                    {
                        InTime = inTimeFlag,
                        OutTime = outTimeFlag,
                        AttendenceListVMs = attendenceList
                    };
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing attendance: {ex.Message}", ex);
            }
        }
        #endregion
    }
}
