using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.BackgroundServices
{
    public class AttendanceTask : IBackgroundTask
    {
        private readonly ILogger<AttendanceTask> _logger;

        public AttendanceTask(ILogger<AttendanceTask> logger)
        {
            _logger = logger;
        }

        public string Name => "AttendanceTask";

        public TimeSpan ScheduledTime => new TimeSpan(13, 5, 0); // 11:00 PM

        public Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Executing Task 1...");
            // Your task logic here
            return Task.CompletedTask;
        }
    }
}
