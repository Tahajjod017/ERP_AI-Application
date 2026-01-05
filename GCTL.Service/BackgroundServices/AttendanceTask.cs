using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.BackgroundServices
{
    public class AttendanceTask : IBackgroundTask
    {
        private readonly ILogger<AttendanceTask> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public AttendanceTask(
            ILogger<AttendanceTask> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public string Name => "AttendanceTask";

        // 09:00 PM Bangladesh time
        public TimeSpan ScheduledTime => new TimeSpan(21, 00, 0);

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Attendance Task started.");

            var tz = TimeZoneInfo.FindSystemTimeZoneById("Bangladesh Standard Time");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
                    var today = now.Date;

                    // 🔹 Create scope ONLY for DB access
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<IDbConnection>();

                        var lastRunDate = await db.ExecuteScalarAsync<DateTime?>(
                            "SELECT LastRunDate FROM ScheduledTaskHistory WHERE TaskName = @TaskName",
                            new { TaskName = Name });

                        var hasRunToday = lastRunDate == today;

                        if (!hasRunToday && now.TimeOfDay >= ScheduledTime)
                        {
                            _logger.LogInformation("Running Attendance Task immediately.");
                            await RunStoredProcedureAsync(db, today);
                            continue; // ✅ prevent double run
                        }
                    }

                    // 🔹 Schedule next run
                    var nextRun = today.Add(ScheduledTime);
                    if (now >= nextRun)
                        nextRun = nextRun.AddDays(1);

                    var delay = nextRun - now;
                    if (delay < TimeSpan.Zero)
                        delay = TimeSpan.Zero;

                    _logger.LogInformation($"Next Attendance Task scheduled at {nextRun}.");
                    await Task.Delay(delay, stoppingToken);

                    if (stoppingToken.IsCancellationRequested)
                        break;

                    // 🔹 New scope for execution
                    using var runScope = _scopeFactory.CreateScope();
                    var runDb = runScope.ServiceProvider.GetRequiredService<IDbConnection>();
                    await RunStoredProcedureAsync(runDb, nextRun.Date);
                }
                catch (TaskCanceledException)
                {
                    // graceful shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Attendance Task execution failed.");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }


        private async Task RunStoredProcedureAsync(IDbConnection db, DateTime runDate)
        {
            if (db.State != ConnectionState.Open)
                db.Open();

            using var tx = db.BeginTransaction();

            try
            {
                await db.ExecuteAsync(
                    "dbo.sp_processFinalAtt",
                    commandType: CommandType.StoredProcedure,
                    transaction: tx);

                await db.ExecuteAsync(
                    @"MERGE ScheduledTaskHistory AS target
              USING (SELECT @TaskName AS TaskName) AS source
              ON target.TaskName = source.TaskName
              WHEN MATCHED THEN
                  UPDATE SET
                      LastRunDate = @RunDate,
                      LastRunUtc = SYSUTCDATETIME(),
                      LastStatus = 'Success',
                      LastError = NULL
              WHEN NOT MATCHED THEN
                  INSERT (TaskName, LastRunDate, LastRunUtc, LastStatus)
                  VALUES (@TaskName, @RunDate, SYSUTCDATETIME(), 'Success');",
                    new { TaskName = Name, RunDate = runDate },
                    transaction: tx);

                tx.Commit();
            }
            catch (Exception ex)
            {
                tx.Rollback();

                await db.ExecuteAsync(
                    @"UPDATE ScheduledTaskHistory
              SET
                LastRunUtc = SYSUTCDATETIME(),
                LastStatus = 'Failed',
                LastError = @Error
              WHERE TaskName = @TaskName",
                    new { TaskName = Name, Error = ex.Message });

                throw;
            }
        }
    }


    //public class AttendanceTask : IBackgroundTask
    //{
    //    private readonly ILogger<AttendanceTask> _logger;
    //    private readonly IServiceScopeFactory _scopeFactory;

    //    public AttendanceTask(ILogger<AttendanceTask> logger, IServiceScopeFactory scopeFactory)
    //    {
    //        _logger = logger;
    //        _scopeFactory = scopeFactory;
    //    }

    //    public string Name => "AttendanceTask";

    //    public TimeSpan ScheduledTime => new TimeSpan(15, 53, 0); // 11:00 PM

    //    #region ExecuteAsync
    //    //public async Task ExecuteAsync(CancellationToken stoppingToken)
    //    //{
    //    //    try
    //    //    {
    //    //        _logger.LogInformation("Attendance Task started...");

    //    //        while (!stoppingToken.IsCancellationRequested)
    //    //        {
    //    //            var now = DateTime.Now;
    //    //            var scheduledTime = DateTime.Today.Add(ScheduledTime);
    //    //            var bufferTime = scheduledTime.AddMinutes(2);

    //    //            // If the scheduled time for today is in the past (including buffer time), schedule for tomorrow
    //    //            if (now > bufferTime)
    //    //            {
    //    //                scheduledTime = scheduledTime.AddDays(1);
    //    //            }

    //    //            var timeUntilExecution = bufferTime - now;

    //    //            // If timeUntilExecution is negative (we missed the scheduled time by more than the buffer), adjust to the next scheduled time
    //    //            if (timeUntilExecution < TimeSpan.Zero)
    //    //            {
    //    //                _logger.LogWarning($"Scheduled time has already passed for today. Adjusting to {scheduledTime.AddDays(1)}.");
    //    //                scheduledTime = scheduledTime.AddDays(1); // Set to the next day's scheduled time
    //    //                timeUntilExecution = scheduledTime - now; // Recalculate the time to the next scheduled time
    //    //            }

    //    //            // Log the delay duration
    //    //            _logger.LogInformation($"Waiting {timeUntilExecution.TotalMinutes} minutes to execute the task at {scheduledTime}...");

    //    //            // Ensure the delay is non-negative
    //    //            await Task.Delay(timeUntilExecution, stoppingToken);

    //    //            if (stoppingToken.IsCancellationRequested)
    //    //                break;

    //    //            // Execute the stored procedure
    //    //            await RunStoredProcedureAsync(stoppingToken);
    //    //        }
    //    //    }
    //    //    catch (Exception ex)
    //    //    {
    //    //        _logger.LogError(ex, "Error occurred during the execution of the Attendance Task.");
    //    //        throw;
    //    //    }
    //    //}
    //    public async Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        _logger.LogInformation("Attendance Task started.");

    //        while (!stoppingToken.IsCancellationRequested)
    //        {
    //            try
    //            {
    //                var tz = TimeZoneInfo.FindSystemTimeZoneById("Bangladesh Standard Time");
    //                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

    //                var nextRun = now.Date.Add(ScheduledTime);

    //                if (now >= nextRun)
    //                    nextRun = nextRun.AddDays(1);

    //                var delay = nextRun - now;

    //                _logger.LogInformation($"Next Attendance Task scheduled at {nextRun}.");

    //                await Task.Delay(delay, stoppingToken);

    //                if (stoppingToken.IsCancellationRequested)
    //                    break;

    //                await RunStoredProcedureAsync(stoppingToken);

    //                // Ensure exactly once per day
    //                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
    //            }
    //            catch (TaskCanceledException)
    //            {
    //                // Graceful shutdown
    //            }
    //            catch (Exception ex)
    //            {
    //                _logger.LogError(ex, "Attendance Task execution failed.");
    //            }
    //        }
    //    }

    //    #endregion


    //    #region RunStoredProcedureAsync
    //    //private async Task RunStoredProcedureAsync(CancellationToken stoppingToken)
    //    //{
    //    //    try
    //    //    {
    //    //        _logger.LogInformation("Executing stored procedure dbo.sp_processFinalAtt...");

    //    //        // Create a new scope to get the DbConnection instance
    //    //        using (var scope = _scopeFactory.CreateScope())
    //    //        {
    //    //            var dbConnection = scope.ServiceProvider.GetRequiredService<IDbConnection>();

    //    //            // Open the connection if it's not open
    //    //            if (dbConnection.State != ConnectionState.Open)
    //    //            {
    //    //                dbConnection.Open();
    //    //            }

    //    //            // Execute the stored procedure using Dapper
    //    //            await dbConnection.ExecuteAsync("dbo.sp_processFinalAtt", commandType: CommandType.StoredProcedure);

    //    //            _logger.LogInformation("Stored procedure dbo.sp_processFinalAtt executed successfully.");
    //    //        }
    //    //    }
    //    //    catch (Exception ex)
    //    //    {
    //    //        _logger.LogError(ex, "Error executing stored procedure dbo.sp_processFinalAtt.");
    //    //    }
    //    //}
    //    private async Task RunStoredProcedureAsync(CancellationToken stoppingToken)
    //    {
    //        _logger.LogInformation("Executing dbo.sp_processFinalAtt...");

    //        using var scope = _scopeFactory.CreateScope();
    //        var dbConnection = scope.ServiceProvider.GetRequiredService<IDbConnection>();

    //        if (dbConnection.State != ConnectionState.Open)
    //            dbConnection.Open();

    //        await dbConnection.ExecuteAsync(
    //            "dbo.sp_processFinalAtt",
    //            commandType: CommandType.StoredProcedure
    //        );

    //        _logger.LogInformation("dbo.sp_processFinalAtt executed successfully.");
    //    }

    //    #endregion
    //}
}
