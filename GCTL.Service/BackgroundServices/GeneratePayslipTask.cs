using GCTL.Core.Helpers;
using GCTL.Core.Helpers.LipLmacAddress;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpSalary;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.PayRollManagements.PayRollEmpSalary;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.BackgroundServices
{

    public class GeneratePayslipTask : IBackgroundTask, ITaskWithDate
    {
        private readonly ILogger<GeneratePayslipTask> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        
        public GeneratePayslipTask(
            ILogger<GeneratePayslipTask> logger,
            IServiceScopeFactory scopeFactory
            )
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            
        }

        public string Name => "GenerateAllPayslipsTask";

        public DateTime? RunOnDate => new DateTime(2025, 10, 11);
        public TimeSpan ScheduledTime => new TimeSpan(18,11, 0);

        //public async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    _logger.LogInformation("Executing GenerateAllPayslipsTask...");

        //    using var scope = _scopeFactory.CreateScope();
        //    var payRollEmpSalaryService = scope.ServiceProvider.GetRequiredService<IPayRollEmpSalaryService>();
        //    var employeeSalarySettingsRepository = scope.ServiceProvider.GetRequiredService<IGenericRepository<EmployeeSalarySettings>>();
        //    // ✅ Get all employees and salaries at once
        //    var employeeSalaries = await employeeSalarySettingsRepository.AllActive()
        //        .Select(x => new { x.EmployeeID, x.Salary })
        //        .ToListAsync();

        //    var tasks = employeeSalaries.Select(async emp =>
        //    {
        //        if (stoppingToken.IsCancellationRequested)
        //            return;

        //        try
        //        {
        //            decimal basicSalary = (decimal)emp.Salary;

        //            var model = new PayRollEmpSalarySaveVM
        //            {
        //                EmployeeID = emp.EmployeeID,
        //                PayPeriodStart = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, 1),
        //                PayPeriodEnd = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)),
        //                IsPaid = false,
        //                LIP = NetworkHelper.GetLocalIP(),
        //                LMAC = NetworkHelper.GetMacAddress(),
        //                CreatedBy = emp.EmployeeID,
        //                BasicSalary = basicSalary
        //            };

        //            var result = await payRollEmpSalaryService.SaveAsync(model);

        //            if (result.Success && result.Data != null)
        //            {
        //                int payslipId = ((PaySlips)result.Data).PaySlipID;
        //                var pdfBytes = await payRollEmpSalaryService.GeneratePdf(payslipId);

        //                string folderPath = Path.Combine("D:\\Payslips", $"Payslip_{DateTime.Now.Year}");
        //                Directory.CreateDirectory(folderPath);

        //                string filePath = Path.Combine(folderPath, $"PaySlip_{payslipId}.pdf");
        //                await File.WriteAllBytesAsync(filePath, pdfBytes);

        //                _logger.LogInformation($"Payslip generated for EmployeeID={emp.EmployeeID}");
        //            }
        //            else
        //            {
        //                _logger.LogError($"Failed to generate payslip for EmployeeID={emp.EmployeeID}: {result.Message}");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, $"Error generating payslip for EmployeeID={emp.EmployeeID}");
        //            //await userInfoService.ActionLogExceptionAsync("Pay Slip according generated from system", ex, emp.EmployeeID, ActionName.Error);
        //        }
        //    });

        //    // ✅ Run all tasks in parallel
        //    await Task.WhenAll(tasks);

        //    _logger.LogInformation("GenerateAllPayslipsTask completed successfully.");
        //}

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Executing GenerateAllPayslipsTask...");

            using var scope = _scopeFactory.CreateScope();
            var payRollEmpSalaryService = scope.ServiceProvider.GetRequiredService<IPayRollEmpSalaryService>();
            var employeeSalarySettingsRepository = scope.ServiceProvider.GetRequiredService<IGenericRepository<EmployeeSalarySettings>>();
            var userInfoService = scope.ServiceProvider.GetRequiredService<IUserInfoService>();
            // ✅ Get all employees and salaries at once
            var employeeSalaries = await employeeSalarySettingsRepository.AllActive()
                .Select(x => new { x.EmployeeID, x.Salary }).ToListAsync();

            // ✅ Create all PayRollEmpSalarySaveVM objects first
            var models = employeeSalaries.Select(emp => new PayRollEmpSalarySaveVM
            {
                EmployeeID = emp.EmployeeID,
                PayPeriodStart = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, 1),
                PayPeriodEnd = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)),
                IsPaid = false,
                LIP = NetworkHelper.GetLocalIP(),
                LMAC = NetworkHelper.GetMacAddress(),
                CreatedBy = emp.EmployeeID,
                BasicSalary = (decimal)emp.Salary
            }).ToList();

            // ✅ Save all payslips in bulk
            var saveResult = await payRollEmpSalaryService.SaveBulkAsync(models);

            if (!saveResult.Success || saveResult.Data == null)
            {
                _logger.LogError($"Failed to save payslips: {saveResult.Message}");
                return;
            }

            var savedPayslips = (List<PaySlips>)saveResult.Data;

            //// ✅ Generate PDFs in parallel
            //var pdfTasks = savedPayslips.Select(async ps =>
            //{
            //    if (stoppingToken.IsCancellationRequested)
            //        return;

            //    try
            //    {
            //        var pdfBytes = await payRollEmpSalaryService.GeneratePdf(ps.PaySlipID);
            //        string folderPath = Path.Combine("D:\\Payslips", $"Payslip_{DateTime.Now.Year}");
            //        Directory.CreateDirectory(folderPath);

            //        string filePath = Path.Combine(folderPath, $"PaySlip_{ps.PaySlipID}.pdf");
            //        await File.WriteAllBytesAsync(filePath, pdfBytes);

            //        _logger.LogInformation($"Payslip PDF generated for EmployeeID={ps.EmployeeID}");
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex, $"Error generating PDF for EmployeeID={ps.EmployeeID}");
            //        await userInfoService.ActionLogExceptionAsync("Pay Slip PDF generation", ex, ps.EmployeeID, ActionName.Error);
            //    }
            //});

            //await Task.WhenAll(pdfTasks);

            foreach (var ps in savedPayslips)
            {
                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    var pdfBytes = await payRollEmpSalaryService.GeneratePdf(ps.PaySlipID);
                    string folderPath = Path.Combine("D:\\Payslips", $"Payslip_{DateTime.Now.Year}");
                    Directory.CreateDirectory(folderPath);

                    string filePath = Path.Combine(folderPath, $"PaySlip_{ps.PaySlipID}.pdf");
                    await File.WriteAllBytesAsync(filePath, pdfBytes);

                    _logger.LogInformation($"Payslip PDF generated for EmployeeID={ps.EmployeeID}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error generating PDF for EmployeeID={ps.EmployeeID}");
                    await userInfoService.ActionLogExceptionAsync("Pay Slip PDF generation", ex, ps.EmployeeID, ActionName.Error);
                }
            }


            _logger.LogInformation("GenerateAllPayslipsTask completed successfully.");
        }


    }


}
