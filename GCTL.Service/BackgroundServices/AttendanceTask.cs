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

        public AttendanceTask(ILogger<AttendanceTask> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public string Name => "AttendanceTask";

        public TimeSpan ScheduledTime => new TimeSpan(23, 0, 0); // 11:00 PM

        #region ExecuteAsync
        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Attendance Task started...");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var scheduledTime = DateTime.Today.Add(ScheduledTime);

                // If the scheduled time for today is in the past, schedule for tomorrow
                if (now > scheduledTime)
                {
                    scheduledTime = scheduledTime.AddDays(1);
                }

                var timeUntilExecution = scheduledTime - now;

                // Wait until the scheduled time
                _logger.LogInformation($"Waiting {timeUntilExecution.TotalMinutes} minutes to execute the task at {scheduledTime}...");
                await Task.Delay(timeUntilExecution, stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                    break;

                // Execute the stored procedure
                await RunStoredProcedureAsync(stoppingToken);
            }
        }
        #endregion


        #region RunStoredProcedureAsync
        private async Task RunStoredProcedureAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Executing stored procedure dbo.sp_processFinalAtt...");

                // Create a new scope to get the DbConnection instance
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbConnection = scope.ServiceProvider.GetRequiredService<IDbConnection>();

                    // Open the connection if it's not open
                    if (dbConnection.State != ConnectionState.Open)
                    {
                        dbConnection.Open();
                    }

                    // Execute the stored procedure using Dapper
                    await dbConnection.ExecuteAsync("dbo.sp_processFinalAtt", commandType: CommandType.StoredProcedure);

                    _logger.LogInformation("Stored procedure dbo.sp_processFinalAtt executed successfully.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing stored procedure dbo.sp_processFinalAtt.");
            }
        }
        #endregion
    }
}
