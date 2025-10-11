using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpSalary;
using GCTL.Service.PayRollManagements.PayRollEmpSalary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.BackgroundServices
{

    public class GeneratePayslipTask : IBackgroundTask
    {
        private readonly ILogger<GeneratePayslipTask> _logger;

        public GeneratePayslipTask(ILogger<GeneratePayslipTask> logger)
        {
            _logger = logger;
        }

        public string Name => "GeneratePayslipTask";

        //public TimeSpan ScheduledTime => new TimeSpan(12, 33, 0); // 11:00 PM
        public TimeSpan ScheduledTime => DateTime.Now.TimeOfDay; // Run immediately
        public Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Executing Task 1...");
            // Your task logic here
            return Task.CompletedTask;
        }
    }

    
}
