using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmployeeUpdateVM;
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
        private readonly IGenericRepository<Benefits> benefitsRepository;
        private readonly IGenericRepository<BenefitSetups> benefitSetupsRepository;
        private readonly IGenericRepository<BenefitTypes> benefitType;
        public PayRollEmploSalaryService(IGenericRepository<EmployeeBaseBenefits> employeeBenefitsRepository, IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfoRepository, IGenericRepository<Data.Models.Employees> employee, IGenericRepository<EmployeeSalarySettings> employeeSalarySettingsRepository, IGenericRepository<EmployeeAllowances> employeeAllowancesRepository, IGenericRepository<EmployeeBaseAllowances> employeeBaseAllowancesRepository, IGenericRepository<Benefits> benefitsRepository, IGenericRepository<BenefitSetups> benefitSetupsRepository , IGenericRepository<BenefitTypes> benefitType)
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
                // For Bonus 
                decimal? basicsalary = await employeeSalarySettingsRepository.AllActive()
                   .Where(x => x.EmployeeID == id).Select(x => x.Salary).FirstOrDefaultAsync();

                var baseBenefits = await _employeeBaseBenefitsRepository.AllActive().Where(x => x.EmployeeID == id).FirstOrDefaultAsync();

                // Get Basic Salary
                //decimal healthInsurance = (baseBenefits?.IsHealthInsuranceEnabled == true ? baseBenefits.HealthInsurance ?? 0 : 0);
                //decimal performanceBonus = (baseBenefits?.IsPerformanceBonusEnabled == true ? baseBenefits.PerformanceBonus ?? 0 : 0);
                //decimal yearlyEndBonus = (baseBenefits?.IsYearlyEndBonusTypeIDEnabled == true ?  0 : 0);
                //decimal festivalBonus = (baseBenefits?.IsFastivalBonusPercentageEnabled == true ?
                //    (baseBenefits.FastivalBonusPercentage ?? 0) * (basicsalary ?? 0) / 100 : 0);
                //decimal providentFund = (baseBenefits?.IsProvidantFundEnabled == true ?
                //    (baseBenefits.ProvidantFundEmployeePercentage ?? 0) * (basicsalary ?? 0) / 100 : 0);

                // decimal totalBonus= healthInsurance+ performanceBonus+yearlyEndBonus+festivalBonus+providentFund;
                decimal totalBonus = 0 + 0 + 0 + 0 + 0;
                //
                // Fetch benefits from DB
                var benefitsList = await benefitsRepository.AllActive()
                    .Include(b => b.BenefitType)
                    .Include(b => b.BenefitSetups)
                    .Where(b => b.OrganizationID == baseQuery.OrganizationID && b.IsActive == true).ToListAsync();

                var benefitsVMs = new List<BeneFitsVM>();

                foreach (var benefit in benefitsList)
                {
                    var setup = benefit.BenefitSetups
                        .Where(s => (s.SalaryMin == null || basicsalary >= s.SalaryMin) &&
                                    (s.SalaryMax == null || basicsalary <= s.SalaryMax))
                        .OrderByDescending(s => s.CreatedAt ?? DateTime.MinValue).FirstOrDefault();

                    if (setup == null)
                    {
                        setup = benefit.BenefitSetups
                            .OrderByDescending(s => s.CreatedAt ?? DateTime.MinValue)
                            .FirstOrDefault();
                    }

                    if (setup != null)
                    {
                        decimal benefitSalary = 0;
                        string display = "";

                        if (setup.CalculationTypeID == 1) 
                        {
                            benefitSalary = (decimal)(basicsalary * (setup.Value ?? 0) / 100);
                            display = $"{setup.Value}% of Basic";
                        }
                        else // Fixed
                        {
                            benefitSalary = setup.Value ?? 0;
                            display = $"{benefitSalary} (Fixed)";
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

                

                // Fetch Allowances
                var allowancesList = await employeeAllowancesRepository.AllActive()
                    .Include(ea => ea.EmployeeAllowanceType)
                    .Include(ea => ea.EmployeeAllowanceSetup)
                    .Where(ea => ea.OrganizationID == baseQuery.OrganizationID && ea.IsActive == true).ToListAsync();

                var allowanceVMs = new List<AllowanceVM>();
                foreach (var allowance in allowancesList)
                {
                    var setup = allowance.EmployeeAllowanceSetup
                        .Where(s => (s.SalaryMin == null || basicsalary >= s.SalaryMin) &&
                                    (s.SalaryMax == null || basicsalary <= s.SalaryMax))
                        
                        .FirstOrDefault();

                   

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
                            Amount = setup.Value ?? 0,    
                            DisplayValue = display,
                            AllowanceSalary = allowanceSalary
                        });
                    }
                }

                //



                //
                var totalSalaryEarning = (basicsalary ?? 0) + (allowanceVMs?.Sum(a => a.AllowanceSalary ?? 0) ?? 0) +(benefitsVMs?.Sum(b => b.BenefitsSalary ?? 0) ?? 0);

                var paySlipVM = new PayRollPaySlipEmpVM
                {
                    OrganizationName = baseQuery.Organization?.OrganizationName ?? "",
                    OrganizationAddress = baseQuery.Organization?.Address ?? "",
                    OrganizationEmailAddress = baseQuery?.Organization?.EmailAddress ?? "",
                    OrganizationLogoPic = baseQuery?.Organization?.LogoLink ?? "",
                    EmployeeName = $"{baseQuery?.Employee.FirstName} {baseQuery?.Employee.LastName}",
                    EmployeeAddress = $"{baseQuery?.Employee.State} {baseQuery?.Employee.City},{baseQuery?.Employee.HouseNo} {baseQuery?.Employee.PostalCode}",
                    EmployeeEmail = baseQuery?.Employee.Email,
                    BasicSalary = basicsalary,
                    Allowances = allowanceVMs ,
                    BeneFits= benefitsVMs,
                    TotalSalary = totalSalaryEarning,
                    SalaryInWords = NumberToWordsConverter.NumberToWords((int)totalSalaryEarning) + " Only",
                    TotalBonus= totalBonus

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
        }
        #endregion

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


        
    }
}
