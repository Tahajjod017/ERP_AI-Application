using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeEducational;
using GCTL.Core.ViewModels.Employee.EmployeeFamily;
using GCTL.Core.ViewModels.Employee.EmployeeTraining;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmployeeUpdateVM;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpAllowance;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpSalary;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Employees.EmployeeBenifit;
using GCTL.Service.FileHandler;
using GCTL.Service.Pagination;
using GCTL.Service.PayRollManagements.PayRollEmpAllowance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Index.HPRtree;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;
using static QuestPDF.Helpers.Colors;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GCTL.Service.PayRollManagements.PayRollEmpSalary
{
    [Authorize]
    public class PayRollEmploSalaryService : IPayRollEmpSalaryService
    {
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOfficeInfoRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> employee;
        private readonly IGenericRepository<EmployeeSalarySettings> employeeSalarySettingsRepository;
        private readonly IGenericRepository<EmployeeAllowances> employeeAllowancesRepository; 
        private readonly IGenericRepository<EmployeeBaseAllowances> _employeeBaseAllowancesRepository;
        private readonly IGenericRepository<Benefits> benefitsRepository;
        private readonly IGenericRepository<BenefitSetups> benefitSetupsRepository;
        private readonly IGenericRepository<BenefitTypes> benefitType;
        private readonly IGenericRepository<EmployeeBaseBenefits> _employeeBaseBenefitsRepository;
        private readonly IUserInfoService userInfoService;
        private readonly IGenericRepository<PaySlips> paySlipsRepository;
        private readonly IPdfFileHandler _pdfFileHandlerService;
        private readonly IGenericRepository<PSettings> pSettingsRepository;
        private readonly IGenericRepository<PayDeductions> payDeductionsRepository;
        private readonly IGenericRepository<PayAllowancBenifits> payAllowancBenifitsRepository;
        int pk = 0;
        public PayRollEmploSalaryService(IGenericRepository<EmployeeBaseBenefits> employeeBenefitsRepository, IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfoRepository, IGenericRepository<Data.Models.Employees> employee, IGenericRepository<EmployeeSalarySettings> employeeSalarySettingsRepository, IGenericRepository<EmployeeAllowances> employeeAllowancesRepository, IGenericRepository<EmployeeBaseAllowances> employeeBaseAllowancesRepository, IGenericRepository<Benefits> benefitsRepository, IGenericRepository<BenefitSetups> benefitSetupsRepository, IGenericRepository<BenefitTypes> benefitType, IUserInfoService userInfoService, IGenericRepository<PaySlips> paySlipsRepository, IPdfFileHandler pdfFileHandlerService, IGenericRepository<PSettings> pSettingsRepository, IGenericRepository<PayDeductions> payDeductionsRepository, IGenericRepository<PayAllowancBenifits> payAllowancBenifitsRepository)
        {
            _employeeBaseBenefitsRepository = employeeBenefitsRepository;
            _employeeOfficeInfoRepository = employeeOfficeInfoRepository;
            this.employee = employee;
            this.employeeSalarySettingsRepository = employeeSalarySettingsRepository;
            this.employeeAllowancesRepository = employeeAllowancesRepository;
            _employeeBaseAllowancesRepository = employeeBaseAllowancesRepository;
            this.benefitsRepository = benefitsRepository;
            this.benefitSetupsRepository = benefitSetupsRepository;
            this.benefitType = benefitType;
            this.userInfoService = userInfoService;
            this.paySlipsRepository = paySlipsRepository;
            _pdfFileHandlerService = pdfFileHandlerService;
            this.pSettingsRepository = pSettingsRepository;
            this.payDeductionsRepository = payDeductionsRepository;
            this.payAllowancBenifitsRepository = payAllowancBenifitsRepository;
        }

        #region Get All Table 
        public async Task<PaginationService<EmployeeSalarySettings, PayRollEmpSalaryGetAllVM>.PaginationResult<PayRollEmpSalaryGetAllVM>> GetAllTableAsync(
    int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "",
    int? organizationId = null, string imgSrcThumb = null, List<int>? deptID = null, List<int>? empID = null, bool? paidUnpaid = null, string? month = null)
        {
            try
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var startOfMonth = new DateTime(currentYear, currentMonth, 1);
                var endOfMonth = new DateTime(currentYear, currentMonth, DateTime.DaysInMonth(currentYear, currentMonth));

                // Build the base query with necessary includes
                var query = employeeSalarySettingsRepository.AllActive()
                    .Include(x => x.Employee)
                    .ThenInclude(e => e.EmployeeOfficeInfoEmployee)
                    .ThenInclude(o => o.Department)
                    .AsQueryable();

                if (query == null)
                {
                    throw new InvalidOperationException("Query source is null.");
                }

                // Apply filters
                if (organizationId.HasValue)
                {
                    query = query.Where(x => x.Employee.EmployeeOfficeInfoEmployee.Any(o => o.OrganizationID == organizationId.Value));
                }
                if (empID != null && empID.Any())
                {
                    query = query.Where(x => empID.Contains((int)x.EmployeeID));
                }
                if (deptID != null && deptID.Any())
                {
                    query = query.Where(x => _employeeOfficeInfoRepository.AllActive()
                        .Any(eo => eo.EmployeeID == x.EmployeeID && deptID.Contains((int)eo.DepartmentID)));
                }
                //if (DateTime.TryParseExact(month, "MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                //{
                //    var firstDay = new DateOnly(parsedDate.Year, parsedDate.Month, 1);
                //    var lastDay = firstDay.AddMonths(1).AddDays(-1);
                //    query = query.Where(x => paySlipsRepository.AllActive()
                //        .Any(p => p.PayPeriodStart <= lastDay && p.PayPeriodEnd >= firstDay));
                //}
                if (paidUnpaid.HasValue)
                {
                    query = query.Where(x => paySlipsRepository.AllActive()
                        .Any(p => p.EmployeeID == x.EmployeeID
                                  && p.PayPeriodStart.Month == currentMonth
                                  && p.PayPeriodStart.Year == currentYear
                                  && p.IsPaid == paidUnpaid.Value));
                }

                // Define sorting
                Expression<Func<EmployeeSalarySettings, object>> orderByExpression = currentSortColumn?.ToLower() switch
                {
                    "empid" => x => x.EmployeeID,
                    "empname" => x => x.Employee.FirstName + " " + x.Employee.LastName,
                    "empdept" => x => x.Employee.EmployeeOfficeInfoEmployee.Select(d => d.Department.DepartmentName).FirstOrDefault(),
                    "empsalary" => x => x.Salary,
                    _ => x => x.Employee.EmployeeCode
                };

                // Preload related data for all employees
                var employeeIds = await query.Select(x => x.EmployeeID).ToListAsync();
                var paySlips = await paySlipsRepository.AllActive()
                    .Where(p => employeeIds.Contains(p.EmployeeID) && p.PayPeriodStart.Month == currentMonth && p.PayPeriodStart.Year == currentYear)
                    .ToListAsync();
                var baseBenefits = await _employeeBaseBenefitsRepository.AllActive()
                    .Include(x => x.Benefit).ThenInclude(b => b.BenefitType)
                    .Where(x => employeeIds.Contains(x.EmployeeID))
                    .ToListAsync();
                var baseAllowances = await _employeeBaseAllowancesRepository.AllActive()
                    .Include(x => x.EmployeeAllowance).ThenInclude(x => x.EmployeeAllowanceType)
                    .Where(x => employeeIds.Contains(x.EmployeeID))
                    .ToListAsync();
                var orgIds = await query.Select(x => x.Employee.EmployeeOfficeInfoEmployee.Select(o => o.OrganizationID).FirstOrDefault()).Distinct().ToListAsync();
                var taxPercentages = await pSettingsRepository.AllActive()
                    .Where(x => orgIds.Contains(x.OrganizationID))
                    .Select(x => new { x.OrganizationID, x.TaxPercentage })
                    .ToDictionaryAsync(x => x.OrganizationID, x => x.TaxPercentage ?? 0);

                // Paginate and project data
                var result = await PaginationService<EmployeeSalarySettings, PayRollEmpSalaryGetAllVM>.GetPaginatedData(
                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    currentSortColumn,
                    currentSortOrder,
                    term => b => string.IsNullOrEmpty(term) || EF.Functions.Like(b.EmployeeID.ToString(), $"%{term}%"),
                    b => new PayRollEmpSalaryGetAllVM
                    {
                        EmployeeCode = b.Employee.EmployeeCode ?? "N/A",
                        EmployeeId = b.EmployeeID,
                        EmployeeName = $"{b.Employee?.FirstName} {b.Employee?.LastName}",
                        EmployeeImage = !string.IsNullOrEmpty(b.Employee?.EmployeeImageFileName) ? imgSrcThumb + b.Employee.EmployeeImageFileName : "",
                        EmpDepartment =  b.Employee?.EmployeeOfficeInfoEmployee?.Select(m => m.Department?.DepartmentName).FirstOrDefault() ?? "N/A",

                        Salary = b.Salary,
                        IsPaid = paySlips.Where(x => x.EmployeeID == b.EmployeeID).Select(x => x.IsPaid).FirstOrDefault()
                    });

                // ✅ Calculate payslip details for each employee (same logic as GetPaySlip)
                foreach (var item in result.Data)
                {
                    try
                    {
                        var employeePaySlip = paySlips.FirstOrDefault(p => p.EmployeeID == item.EmployeeId);
                        var employeeBenefits = baseBenefits.Where(b => b.EmployeeID == item.EmployeeId).ToList();
                        var employeeAllowance = baseAllowances.FirstOrDefault(a => a.EmployeeID == item.EmployeeId);
                        var orgId = query
                            .Where(x => x.EmployeeID == item.EmployeeId)
                            .Select(x => x.Employee.EmployeeOfficeInfoEmployee
                                .Select(o => o.OrganizationID)
                                .FirstOrDefault())
                            .FirstOrDefault();

                        var taxPercentage = taxPercentages.GetValueOrDefault(orgId, 0);
                        decimal basicSalary = item.Salary ?? 0;
                        decimal totalBonus = 0;

                        // ---------------- Employee-level Benefits ----------------
                        if (employeeBenefits.Any())
                        {
                            foreach (var benefit in employeeBenefits)
                            {
                                decimal benefitSalary = 0;

                                if (benefit.CalculationTypeID == 2) // Percentage
                                    benefitSalary = basicSalary * (benefit.BenefitValue ?? 0) / 100;
                                else // Fixed
                                    benefitSalary = benefit.BenefitValue ?? 0;

                                totalBonus += benefitSalary;
                            }
                        }
                        else
                        {
                            // ---------------- Organization-level Benefits fallback ----------------
                            var orgBenefits = await benefitsRepository.AllActive()
                                .Include(b => b.BenefitSetups)
                                .Where(b => b.OrganizationID == orgId && b.IsActive== true && b.EffectiveDate >= startOfMonth
                && b.EffectiveDate <= endOfMonth)
                                .ToListAsync();

                            foreach (var benefit in orgBenefits)
                            {
                                var setup = benefit.BenefitSetups
                                    .Where(s => (s.SalaryMin == null || basicSalary >= s.SalaryMin) &&
                                                (s.SalaryMax == null || basicSalary <= s.SalaryMax))
                                    .OrderByDescending(s => s.CreatedAt ?? DateTime.MinValue)
                                    .FirstOrDefault();

                                if (setup != null)
                                {
                                    decimal benefitSalary = 0;

                                    if (setup.CalculationTypeID == 2)
                                        benefitSalary = basicSalary * (setup.Value ?? 0) / 100;
                                    else
                                        benefitSalary = setup.Value ?? 0;

                                    totalBonus += benefitSalary;
                                }
                            }
                        }

                        // ---------------- Employee-level Allowance ----------------
                        if (employeeAllowance != null)
                        {
                            decimal allowanceSalary = 0;

                            if (employeeAllowance.CalculationTypeID == 2) // Percentage
                                allowanceSalary = basicSalary * (employeeAllowance.AllowanceValue ?? 0) / 100;
                            else // Fixed
                                allowanceSalary = employeeAllowance.AllowanceValue ?? 0;

                            totalBonus += allowanceSalary;
                        }
                        else
                        {
                            // ---------------- Organization-level Allowance fallback ----------------
                            var orgAllowances = await employeeAllowancesRepository.AllActive()
                                .Include(a => a.EmployeeAllowanceSetup)
                                .Where(a => a.OrganizationID == orgId && a.IsActive==true)
                                .ToListAsync();

                            foreach (var allowance in orgAllowances)
                            {
                                var setup = allowance.EmployeeAllowanceSetup
                                    .Where(s => (s.SalaryMin == null || basicSalary >= s.SalaryMin) &&
                                                (s.SalaryMax == null || basicSalary <= s.SalaryMax))
                                    .FirstOrDefault();

                                if (setup != null)
                                {
                                    decimal allowanceSalary = 0;

                                    if (setup.CalculationTypeID == 2)
                                        allowanceSalary = basicSalary * (setup.Value ?? 0) / 100;
                                    else
                                        allowanceSalary = setup.Value ?? 0;

                                    totalBonus += allowanceSalary;
                                }
                            }
                        }

                        // ---------------- Tax and Totals ----------------
                        decimal totalSalaryEarning = basicSalary + totalBonus;
                        decimal totalDeductions = totalSalaryEarning * taxPercentage / 100;
                        decimal netSalary = totalSalaryEarning - totalDeductions;

                        // ---------------- Save results back to item ----------------
                        item.Bonus = totalBonus;
                        item.Deduction = totalDeductions;
                        item.NetSalary = netSalary;
                    }
                    catch (Exception exRow)
                    {
                        await userInfoService.ActionLogExceptionAsync("Pay Slip Row", exRow, item.EmployeeId, ActionName.Error);
                    }
                }



                return result;
            }
            catch (Exception ex)
            {
                await userInfoService.ActionLogExceptionAsync("Pay Slip Table", ex, null, ActionName.Error);
                return new PaginationService<EmployeeSalarySettings, PayRollEmpSalaryGetAllVM>.PaginationResult<PayRollEmpSalaryGetAllVM>
                {
                    Data = new List<PayRollEmpSalaryGetAllVM>(),
                    TotalCount = 0
                };
            }
        }


        #endregion


        #region Save PaySlip



        public async Task<CommonReturnViewModel> SaveAsync(PayRollEmpSalarySaveVM entityVM)
        {
            var result = new CommonReturnViewModel();
            int pk = 0;

            if (entityVM == null)
            {
                result.Success = false;
                result.Message = "Invalid payslip data!";
                return result;
            }

            // Get basic salary
            decimal? basicsalary = await employeeSalarySettingsRepository.AllActive()
                .Where(x => x.EmployeeID == entityVM.EmployeeID) .Select(x => x.Salary) .FirstOrDefaultAsync();

            if (basicsalary == null)
            {
                result.Success = false;
                result.Message = "Employee salary not found!";
                return result;
            }

            await paySlipsRepository.BeginTransactionAsync();

            var currentYear = DateTime.Now.Year;
            var currentMonth = DateTime.Now.Month;

            // Check for duplicate paid payslip
            var hasDuplicate = await paySlipsRepository.AllActive()
                .AnyAsync(p =>
                    p.EmployeeID == entityVM.EmployeeID &&
                    p.PayPeriodStart.Month == currentMonth &&
                    p.PayPeriodStart.Year == currentYear &&
                    p.IsPaid == true);

            if (hasDuplicate)
            {
                result.Success = false;
                result.Message = $"Payslip already exists for Employee ID: {entityVM.EmployeeID} (Paid this month)";
                return result;
            }

            try
            {
                // Create Payslip
                var entity = new PaySlips
                {
                    EmployeeID = entityVM.EmployeeID,
                    BasicSalary = basicsalary.Value,
                    PayPeriodStart = new DateOnly(currentYear, currentMonth, 1),
                    PayPeriodEnd = new DateOnly(currentYear, currentMonth, DateTime.DaysInMonth(currentYear, currentMonth)),
                    IsPaid = true,
                    LIP = entityVM.LIP,
                    LMAC = entityVM.LMAC,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = entityVM.CreatedBy,
                };

                await paySlipsRepository.AddAsync(entity);
                pk = entity.PaySlipID;

                //------------------------- GET PAYSLIP CALCULATION ----------------------------
                var paySlipResult = await GetPaySlip((int)entityVM.EmployeeID);
                if (!paySlipResult.Success)
                {
                    await paySlipsRepository.RollbackTransactionAsync();
                    result.Success = false;
                    result.Message = "Failed to calculate payslip.";
                    return result;
                }

                var paySlipVM = (PayRollPaySlipEmpVM)paySlipResult.Data;


                if (paySlipVM.BeneFits != null)
                {
                    foreach (var benefit in paySlipVM.BeneFits)
                    {
                        // Determine if it's percentage-based
                        bool isPercentage = false;
                        decimal percentageBasicOf = 0;

                        // Check DisplayValue ends with % (as a fallback)
                        if (benefit.DisplayValue.EndsWith("%"))
                        {
                            isPercentage = true;
                            // Extract the numeric value from display
                            decimal.TryParse(benefit.DisplayValue.TrimEnd('%'), out percentageBasicOf);
                        }

                        var payBenefit = new PayAllowancBenifits
                        {
                            PaySlipID = entity.PaySlipID,
                            PayAllowancBenifitName = benefit.Type,          // Name of the benefit
                            Amount = (decimal)benefit.BenefitsSalary,       // Calculated salary
                            IsPercentage = isPercentage,                    // True if percentage
                            PercentageOfBasic = percentageBasicOf,         // Store the percentage value
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = entityVM.CreatedBy,
                            LIP = entityVM.LIP,
                            LMAC = entityVM.LMAC
                        };

                        await payAllowancBenifitsRepository.AddAsync(payBenefit);
                    }
                }

                // Similarly for Allowances
                if (paySlipVM.Allowances != null)
                {
                    foreach (var allowance in paySlipVM.Allowances)
                    {
                        bool isPercentage = false;
                        decimal percentageBasicOf = 0;

                        if (allowance.DisplayValue.EndsWith("%"))
                        {
                            isPercentage = true;
                            decimal.TryParse(allowance.DisplayValue.TrimEnd('%'), out percentageBasicOf);
                        }

                        var payAllowance = new PayAllowancBenifits
                        {
                            PaySlipID = entity.PaySlipID,
                            PayAllowancBenifitName = allowance.Type,
                            Amount = (decimal)allowance.AllowanceSalary,
                            IsPercentage = isPercentage,
                            PercentageOfBasic = percentageBasicOf,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = entityVM.CreatedBy,
                            LIP = entityVM.LIP,
                            LMAC = entityVM.LMAC
                        };

                        await payAllowancBenifitsRepository.AddAsync(payAllowance);
                    }
                }


                //------------------------- SAVE DEDUCTIONS ----------------------------
                var payDeduction = new PayDeductions
                {
                    PaySlipID = entity.PaySlipID,
                    PayAllowancBenifitName = "Tax / Deduction",
                    Amount = paySlipVM.TotalDeductions,
                    IsPercentage = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = entityVM.CreatedBy,
                    LIP = entityVM.LIP,
                    LMAC = entityVM.LMAC
                };
                //await payDeductionsRepository.AddAsync(payDeduction);

                await paySlipsRepository.CommitTransactionAsync();

                await userInfoService.ActionLogAsync("Pay Slip Save", ActionName.DataAdd, null, entity, entity.PaySlipID, entityVM);

                result.Success = true;
                result.Message = "Payslip saved successfully!";
                result.Data = entity;
            }
            catch (Exception ex)
            {
                await paySlipsRepository.RollbackTransactionAsync();
                result.Success = false;
                result.Message = ex.Message;
                result.Errors.Add(ex.Message);
                await userInfoService.ActionLogExceptionAsync("Pay Slip Save", ex, pk, ActionName.Error);
            }

            return result;
        }



        #region Save Payslips in Bulk
        public async Task<CommonReturnViewModel> SaveBulkAsync(List<PayRollEmpSalarySaveVM> entityVMList)
        {
            var result = new CommonReturnViewModel();

            if (entityVMList == null || !entityVMList.Any())
            {
                result.Success = false;
                result.Message = "No payslip data provided!";
                return result;
            }

            // Preload all salaries in one query
            var employeeIds = entityVMList.Select(x => x.EmployeeID).ToList();
            var salaryData = await employeeSalarySettingsRepository.AllActive()
                .Where(x => employeeIds.Contains(x.EmployeeID))
                .Select(x => new { x.EmployeeID, x.Salary })
                .ToListAsync();

            await paySlipsRepository.BeginTransactionAsync();
            try
            {
                var entities = entityVMList.Select(vm =>
                {
                    var basicSalary = salaryData.FirstOrDefault(s => s.EmployeeID == vm.EmployeeID)?.Salary ?? 0;

                    return new PaySlips
                    {
                        EmployeeID = vm.EmployeeID,
                        BasicSalary = basicSalary,
                        PayPeriodStart = vm.PayPeriodStart,
                        PayPeriodEnd = vm.PayPeriodEnd,
                        IsPaid = vm.IsPaid,
                        LIP = vm.LIP,
                        LMAC = vm.LMAC,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = vm.CreatedBy
                    };
                }).ToList();

                // ✅ Bulk insert
                await paySlipsRepository.AddRangeAsync(entities);
                await paySlipsRepository.CommitTransactionAsync();

                //// ✅ Optional: log each save
                //foreach (var entity in entityVMList)
                //{
                //    await userInfoService.ActionLogAsync("Pay Slip Save", ActionName.DataAdd, null, entity, entities.FirstOrDefault().PaySlipID, entity);

                //}

                result.Success = true;
                result.Message = $"{entities.Count} Payslips saved successfully!";
                result.Data = entities;
            }
            catch (Exception ex)
            {
                await paySlipsRepository.RollbackTransactionAsync();
                result.Success = false;
                result.Message = ex.Message;
                result.Errors.Add(ex.Message);
                await userInfoService.ActionLogExceptionAsync("Pay Slip Save", ex, null, ActionName.Error);
            }

            return result;
        }
        #endregion

        #endregion

        #region Save pdf emp Saalry



        //public async Task<CommonReturnViewModel> SaveExportAsync(PaySlipRequestVM model)
        //{
        //    var result = new CommonReturnViewModel();

        //    if (model == null || model.Employees == null || !model.Employees.Any())
        //    {
        //        result.Success = false;
        //        result.Message = "No employee data received!";
        //        return result;
        //    }

        //    await paySlipsRepository.BeginTransactionAsync();



        //    try
        //    {
        //        var employeeIds = model.Employees.Select(e => e.EmployeeID).ToList();



        //        var currentYear = DateTime.Now.Year;
        //        var currentMonth = DateTime.Now.Month;

        //        // ✅ Check for duplicate payslips for this month where IsPaid == true
        //        var duplicatePayslips = await paySlipsRepository.AllActive()
        //            .Where(p => employeeIds.Contains((int)p.EmployeeID)
        //                        && p.PayPeriodStart.Month == currentMonth
        //                        && p.PayPeriodStart.Year == currentYear
        //                        && p.IsPaid==true)
        //            .ToListAsync();

        //        if (duplicatePayslips.Any())
        //        {
        //            var duplicateEmpIds = string.Join(", ", duplicatePayslips.Select(p => p.EmployeeID));
        //            result.Success = false;
        //            result.Message = $"Payslips already exist for employees: {duplicateEmpIds} (Paid this month)";
        //            return result;
        //        }
        //        //

        //        var salaries = await employeeSalarySettingsRepository.AllActive()
        //            .Where(x => employeeIds.Contains((int)x.EmployeeID))
        //            .Select(x => new { x.EmployeeID, x.Salary })
        //            .ToListAsync();


        //        // 1️⃣ Create and add all payslips first
        //        var paySlipsList = salaries.Select(emp =>
        //        {
        //            var empData = model.Employees.First(e => e.EmployeeID == emp.EmployeeID);

        //            return new PaySlips
        //            {
        //                EmployeeID = emp.EmployeeID,
        //                BasicSalary = emp.Salary ?? 0,
        //                PayPeriodStart = new DateOnly(currentYear, currentMonth, 1),
        //                PayPeriodEnd = new DateOnly(currentYear, currentMonth, DateTime.DaysInMonth(currentYear, currentMonth)),
        //                IsPaid = empData.IsPaid,
        //                LIP = model.LIP,
        //                LMAC = model.LMAC,
        //                CreatedAt = DateTime.UtcNow,
        //                CreatedBy = model.CreatedBy
        //            };
        //        }).ToList();

        //        await paySlipsRepository.AddRangeAsync(paySlipsList);

        //        await paySlipsRepository.CommitTransactionAsync();

        //        result.Success = true;
        //        result.Message = "Payslips saved successfully!";
        //        result.Data = paySlipsList;



        //    }
        //    catch (Exception ex)
        //    {
        //        await paySlipsRepository.RollbackTransactionAsync();
        //        result.Success = false;
        //        result.Message = ex.Message;
        //        result.Errors.Add(ex.Message);
        //        await userInfoService.ActionLogExceptionAsync("Pay Slip Save", ex, null, ActionName.Error);
        //    }

        //    return result;
        //}


        //
        public async Task<CommonReturnViewModel> SaveExportAsync(PaySlipRequestVM model)
        {
            var result = new CommonReturnViewModel();

            if (model == null || model.Employees == null || !model.Employees.Any())
            {
                result.Success = false;
                result.Message = "No employee data received!";
                return result;
            }

            await paySlipsRepository.BeginTransactionAsync();

            try
            {
                var employeeIds = model.Employees.Select(e => e.EmployeeID).ToList();

                var currentYear = DateTime.Now.Year;
                var currentMonth = DateTime.Now.Month;

                // Check duplicate payslips
                var duplicatePayslips = await paySlipsRepository.AllActive()
                    .Where(p => employeeIds.Contains((int)p.EmployeeID)
                                && p.PayPeriodStart.Month == currentMonth
                                && p.PayPeriodStart.Year == currentYear
                                && p.IsPaid == true)
                    .ToListAsync();

                if (duplicatePayslips.Any())
                {
                    var duplicateEmpIds = string.Join(", ", duplicatePayslips.Select(p => p.EmployeeID));
                    result.Success = false;
                    result.Message = $"Payslips already exist for employees: {duplicateEmpIds} (Paid this month)";
                    return result;
                }

                // Get salaries
                var salaries = await employeeSalarySettingsRepository.AllActive()
                    .Where(x => employeeIds.Contains((int)x.EmployeeID))
                    .Select(x => new { x.EmployeeID, x.Salary })
                    .ToListAsync();

                var paySlipsList = new List<PaySlips>();

                // 1️⃣ Create payslips
                foreach (var emp in salaries)
                {
                    var empData = model.Employees.First(e => e.EmployeeID == emp.EmployeeID);

                    var entity = new PaySlips
                    {
                        EmployeeID = emp.EmployeeID,
                        BasicSalary = emp.Salary ?? 0,
                        PayPeriodStart = new DateOnly(currentYear, currentMonth, 1),
                        PayPeriodEnd = new DateOnly(currentYear, currentMonth, DateTime.DaysInMonth(currentYear, currentMonth)),
                        IsPaid = empData.IsPaid,
                        LIP = model.LIP,
                        LMAC = model.LMAC,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = model.CreatedBy
                    };

                    await paySlipsRepository.AddAsync(entity);
                    paySlipsList.Add(entity);
                }

                // 2️⃣ Calculate benefits/allowances for each employee
                foreach (var payslip in paySlipsList)
                {
                    var paySlipResult = await GetPaySlip((int)payslip.EmployeeID);
                    if (!paySlipResult.Success) continue;

                    var paySlipVM = (PayRollPaySlipEmpVM)paySlipResult.Data;

                    // Save Benefits
                    if (paySlipVM.BeneFits != null)
                    {
                        foreach (var benefit in paySlipVM.BeneFits)
                        {
                            bool isPercentage = benefit.DisplayValue.EndsWith("%");
                            decimal percentageBasicOf = 0;

                            if (isPercentage)
                                decimal.TryParse(benefit.DisplayValue.TrimEnd('%'), out percentageBasicOf);

                            var payBenefit = new PayAllowancBenifits
                            {
                                PaySlipID = payslip.PaySlipID,
                                PayAllowancBenifitName = benefit.Type,
                                Amount = (decimal)benefit.BenefitsSalary,
                                IsPercentage = isPercentage,
                                PercentageOfBasic = percentageBasicOf,
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = model.CreatedBy,
                                LIP = model.LIP,
                                LMAC = model.LMAC
                            };

                            await payAllowancBenifitsRepository.AddAsync(payBenefit);
                        }
                    }

                    // Save Allowances
                    if (paySlipVM.Allowances != null)
                    {
                        foreach (var allowance in paySlipVM.Allowances)
                        {
                            bool isPercentage = allowance.DisplayValue.EndsWith("%");
                            decimal percentageBasicOf = 0;

                            if (isPercentage)
                                decimal.TryParse(allowance.DisplayValue.TrimEnd('%'), out percentageBasicOf);

                            var payAllowance = new PayAllowancBenifits
                            {
                                PaySlipID = payslip.PaySlipID,
                                PayAllowancBenifitName = allowance.Type,
                                Amount = (decimal)allowance.AllowanceSalary,
                                IsPercentage = isPercentage,
                                PercentageOfBasic = percentageBasicOf,
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = model.CreatedBy,
                                LIP = model.LIP,
                                LMAC = model.LMAC
                            };

                            await payAllowancBenifitsRepository.AddAsync(payAllowance);
                        }
                    }
                }

                await paySlipsRepository.CommitTransactionAsync();

                result.Success = true;
                result.Message = "Payslips saved successfully!";
                result.Data = paySlipsList;
            }
            catch (Exception ex)
            {
                await paySlipsRepository.RollbackTransactionAsync();
                result.Success = false;
                result.Message = ex.Message;
                result.Errors.Add(ex.Message);
                await userInfoService.ActionLogExceptionAsync("Pay Slip Save Export", ex, null, ActionName.Error);
            }

            return result;
        }


        #endregion

        #region Get PaySlip 


        public async Task<CommonReturnViewModel> GetPaySlip(int id)
        {
            try
            {
                // If month/year comes from request
                var currentYear = DateTime.Now.Year;
                var currentMonth = DateTime.Now.Month;
                var startOfMonth = new DateTime(currentYear, currentMonth, 1);
                var endOfMonth = new DateTime(currentYear, currentMonth, DateTime.DaysInMonth(currentYear, currentMonth));

                var paySlipNo = (await paySlipsRepository.AllActive().MaxAsync(x => (int?)x.PaySlipID) ?? 0) + 1;

                var baseQuery = await _employeeOfficeInfoRepository.AllActive()
                         .Include(x => x.Employee)
                         .ThenInclude(x => x.EmployeeSalarySettingsEmployee)
                         .Where(x => x.EmployeeID == id)
                         .Include(x => x.Organization)
                        .ThenInclude(x => x.EmployeeAllowances)
                        .FirstOrDefaultAsync();
                var orgPercent = await pSettingsRepository.AllActive()
               .Where(x => x.OrganizationID == baseQuery.OrganizationID).Select(x => x.TaxPercentage).FirstOrDefaultAsync() ?? 0;



                if (baseQuery == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Employee not found."
                    };
                }

               

                // Get basic salary
                decimal? basicsalary = await employeeSalarySettingsRepository.AllActive()
                    .Where(x => x.EmployeeID == id)
                    .Select(x => x.Salary)
                    .FirstOrDefaultAsync();

                //------------------------- BENEFITS ----------------------------
                var benefitsVMs = new List<BeneFitsVM>();

                // Employee Base Benefits
                var baseBenefitsList = await _employeeBaseBenefitsRepository.AllActive()
                    .Include(x => x.Benefit)
                        .ThenInclude(b => b.BenefitType)
                    .Where(x => x.EmployeeID == id)
                    .ToListAsync();

                if (baseBenefitsList.Any())
                {
                    foreach (var baseBenefit in baseBenefitsList)
                    {
                        decimal benefitSalary = 0;
                        string display = "";

                        if (baseBenefit.CalculationTypeID == 2) // Percentage
                        {
                            benefitSalary = (decimal)(basicsalary * (baseBenefit.BenefitValue ?? 0) / 100);
                            display = $"{(baseBenefit.BenefitValue ?? 0).ToString("0")}%";
                        }
                        else // Fixed
                        {
                            benefitSalary = (baseBenefit.BenefitValue ?? 0);
                            display = $"{Math.Floor(baseBenefit.BenefitValue ?? 0)}";
                        }

                        benefitsVMs.Add(new BeneFitsVM
                        {
                            Type = baseBenefit.Benefit?.BenefitType?.BenefitTypeName ?? "Employee Benefit",
                            Amount = baseBenefit.BenefitValue ?? 0,
                            DisplayValue = display,
                            BenefitsSalary = benefitSalary
                        });
                    }
                }
                else
                {
                    // Organization Benefits fallback

                 

                    var benefitsList = await benefitsRepository.AllActive()
                        .Include(b => b.BenefitType)
                        .Include(b => b.BenefitSetups)
                        .Where(b => b.OrganizationID == baseQuery.OrganizationID && b.IsActive == true &&  b.EffectiveDate >= startOfMonth
                            && b.EffectiveDate <= endOfMonth)
                        .ToListAsync();

                    foreach (var benefit in benefitsList)
                    {
                        var setup = benefit.BenefitSetups
                            .Where(s => (s.SalaryMin == null || basicsalary >= s.SalaryMin) &&
                                        (s.SalaryMax == null || basicsalary <= s.SalaryMax))
                            .OrderByDescending(s => s.CreatedAt ?? DateTime.MinValue)
                            .FirstOrDefault();

                        if (setup != null)
                        {
                            decimal benefitSalary = 0;
                            string display = "";

                            if (setup.CalculationTypeID == 2) // Percentage
                            {
                                benefitSalary = (decimal)(basicsalary * (setup.Value ?? 0) / 100);
                                display = $"{(setup.Value ?? 0).ToString("0")}%";
                            }
                            else // Fixed
                            {
                                benefitSalary = setup.Value ?? 0;
                                display = $"{Math.Floor(setup.Value ?? 0)}";
                            }

                            benefitsVMs.Add(new BeneFitsVM
                            {
                                Type = benefit.BenefitType.BenefitTypeName,
                                Amount = setup.Value ?? 0,
                                DisplayValue = display,
                                BenefitsSalary = benefitSalary
                            });
                        }
                    }
                }

                //------------------------- ALLOWANCES ----------------------------
                var allowanceVMs = new List<AllowanceVM>();

                // Employee Base Allowances
                var baseAllowances = await _employeeBaseAllowancesRepository.AllActive()
                    .Include(x => x.EmployeeAllowance)
                        .ThenInclude(x => x.EmployeeAllowanceType)
                    .Where(x => x.EmployeeID == id)
                    .FirstOrDefaultAsync();

                if (baseAllowances != null)
                {
                    decimal allowanceSalary = 0;
                    string display = "";

                    if (baseAllowances.CalculationTypeID == 2) 
                    {
                        allowanceSalary = (decimal)(basicsalary * (baseAllowances.AllowanceValue ?? 0) / 100);
                        // display = $"{baseAllowances.AllowanceValue}%";
                        display = $"{(baseAllowances.AllowanceValue ?? 0).ToString("0")}%"; // ✅ fixed
                    }
                    else // Fixed
                    {
                        allowanceSalary = baseAllowances.AllowanceValue ?? 0;
                        display = $"{Math.Floor(baseAllowances.AllowanceValue ?? 0)}";
                    }

                    allowanceVMs.Add(new AllowanceVM
                    {
                        Type = baseAllowances.EmployeeAllowance?.EmployeeAllowanceType?.EmployeeAllowanceTypeName ?? "Employee Allowance",
                        Amount = baseAllowances.AllowanceValue ?? 0,
                        DisplayValue = display,
                        AllowanceSalary = allowanceSalary
                    });
                }
                else
                {
                    // Organization Allowances fallback
                    var allowancesList = await employeeAllowancesRepository.AllActive()
                        .Include(ea => ea.EmployeeAllowanceType)
                        .Include(ea => ea.EmployeeAllowanceSetup).Where(ea => ea.OrganizationID == baseQuery.OrganizationID && ea.IsActive == true && ea.EffectiveDate >= startOfMonth
                && ea.EffectiveDate <= endOfMonth).ToListAsync();

                    foreach (var allowance in allowancesList)
                    {
                        var setup = allowance.EmployeeAllowanceSetup
                            .Where(s => (s.SalaryMin == null || basicsalary >= s.SalaryMin) &&
                                        (s.SalaryMax == null || basicsalary <= s.SalaryMax)).FirstOrDefault();

                        if (setup != null)
                        {
                            decimal allowanceSalary = 0;
                            string display = "";

                            if (setup.CalculationTypeID ==2)
                            {
                                allowanceSalary = (decimal)(basicsalary * (setup.Value ?? 0) / 100);
                                // display = $"{setup.Value}%";
                                display = $"{(setup.Value ?? 0).ToString("0")}%"; // ✅ fixed
                            }
                            else // Fixed
                            {
                                allowanceSalary = setup.Value ?? 0;
                                display = $"{Math.Floor(setup.Value ?? 0)}";
                            }

                            allowanceVMs.Add(new AllowanceVM
                            {
                                Type = allowance.EmployeeAllowanceType.EmployeeAllowanceTypeName,
                                Amount = setup.Value ?? 0,
                                DisplayValue = display,
                                AllowanceSalary = allowanceSalary
                            });
                        }
                    }
                }

                //------------------------- TOTAL ----------------------------
                var totalSalaryEarning = (basicsalary ?? 0)
                    + (allowanceVMs?.Sum(a => a.AllowanceSalary ?? 0) ?? 0)
                    + (benefitsVMs?.Sum(b => b.BenefitsSalary ?? 0) ?? 0);

                decimal taxPercentage = orgPercent;
                decimal taxAmount = totalSalaryEarning * taxPercentage / 100;
                decimal totalAfterTax = totalSalaryEarning - taxAmount;

                var paySlipVM = new PayRollPaySlipEmpVM
                {
                    OrganizationName = baseQuery.Organization?.OrganizationName ?? "",
                    OrganizationAddress = baseQuery.Organization?.Address ?? "",
                    OrganizationEmailAddress = baseQuery?.Organization?.EmailAddress ?? "",
                    OrganizationLogoPic = baseQuery?.Organization?.LogoLink != null ? $"/media/company/logo/{baseQuery.Organization.LogoLink}" : "",
                    EmployeeName = $"{baseQuery?.Employee.FirstName} {baseQuery?.Employee.LastName}",
                    EmployeeAddress = $"{baseQuery?.Employee.State} {baseQuery?.Employee.City},{baseQuery?.Employee.HouseNo} {baseQuery?.Employee.PostalCode}",
                    EmployeeEmail = baseQuery?.Employee.Email,
                    BasicSalary = basicsalary,
                    Allowances = allowanceVMs,
                    BeneFits = benefitsVMs,
                    TotalSalary = totalSalaryEarning,
                    SalaryInWords = NumberToWordsConverter.NumberToWords((int)totalAfterTax) + " Only",
                    TotalBonus = (allowanceVMs?.Sum(a => a.AllowanceSalary ?? 0) ?? 0) + (benefitsVMs?.Sum(b => b.BenefitsSalary ?? 0) ?? 0),
                    TotalDeductions = taxAmount,
                    NetPay = totalAfterTax,
                    ProfessionalTax = taxAmount,
                    PayslipNo=paySlipNo
                };

                return new CommonReturnViewModel
                {
                    Success = true,
                    Data = paySlipVM,
                    Message = "Payslip retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                await userInfoService.ActionLogExceptionAsync("Pay Slip Get Method", ex, id, ActionName.Error);
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }


       

        #endregion


        #region  Number converter 
        public static class NumberToWordsConverter
        {
            private static readonly string[] Ones =
            {
        "Zero","One","Two","Three","Four","Five","Six","Seven","Eight","Nine",
        "Ten","Eleven","Twelve","Thirteen","Fourteen","Fifteen","Sixteen","Seventeen","Eighteen","Nineteen"
          };

            private static readonly string[] Tens =
            {
              "Zero","Ten","Twenty","Thirty","Forty","Fifty","Sixty","Seventy","Eighty","Ninety"
          };

            public static string NumberToWords(int number)
            {
                if (number == 0)
                    return "Zero";

                if (number < 0)
                    return "Minus " + NumberToWords(Math.Abs(number));

                string words = "";

                if ((number / 1000000) > 0)
                {
                    words += NumberToWords(number / 1000000) + " Million ";
                    number %= 1000000;
                }

                if ((number / 1000) > 0)
                {
                    words += NumberToWords(number / 1000) + " Thousand ";
                    number %= 1000;
                }

                if ((number / 100) > 0)
                {
                    words += NumberToWords(number / 100) + " Hundred ";
                    number %= 100;
                }

                if (number > 0)
                {
                    if (words != "")
                        words += " ";

                    if (number < 20)
                        words += Ones[number];
                    else
                    {
                        words += Tens[number / 10];
                        if ((number % 10) > 0)
                            words += " " + Ones[number % 10];
                    }
                }

                return words.Trim();
            }
        }

        #endregion


        #region  Pdf Generate

        public async Task<byte[]> GeneratePdf(int id)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            try
            {
                var payslipEntity = await paySlipsRepository.AllActive()
                    .Where(x => x.PaySlipID == id)
                    .Select(x => x.EmployeeID)
                    .FirstOrDefaultAsync();

                if (payslipEntity == null)
                    throw new Exception("Payslip not found.");

                var payslipResult = await GetPaySlip((int)payslipEntity);

                if (!payslipResult.Success || payslipResult.Data == null)
                    throw new Exception(payslipResult.Message ?? "Failed to fetch payslip data.");

                var data = (PayRollPaySlipEmpVM)payslipResult.Data;

                using (var stream = new MemoryStream())
                {
                    Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(35);
                            page.DefaultTextStyle(x => x.FontFamily(Fonts.TimesNewRoman).FontSize(10));

                            page.Content().Column(col =>
                            {
                                // Payslip title
                                col.Item().AlignCenter().Text($"Payslip for the month of {DateTime.Now:MMMM yyyy}")
                                    .FontSize(16).Bold();

                                col.Item().PaddingVertical(10);

                                // Payment Info Section with Border
                                col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(10).Row(row =>
                                {
                                    row.RelativeItem().Column(left =>
                                    {
                                        left.Item().Text(text =>
                                        {
                                            text.Span("Payslip No. ");
                                            text.Span(id.ToString()).Bold();
                                        });
                                    });

                                    row.RelativeItem().AlignRight().Column(right =>
                                    {
                                        right.Item().Text(text =>
                                        {
                                            text.Span("Payment Date ");
                                            text.Span($"{DateTime.Now:MMM dd, yyyy - hh:mm tt}").Bold();
                                        });
                                    });
                                });

                                col.Item().PaddingVertical(10);

                                // Organization & Employee Details
                                col.Item().PaddingVertical(10).Row(row =>
                                {
                                    row.RelativeItem().Column(left =>
                                    {
                                        var logoPath = string.IsNullOrEmpty(data.OrganizationLogoPic)
                                            ? Path.Combine("wwwroot/assets/img/icons", "No-Image-Placeholder.svg.png")
                                            : data.OrganizationLogoPic;

                                        if (System.IO.File.Exists(logoPath))
                                        {
                                            left.Item()
                                                .Height(26)
                                                .Width(26)
                                                .Image(logoPath)
                                                .FitArea();
                                        }

                                        left.Item().PaddingTop(5).Text(data.OrganizationName).Bold().FontSize(11);
                                        left.Item().Text(data.OrganizationAddress).FontSize(9);
                                    });

                                    row.RelativeItem().AlignRight().Column(right =>
                                    {
                                        right.Item().Text("Payment To").FontSize(9).Bold();
                                        right.Item().Text(data.EmployeeName).Bold().FontSize(11);
                                        right.Item().Text(data.EmployeeAddress).FontSize(9);
                                    });
                                });

                                col.Item().PaddingVertical(10);

                                // Earnings & Deductions Tables
                                col.Item().Row(row =>
                                {
                                    // Earnings Column
                                    row.RelativeItem().PaddingRight(10).Column(earn =>
                                    {
                                        earn.Item().Text("Earnings").Bold().FontSize(11).Bold();
                                        earn.Item().PaddingVertical(5);

                                        earn.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Table(table =>
                                        {
                                            table.ColumnsDefinition(c =>
                                            {
                                                c.RelativeColumn();
                                                c.ConstantColumn(80);
                                            });

                                            void AddRow(string label, string value, bool isBold = false)
                                            {
                                                var labelCell = table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2)
                                                    .Padding(8).Text(label).FontSize(10);

                                                var valueCell = table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2)
                                                    .Padding(8).AlignRight().Text(value).FontSize(10);

                                                if (isBold)
                                                {
                                                    labelCell.Bold();
                                                    valueCell.Bold();
                                                }
                                            }

                                            // Basic Salary
                                            AddRow("Basic", data.BasicSalary?.ToString("N2") ?? "0.00");

                                            // Allowances
                                            foreach (var a in data.Allowances)
                                            {
                                                AddRow($"{a.Type} ({a.DisplayValue})",
                                                       Math.Floor(a.AllowanceSalary ?? 0).ToString("N2"));
                                            }

                                            // Benefits
                                            foreach (var b in data.BeneFits)
                                            {
                                                AddRow($"{b.Type} ({b.DisplayValue})",
                                                       Math.Floor(b.BenefitsSalary ?? 0).ToString("N2"));
                                            }

                                            // Total Earnings
                                            AddRow("Total Earnings", data.TotalSalary.ToString("N2") ?? "0.00", true);

                                        });
                                    });

                                    // Deductions Column
                                    row.RelativeItem().PaddingLeft(10).Column(ded =>
                                    {
                                        ded.Item().Text("Deductions").Bold().FontSize(11);
                                        ded.Item().PaddingVertical(5);

                                        ded.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Table(table =>
                                        {
                                            table.ColumnsDefinition(c =>
                                            {
                                                c.RelativeColumn();
                                                c.ConstantColumn(80);
                                            });



                                            void AddRow(string label, string value, bool isBold = false)
                                            {
                                                var labelCell = table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2)
                                                    .Padding(8).Text(label).FontSize(10);

                                                var valueCell = table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2)
                                                    .Padding(8).AlignRight().Text(value).FontSize(10);

                                                if (isBold)
                                                {
                                                    labelCell.Bold();
                                                    valueCell.Bold();
                                                }
                                            }

                                            AddRow("Provident Fund", "0.00");
                                            AddRow("Professional Tax", "0.00");
                                            AddRow("ESI", "0.00");
                                            AddRow("Home Loan", "0.00");
                                            AddRow("TDS", "0.00");
                                            AddRow("Total Deductions", "0.00", true);
                                        });
                                    });
                                });

                                // Net Pay Section
                                col.Item().PaddingTop(15).Column(netPay =>
                                {
                                    var netPayAmount = (data?.TotalSalary ?? 0) - 0;
                                    netPay.Item().Text($"Net Pay : {netPayAmount:N2}").Bold().FontSize(11);
                                    netPay.Item().PaddingTop(8).Text(data.SalaryInWords ?? "").FontSize(10);
                                });

                                // Signature Section
                                col.Item().PaddingTop(30).AlignRight().Column(sig =>
                                {
                                    sig.Item().Text("For Priya Jain").Bold().FontSize(10);
                                    sig.Item().PaddingTop(25).Text("Authorised Signatory").FontSize(10);
                                });

                                // Print footer (optional in PDF)
                                col.Item().PaddingTop(20).AlignCenter()
                                    .Text("This is a system-generated payslip")
                                    .FontSize(9).Italic().FontColor(Colors.Grey.Medium);
                            });
                        });
                    }).GeneratePdf(stream);

                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("PDF Generation Error: " + ex.Message);
                await userInfoService.ActionLogExceptionAsync("Pay Slip according to EmployeeID", ex, id, ActionName.Error);
                throw new Exception("Error while generating Pay Slip PDF.");
            }
        }

        
        #endregion



    }
}
