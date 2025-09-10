using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpAllowance;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpSalary;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using GCTL.Service.PayRollManagements.PayRollEmpAllowance;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollEmpSalary
{
    public class PayRollEmploSalaryService : IPayRollEmpSalaryService
    {
        private readonly IGenericRepository<EmployeeBaseBenefits> _employeeBaseBenefitsRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOfficeInfoRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> employee;
        private readonly IGenericRepository<EmployeeSalarySettings> employeeSalarySettingsRepository;
        private readonly IGenericRepository<EmployeeAllowances> employeeAllowancesRepository; 
        private readonly IGenericRepository<EmployeeBaseAllowances> _employeeBaseAllowancesRepository;
        public PayRollEmploSalaryService(IGenericRepository<EmployeeBaseBenefits> employeeBenefitsRepository, IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfoRepository, IGenericRepository<Data.Models.Employees> employee, IGenericRepository<EmployeeSalarySettings> employeeSalarySettingsRepository = null, IGenericRepository<EmployeeAllowances> employeeAllowancesRepository = null, IGenericRepository<EmployeeBaseAllowances> employeeBaseAllowancesRepository = null)
        {
            _employeeBaseBenefitsRepository = employeeBenefitsRepository;
            _employeeOfficeInfoRepository = employeeOfficeInfoRepository;
            this.employee = employee;
            this.employeeSalarySettingsRepository = employeeSalarySettingsRepository;
            this.employeeAllowancesRepository = employeeAllowancesRepository;
            _employeeBaseAllowancesRepository = employeeBaseAllowancesRepository;
        }

        #region Get All Table 
        public async Task<PaginationService<EmployeeBaseBenefits, PayRollEmpSalaryGetAllVM>.PaginationResult<PayRollEmpSalaryGetAllVM>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", int? organizationId = null, string imgSrcThumb = null)
        {
            try
            {
                // 🔹 Step 3: Base query with includes
                var query = _employeeBaseBenefitsRepository.AllActive().Include(x => x.Employee).OrderByDescending(x => x.EmployeeBaseBenefitID).AsQueryable();
                if (query == null)
                {
                    throw new InvalidOperationException("query source is null.");
                }

                //if (organizationId.HasValue)
                //{

                //    query = query.Where(x => x.OrganizationID == organizationId);
                //}
                Expression<Func<EmployeeBaseBenefits, object>> orderByExpression = currentSortColumn?.ToLower() switch
                {
                    //"companyname" => x => x.Organization.OrganizationName,
                    //"helthinsurance" => x => x.HealthInsurance,
                    //"ec" => x => x.ProvidentFundEmployeeContrebution,
                    //"oc" => x => x.ProvidentFundOrganizationContrebution,
                    //"time" => x => x.ProvidentFundMinimumServiceYear,
                    //"salaryof" => x => x.ProvidentFundOnSalaryType.SalaryTypeName,
                    //"rate" => x => x.FastivalBonusRate,
                    //"festivalbonus" => x => x.FastivalBonusOnSalaryType.SalaryTypeName,
                    //"performancebonus" => x => x.PerformanceBonus,
                    //"yearendbonus" => x => x.YearlyEndBonusType.YearlyEndBonusTypeName,
                    _ => x => x.EmployeeBaseBenefitID
                };
                query = currentSortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(orderByExpression)
                    : query.OrderBy(orderByExpression);

                var result = await PaginationService<EmployeeBaseBenefits, PayRollEmpSalaryGetAllVM>.GetPaginatedData(
                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    currentSortColumn,
                    currentSortOrder,
                   term => b =>
                      string.IsNullOrEmpty(term) ||
                      EF.Functions.Like(b.EmployeeBaseBenefitID.ToString(), $"%{term}%"),
                     b => new PayRollEmpSalaryGetAllVM
                     {
                         EmployeeId = b.EmployeeID,
                         EmployeeName = $"{b.Employee?.FirstName} {b.Employee?.LastName}",
                         EmployeeImage = !string.IsNullOrEmpty(b.Employee?.EmployeeImageFileName) ? imgSrcThumb + b.Employee.EmployeeImageFileName : "",
                         EmpDepartment = _employeeOfficeInfoRepository.AllActive().Where(e => e.EmployeeID == b.EmployeeID).Include(e => e.Department).Select(m => m.Department.DepartmentName).FirstOrDefault(),
                         Salary = employeeSalarySettingsRepository.AllActive().Where(e => e.EmployeeID == b.EmployeeID).Select(x => x.Salary).FirstOrDefault(),
                         //Bonus = CalculateBonus(b.EmployeeID),
                         //Deduction = CalculateDeduction(b.EmployeeID),
                         //NetSalary = CalculateNetSalary(b.EmployeeID)

                     });

                return result;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");

                return new PaginationService<EmployeeBaseBenefits, PayRollEmpSalaryGetAllVM>.PaginationResult<PayRollEmpSalaryGetAllVM>
                {
                    Data = new List<PayRollEmpSalaryGetAllVM>(),
                    TotalCount = 0
                };
            }
        }



        //
        #endregion

        #region Get PaySlip 
        public async Task<CommonReturnViewModel> GetPaySlip(int id)
        {

            try
            {
                var baseQuery = await _employeeOfficeInfoRepository.AllActive()
                    .Include(x => x.Employee).ThenInclude(x => x.EmployeeSalarySettingsEmployee)
                    .Where(x => x.EmployeeID == id)
                    .Include(x => x.Organization).ThenInclude(x => x.EmployeeAllowances)
                    .FirstOrDefaultAsync();

                if (baseQuery == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Employee not found."
                    };
                }

                // Get Basic Salary
                decimal? basicsalary = await employeeSalarySettingsRepository.AllActive()
                    .Where(x => x.EmployeeID == id)
                    .Select(x => x.Salary)
                    .FirstOrDefaultAsync();

                // Fetch Allowances
                var allowancesList = await employeeAllowancesRepository.AllActive()
                    .Include(ea => ea.EmployeeAllowanceType)
                    .Include(ea => ea.EmployeeAllowanceSetup)
                    .Where(ea => ea.OrganizationID == baseQuery.OrganizationID && ea.IsActive == true)
                    .ToListAsync();

                var allowanceVMs = new List<AllowanceVM>();
                //decimal amount = 0;
               // string display = "";

                //foreach (var allowance in allowancesList)
                //{

                //    // Pick the setup for this employee’s salary range
                //    var setup = allowance.EmployeeAllowanceSetup
                //     .Where(s => (s.SalaryMin == null || basicsalary >= s.SalaryMin) &&
                //    (s.SalaryMax == null || basicsalary <= s.SalaryMax))
                //  .OrderByDescending(s => s.EffectiveDate ?? DateTime.MinValue)
                //   .FirstOrDefault();

                //    // fallback: if no matching range, take the latest effective setup
                //    if (setup == null)
                //    {
                //        setup = allowance.EmployeeAllowanceSetup
                //            .OrderByDescending(s => s.EffectiveDate ?? DateTime.MinValue)
                //            .FirstOrDefault();
                //    }


                //    if (setup != null)
                //    {


                //        if (setup.CalculationType?.CalculationTypeName == "Percentage")
                //        {
                //            amount = (decimal)(basicsalary * (setup.Value ?? 0) / 100);
                //            display = $"{setup.Value}%";
                //        }
                //        else if (setup.CalculationType?.CalculationTypeName == "Fixed")
                //        {
                //            amount = setup.Value ?? 0;
                //            display = $"{amount} (Fixed)";
                //        }
                //        else
                //        {
                //            amount = setup.Value ?? 0;
                //            display = $"{amount}";
                //        }

                //        allowanceVMs.Add(new AllowanceVM
                //        {
                //            Type = allowance.EmployeeAllowanceType.EmployeeAllowanceTypeName,
                //            Amount = amount,
                //            DisplayValue = display
                //        });
                //    }
                //}
                foreach (var allowance in allowancesList)
                {
                    var setup = allowance.EmployeeAllowanceSetup
                        .Where(s => (s.SalaryMin == null || basicsalary >= s.SalaryMin) &&
                                    (s.SalaryMax == null || basicsalary <= s.SalaryMax))
                        .OrderByDescending(s => s.EffectiveDate ?? DateTime.MinValue)
                        .FirstOrDefault();

                    if (setup == null)
                    {
                        setup = allowance.EmployeeAllowanceSetup
                            .OrderByDescending(s => s.EffectiveDate ?? DateTime.MinValue)
                            .FirstOrDefault();
                    }

                    if (setup != null)
                    {
                        decimal allowanceSalary = 0;
                        string display = "";

                        if (setup.CalculationType?.CalculationTypeName == "Percentage")
                        {
                            allowanceSalary = (decimal)(basicsalary * (setup.Value ?? 0) / 100);
                            display = $"{setup.Value}% of Basic";
                        }
                        else // Fixed
                        {
                            allowanceSalary = setup.Value ?? 0;
                            display = $"{allowanceSalary} (Fixed)";
                        }

                        allowanceVMs.Add(new AllowanceVM
                        {
                            Type = allowance.EmployeeAllowanceType.EmployeeAllowanceTypeName,
                            Amount = setup.Value ?? 0,      // raw DB value
                            DisplayValue = display,
                            AllowanceSalary = allowanceSalary
                        });
                    }
                }
                // Create VM
                var paySlipVM = new PayRollPaySlipEmpVM
                {
                    OrganizationName = baseQuery.Organization?.OrganizationName ?? "",
                    OrganizationAddress = baseQuery.Organization?.Address ?? "",
                    OrganizationEmailAddress = baseQuery?.Organization?.EmailAddress ?? "",
                    OrganizationLogoPic = baseQuery?.Organization.LogoLink ?? "",
                    EmployeeName = $"{baseQuery?.Employee.FirstName} {baseQuery?.Employee.LastName}",
                    EmployeeAddress = $"{baseQuery.Employee.State} {baseQuery.Employee.City},{baseQuery.Employee.HouseNo} {baseQuery.Employee.PostalCode}",
                    EmployeeEmail = baseQuery.Employee.Email,
                    BasicSalary = basicsalary,
                    Allowances = allowanceVMs,
                    //TotalSalary = (decimal)(basicsalary + allowanceVMs.Sum(a => a.Amount))
                    TotalSalary = (decimal)(basicsalary + allowanceVMs.Sum(a => a.AllowanceSalary)* basicsalary/100)
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
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }


            //try
            //{
            //    var baseQuery = await _employeeOfficeInfoRepository.AllActive().Include(x=>x.Employee).ThenInclude(x=>x.EmployeeSalarySettingsEmployee)
            //        .Where(x => x.EmployeeID == id).Include(x => x.Organization).ThenInclude(x=>x.EmployeeAllowances).FirstOrDefaultAsync();

            //    if (baseQuery == null)
            //    {
            //        return new CommonReturnViewModel
            //        {
            //            Success = false,
            //            Message = "Employee not found."
            //        };
            //    }
            //    var allowances = await employeeAllowancesRepository.AllActive()
            //      .Include(ea => ea.EmployeeAllowanceType)   // type of allowance (e.g., DA, HRA, Bonus)
            //      .Include(ea => ea.EmployeeAllowanceSetup) // allowance setup values
            //      .Where(ea => ea.OrganizationID == baseQuery.OrganizationID && ea.IsActive == true)
            //      .Select(ea => new
            //      {
            //          AllowanceID = ea.EmployeeAllowanceID,
            //          AllowanceType = ea.EmployeeAllowanceType.EmployeeAllowanceTypeName, 
            //          Setups = ea.EmployeeAllowanceSetup.Select(s => new
            //          {
            //              MinSalary = s.SalaryMin,
            //              MaxSalary = s.SalaryMax,
            //              CalculationType = s.CalculationType.CalculationTypeName,
            //              Value = s.Value,
            //              EffectiveDate = s.EffectiveDate
            //          }).ToList()
            //      }).ToListAsync();

            //    //employeeAllowancesRepository
            //    var basicsalary =await employeeSalarySettingsRepository.AllActive().Where(x=>x.EmployeeID==id).Select(x=>x.Salary).FirstOrDefaultAsync();
            //    var paySlipVM = new PayRollPaySlipEmpVM
            //    {
            //        OrganizationName = baseQuery.Organization?.OrganizationName ?? "", 
            //        OrganizationAddress=baseQuery.Organization?.Address ?? "",
            //        OrganizationEmailAddress=baseQuery?.Organization?.EmailAddress ?? "",
            //        OrganizationLogoPic=baseQuery?.Organization.LogoLink ?? "",
            //        EmployeeName=$"{baseQuery?.Employee.FirstName} {baseQuery?.Employee.LastName}",
            //        EmployeeAddress =$"{baseQuery.Employee.State} {baseQuery.Employee.City},{baseQuery.Employee.HouseNo} {baseQuery.Employee.PostalCode}",
            //        EmployeeEmail=baseQuery.Employee.Email,
            //        BasicSalary=basicsalary,

            //    };

            //    return new CommonReturnViewModel
            //    {
            //        Success = true,
            //        Data = paySlipVM,
            //        Message = "Payslip retrieved successfully."
            //    };
            //}
            //catch (Exception ex)
            //{
            //    // Log exception if logging is implemented
            //    return new CommonReturnViewModel
            //    {
            //        Success = false,
            //        Message = $"An error occurred: {ex.Message}"
            //    };
            //}
        }
        #endregion

        //private decimal CalculateSalary(int? employeeId)
        //{
        //    var salarySettings = employeeSalarySettingsRepository.AllActive()
        //        .Where(e => e.EmployeeID == employeeId)
        //        .FirstOrDefault();
        //    var baseAllowances = _employeeBaseAllowancesRepository.AllActive()
        //        .Where(e => e.EmployeeID == employeeId)
        //        .FirstOrDefault();
        //    var allowances = employeeAllowancesRepository.AllActive()
        //        .Where(e => e.OrganizationID == salarySettings?.OrganizationID)
        //        .FirstOrDefault();

        //    decimal basicSalary = salarySettings?.Salary ?? 0; // Basic + DA (assume DA is included or add separately)
        //    decimal hra = 0, medical = 0, conveyance = 0, specialAllowance = 0;

        //    // House Rent Allowance
        //    if (baseAllowances?.IsHouseRentAllowancePercentageEnabled == true && baseAllowances.HouseRentAllowancePercentage.HasValue)
        //    {
        //        hra = basicSalary * (baseAllowances.HouseRentAllowancePercentage.Value / 100);
        //    }
        //    else if (allowances?.IsHouseRentAllowanceEnabled == true && allowances.HouseRentAllowanceRate.HasValue)
        //    {
        //        hra = allowances.HouseRentAllowanceRate.Value;
        //    }

        //    // Medical Allowance
        //    if (baseAllowances?.IsMedicalAllowancePercentageEnabled == true && baseAllowances.MedicalAllowancePercentage.HasValue)
        //    {
        //        medical = basicSalary * (baseAllowances.MedicalAllowancePercentage.Value / 100);
        //    }
        //    else if (allowances?.IsMedicalAllowanceEnabled == true && allowances.MedicalAllowanceRate.HasValue)
        //    {
        //        medical = allowances.MedicalAllowanceRate.Value;
        //    }

        //    // Conveyance Allowance
        //    if (baseAllowances?.IsConveyanceAllowancePercentageEnabled == true && baseAllowances.ConveyanceAllowancePercentage.HasValue)
        //    {
        //        conveyance = basicSalary * (baseAllowances.ConveyanceAllowancePercentage.Value / 100);
        //    }
        //    else if (allowances?.IsConveyanceAllowanceEnabled == true && allowances.ConveyanceAllowanceRate.HasValue)
        //    {
        //        conveyance = allowances.ConveyanceAllowanceRate.Value;
        //    }

        //    // Special Allowance (Mobile + Internet + Shift)
        //    if (baseAllowances != null)
        //    {
        //        specialAllowance += (baseAllowances.IsMobileAllowanceEnabled ? baseAllowances.MobileAllowance ?? 0 : 0) +
        //                           (baseAllowances.IsInternetAllowanceEnabled ? baseAllowances.InternetAllowance ?? 0 : 0) +
        //                           (baseAllowances.IsShiftAllowanceEnabled ? baseAllowances.ShiftAllowance ?? 0 : 0);
        //    }
        //    if (allowances?.IsMobileInternetAllowanceEnabled == true)
        //    {
        //        specialAllowance += allowances.MobileInternetAllowance ?? 0;
        //    }

        //    return basicSalary + hra + medical + conveyance + specialAllowance;
        //}

        //private decimal CalculateBonus(int? employeeId)
        //{
        //    var baseBenefits = _employeeBaseBenefitsRepository.AllActive()
        //        .Where(e => e.EmployeeID == employeeId)
        //        .FirstOrDefault();
        //    var benefits = _employeeBaseBenefitsRepository.AllActive()
        //        .Where(e => e.OrganizationID == baseBenefits?.OrganizationID)
        //        .FirstOrDefault();

        //    decimal bonus = 0;

        //    // Performance Bonus
        //    if (baseBenefits?.IsPerformanceBonusEnabled == true && baseBenefits.PerformanceBonus.HasValue)
        //    {
        //        bonus += baseBenefits.PerformanceBonus.Value;
        //    }
        //    else if (benefits?.IsPerformanceBonusEnabled == true && benefits.PerformanceBonus.HasValue)
        //    {
        //        bonus += benefits.PerformanceBonus.Value;
        //    }

        //    // Festival Bonus
        //    if (baseBenefits?.IsFastivalBonusPercentageEnabled == true && baseBenefits.FastivalBonusPercentage.HasValue)
        //    {
        //        var basicSalary = employeeSalarySettingsRepository.AllActive()
        //            .Where(e => e.EmployeeID == employeeId)
        //            .Select(x => x.Salary)
        //            .FirstOrDefault();
        //        bonus += basicSalary * (baseBenefits.FastivalBonusPercentage.Value / 100);
        //    }
        //    else if (benefits?.IsFastivalBonusEnabled == true && benefits.FastivalBonusRate.HasValue)
        //    {
        //        bonus += benefits.FastivalBonusRate.Value;
        //    }

        //    return bonus;
        //}

        //private decimal CalculateDeduction(int? employeeId)
        //{
        //    var baseBenefits = _employeeBaseBenefitsRepository.AllActive()
        //        .Where(e => e.EmployeeID == employeeId)
        //        .FirstOrDefault();
        //    var benefits = _employeeBaseBenefitsRepository.AllActive()
        //        .Where(e => e.OrganizationID == baseBenefits?.OrganizationID)
        //        .FirstOrDefault();

        //    decimal deduction = 0;

        //    // Provident Fund
        //    if (baseBenefits?.IsProvidantFundEnabled == true && baseBenefits.ProvidantFundEmployeePercentage.HasValue)
        //    {
        //        var basicSalary = employeeSalarySettingsRepository.AllActive()
        //            .Where(e => e.EmployeeID == employeeId)
        //            .Select(x => x.Salary)
        //            .FirstOrDefault();
        //        deduction += basicSalary * (baseBenefits.ProvidantFundEmployeePercentage.Value / 100);
        //    }
        //    else if (benefits?.IsProvidentFundEnabled == true && benefits.ProvidentFundEmployeeContrebution.HasValue)
        //    {
        //        deduction += benefits.ProvidentFundEmployeeContrebution.Value;
        //    }

        //    // Professional Tax (assume a fixed value or fetch from a configuration)
        //    decimal professionalTax = 2000; // Replace with actual logic (e.g., from a table or config)
        //    deduction += professionalTax;

        //    // Home Loan (not in tables, assume custom deduction)
        //    decimal homeLoan = 20000; // Replace with actual logic
        //    deduction += homeLoan;

        //    // TDS (calculate based on tax slabs or fetch from a tax table)
        //    decimal tds = 10000; // Replace with actual tax calculation logic
        //    deduction += tds;

        //    return deduction;
        //}

        //private decimal CalculateNetSalary(int? employeeId)
        //{
        //    decimal salary = CalculateSalary(employeeId);
        //    decimal bonus = CalculateBonus(employeeId);
        //    decimal deduction = CalculateDeduction(employeeId);
        //    return salary + bonus - deduction;
        //}
        //
    }
}
