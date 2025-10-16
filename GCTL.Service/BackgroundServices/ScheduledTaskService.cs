using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.BackgroundServices
{
    public class ScheduledTaskService : BackgroundService
    {
        private readonly ILogger<ScheduledTaskService> _logger;
        private readonly IEnumerable<IBackgroundTask> _tasks;
        private readonly Dictionary<string, DateTime> _lastRunTimes = new();

        public ScheduledTaskService(
            ILogger<ScheduledTaskService> logger,
            IEnumerable<IBackgroundTask> tasks)
        {
            _logger = logger;
            _tasks = tasks;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ScheduledTaskRunnerService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                foreach (var task in _tasks)
                {
                    // Has it run today?
                    if (_lastRunTimes.TryGetValue(task.Name, out var lastRun))
                    {
                        // Already ran today
                        if (lastRun.Date == now.Date)
                            continue;
                    }

                    // Is it time to run?
                    if (now.TimeOfDay >= task.ScheduledTime)
                    {
                        try
                        {
                            _logger.LogInformation($"Running scheduled task: {task.Name} at {now}");
                            await task.ExecuteAsync(stoppingToken);
                            _lastRunTimes[task.Name] = now;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error executing task: {task.Name}");
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // check every minute
            }

            _logger.LogInformation("ScheduledTaskRunnerService stopping.");
        }
    }
}
