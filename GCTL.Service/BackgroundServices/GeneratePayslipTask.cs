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
        public TimeSpan ScheduledTime => new TimeSpan(17,5, 0);

        public DateTime? RunOnDate
        {
            get
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Get all active organization IDs
                var organizationIds = db.PSettings.Select(p => p.OrganizationID).Distinct().ToList();

                var now = DateTime.Now;

                foreach (var organizationId in organizationIds)
                {
                    var psettings = db.PSettings.FirstOrDefault(p => p.OrganizationID == organizationId);

                    if (psettings?.SalaryDay != null)
                    {
                        var day = psettings.SalaryDay.Value;
                        var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                        if (day > daysInMonth) day = (byte)daysInMonth;

                        var runDate = new DateTime(now.Year, now.Month, day);
                        if (now.Date == runDate.Date)
                        {
                            return runDate;
                        }
                    }
                }

                return null;
            }
        }


        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting GeneratePayslipTask for all organizations...");

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // 🔹 Get all active organizations from PSettings
            var organizationSettings = await db.PSettings.Select(p => new { p.OrganizationID, p.SalaryDay }).ToListAsync();

            var today = DateTime.Now.Day;

            foreach (var setting in organizationSettings)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                // ✅ Check if today matches SalaryDay
                if (setting.SalaryDay == today)
                {
                    _logger.LogInformation("Running payslip generation for OrganizationID={OrgId} (SalaryDay={Day})", setting.OrganizationID, setting.SalaryDay);
                    await ProcessOrganizationAsync((int)setting.OrganizationID, scope.ServiceProvider, stoppingToken);
                }
                else
                {
                    _logger.LogInformation("Skipping OrganizationID={OrgId}, today ({Today}) ≠ SalaryDay ({Day})", setting.OrganizationID, today, setting.SalaryDay);
                }
            }

            _logger.LogInformation("GeneratePayslipTask finished for all organizations.");
        }


        private async Task ProcessOrganizationAsync(int organizationId, IServiceProvider services, CancellationToken stoppingToken)
        {
            using var scope = services.CreateScope();

            var payRollEmpSalaryService = scope.ServiceProvider.GetRequiredService<IPayRollEmpSalaryService>();
            var employeeSalarySettingsRepository = scope.ServiceProvider.GetRequiredService<IGenericRepository<EmployeeSalarySettings>>();
            var employeeOfficeInfoRepository = scope.ServiceProvider.GetRequiredService<IGenericRepository<EmployeeOfficeInfo>>();
            var psettingsRepository = scope.ServiceProvider.GetRequiredService<IGenericRepository<PSettings>>();
            var userInfoService = scope.ServiceProvider.GetRequiredService<IUserInfoService>();
            var psettings = await psettingsRepository.AllActive().FirstOrDefaultAsync(p => p.OrganizationID == organizationId);

            if (psettings == null)
            {
                _logger.LogWarning("No payroll settings found for OrganizationID={OrgId}", organizationId);
                return;
            }

            var employeeIds = await employeeOfficeInfoRepository.AllActive()
                .Where(eo => eo.OrganizationID == organizationId).Select(eo => eo.EmployeeID).ToListAsync();

            if (!employeeIds.Any())
            {
                _logger.LogWarning("No employees found for OrganizationID={OrgId}", organizationId);
                return;
            }

            var employeeSalaries = await employeeSalarySettingsRepository.AllActive()
                .Where(es => es.EmployeeID.HasValue && employeeIds.Contains(es.EmployeeID.Value)).Select(es => new { es.EmployeeID, es.Salary }).ToListAsync();

            if (!employeeSalaries.Any())
            {
                _logger.LogWarning("No salary settings found for OrganizationID={OrgId}", organizationId);
                return;
            }

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

            var saveResult = await payRollEmpSalaryService.SaveBulkAsync(models);

            if (!saveResult.Success || saveResult.Data == null)
            {
                _logger.LogError($"Failed to save payslips: {saveResult.Message}");
                return;
            }

            var savedPayslips = (List<PaySlips>)saveResult.Data;

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

            _logger.LogInformation("GenerateAllPayslipsTask completed for OrganizationID={OrgId}.", organizationId);
        }
    }



}
