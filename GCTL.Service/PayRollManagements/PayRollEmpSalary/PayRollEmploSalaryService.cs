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
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Index.HPRtree;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GCTL.Service.PayRollManagements.PayRollEmpSalary
{
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
        int pk = 0;
        public PayRollEmploSalaryService(IGenericRepository<EmployeeBaseBenefits> employeeBenefitsRepository, IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfoRepository, IGenericRepository<Data.Models.Employees> employee, IGenericRepository<EmployeeSalarySettings> employeeSalarySettingsRepository, IGenericRepository<EmployeeAllowances> employeeAllowancesRepository, IGenericRepository<EmployeeBaseAllowances> employeeBaseAllowancesRepository, IGenericRepository<Benefits> benefitsRepository, IGenericRepository<BenefitSetups> benefitSetupsRepository, IGenericRepository<BenefitTypes> benefitType, IUserInfoService userInfoService, IGenericRepository<PaySlips> paySlipsRepository, IPdfFileHandler pdfFileHandlerService)
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
        }

        #region Get All Table 
        public async Task<PaginationService<EmployeeSalarySettings, PayRollEmpSalaryGetAllVM>.PaginationResult<PayRollEmpSalaryGetAllVM>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", int? organizationId = null, string imgSrcThumb = null, List<int>? deptID = null,List<int>? empID = null)
        {
            try
            {
                
                var query = employeeSalarySettingsRepository.AllActive().Include(x => x.Employee)
                    .ThenInclude(e => e.EmployeeOfficeInfoEmployee).ThenInclude(o => o.Department).AsQueryable();
                if (query == null)
                {
                    throw new InvalidOperationException("query source is null.");
                }

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

                Expression<Func<EmployeeBaseBenefits, object>> orderByExpression = currentSortColumn?.ToLower() switch
                {
                    "empid" => x => x.EmployeeID,
                     "empname" => x => x.Employee.FirstName + " " + x.Employee.LastName,
                    "empdept" => x => _employeeOfficeInfoRepository.AllActive().Where(e => e.EmployeeID == x.EmployeeID)
                                    .Select(d => d.Department.DepartmentName) .FirstOrDefault(),
                    "empsalary" => x => employeeSalarySettingsRepository.AllActive().Where(e => e.EmployeeID == x.EmployeeID).Select(x => x.Salary).FirstOrDefault(),
                    
                    _ => x => x.EmployeeID
                };


                var result = await PaginationService<EmployeeSalarySettings, PayRollEmpSalaryGetAllVM>.GetPaginatedData(
                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    currentSortColumn,
                    currentSortOrder,
                   term => b =>
                      string.IsNullOrEmpty(term) ||
                      EF.Functions.Like(b.EmployeeID.ToString(), $"%{term}%"),
                     b => new PayRollEmpSalaryGetAllVM
                     {
                         EmployeeId = b.EmployeeID,
                         EmployeeName = $"{b.Employee?.FirstName} {b.Employee?.LastName}",
                         EmployeeImage = !string.IsNullOrEmpty(b.Employee?.EmployeeImageFileName) ? imgSrcThumb + b.Employee.EmployeeImageFileName : "",
                         EmpDepartment = _employeeOfficeInfoRepository.AllActive().Where(e => e.EmployeeID == b.EmployeeID).Include(e => e.Department).Select(m => m.Department.DepartmentName).FirstOrDefault(),
                         Salary = employeeSalarySettingsRepository.AllActive().Where(e => e.EmployeeID == b.EmployeeID).Select(x => x.Salary).FirstOrDefault(),
                         //Deduction = CalculateDeduction(b.EmployeeID),


                     });
                
                foreach (var item in result.Data)
                {
                    try
                    {
                        var paySlip = await GetPaySlip((int)item.EmployeeId);
                        var a = paySlip.Data as PayRollPaySlipEmpVM;
                        item.Bonus = a?.TotalBonus ?? 0;
                        item.NetSalary = a?.TotalSalary
                            ?? employeeSalarySettingsRepository
                                .AllActive()
                                .Where(e => e.EmployeeID == item.EmployeeId)
                                .Select(x => x.Salary)
                                .FirstOrDefault();
                    }
                    catch (Exception exRow)
                    {
                        // 🔹 Specific error log for this employee
                        await userInfoService.ActionLogExceptionAsync("Pay Slip Row", exRow, item.EmployeeId, ActionName.Error);
                    }
                }
                return result;


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");

                await userInfoService.ActionLogExceptionAsync("Pay Slip Table", ex, null, ActionName.Error);
                return new PaginationService<EmployeeSalarySettings, PayRollEmpSalaryGetAllVM>.PaginationResult<PayRollEmpSalaryGetAllVM>
                {
                    Data = new List<PayRollEmpSalaryGetAllVM>(),
                    TotalCount = 0
                };
            }
        }



        //
        #endregion


        #region Save PaySlip
        public async Task<CommonReturnViewModel> SaveAsync(PayRollEmpSalarySaveVM entityVM)
        {
            var result = new CommonReturnViewModel();
            
            if (entityVM == null)
            {
                result.Success = false;
                result.Message = "Invalid payslip data!";
                return result;
            }
            decimal? basicsalary = await employeeSalarySettingsRepository.AllActive()
                    .Where(x => x.EmployeeID ==entityVM.EmployeeID).Select(x => x.Salary) .FirstOrDefaultAsync();
            await paySlipsRepository.BeginTransactionAsync();

            try
            {
                
                var entity = new PaySlips
                {
                    EmployeeID = entityVM.EmployeeID,
                    BasicSalary = (decimal)basicsalary,
                    PayPeriodStart = entityVM.PayPeriodStart,
                    PayPeriodEnd = entityVM.PayPeriodEnd,
                    IsPaid = entityVM.IsPaid,
                    LIP = entityVM.LIP,
                    LMAC = entityVM.LMAC,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = entityVM.CreatedBy,
                };

                await paySlipsRepository.AddAsync(entity);
                await paySlipsRepository.CommitTransactionAsync();
                pk=entity.PaySlipID;
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
        #endregion


        #region Get PaySlip 

        
        public async Task<CommonReturnViewModel> GetPaySlip(int id)
        {
            try
            {
                var baseQuery = await _employeeOfficeInfoRepository.AllActive()
                    .Include(x => x.Employee)
                        .ThenInclude(x => x.EmployeeSalarySettingsEmployee)
                    .Where(x => x.EmployeeID == id)
                    .Include(x => x.Organization)
                        .ThenInclude(x => x.EmployeeAllowances)
                    .FirstOrDefaultAsync();

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

                        if (baseBenefit.CalculationTypeID == 1) // Percentage
                        {
                            benefitSalary = (decimal)(basicsalary * (baseBenefit.BenefitValue ?? 0) / 100);
                            display = $"{(baseBenefit.BenefitValue ?? 0).ToString("0")}%";
                        }
                        else // Fixed
                        {
                            benefitSalary = (decimal)(basicsalary * (baseBenefit.BenefitValue ?? 0) / 100);
                            display = $"{Math.Floor(baseBenefit.BenefitValue ?? 0)}%";
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
                        .Where(b => b.OrganizationID == baseQuery.OrganizationID && b.IsActive == true)
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

                            if (setup.CalculationTypeID == 1) // Percentage
                            {
                                benefitSalary = (decimal)(basicsalary * (setup.Value ?? 0) / 100);
                                display = $"{(setup.Value ?? 0).ToString("0")}%";
                            }
                            else // Fixed
                            {
                                benefitSalary = (decimal)(basicsalary * (setup.Value ?? 0) / 100);
                                display = $"{Math.Floor(setup.Value ?? 0)}%";
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

                    if (baseAllowances.CalculationTypeID == 1) // Percentage
                    {
                        allowanceSalary = (decimal)(basicsalary * (baseAllowances.AllowanceValue ?? 0) / 100);
                       // display = $"{baseAllowances.AllowanceValue}%";
                        display = $"{(baseAllowances.AllowanceValue ?? 0).ToString("0")}%"; // ✅ fixed
                    }
                    else // Fixed
                    {
                        allowanceSalary = (decimal)(basicsalary * (baseAllowances.AllowanceValue ?? 0) / 100);
                        display = $"{Math.Floor( baseAllowances.AllowanceValue ?? 0)}%";
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
                        .Include(ea => ea.EmployeeAllowanceSetup)
                        .Where(ea => ea.OrganizationID == baseQuery.OrganizationID && ea.IsActive == true)
                        .ToListAsync();

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
                               // display = $"{setup.Value}%";
                                display = $"{(setup.Value ?? 0).ToString("0")}%"; // ✅ fixed
                            }
                            else // Fixed
                            {
                                allowanceSalary = (decimal)(basicsalary*(setup.Value ?? 0)/100);
                                display = $"{Math.Floor( setup.Value ?? 0)}%";
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

                var paySlipVM = new PayRollPaySlipEmpVM
                {
                    OrganizationName = baseQuery.Organization?.OrganizationName ?? "",
                    OrganizationAddress = baseQuery.Organization?.Address ?? "",
                    OrganizationEmailAddress = baseQuery?.Organization?.EmailAddress ?? "",
                    //OrganizationLogoPic = baseQuery?.Organization?.LogoLink ?? "",
                    OrganizationLogoPic = baseQuery?.Organization?.LogoLink != null? $"/media/company/logo/{baseQuery.Organization.LogoLink}" : "",

                    EmployeeName = $"{baseQuery?.Employee.FirstName} {baseQuery?.Employee.LastName}",
                    EmployeeAddress = $"{baseQuery?.Employee.State} {baseQuery?.Employee.City},{baseQuery?.Employee.HouseNo} {baseQuery?.Employee.PostalCode}",
                    EmployeeEmail = baseQuery?.Employee.Email,
                    BasicSalary = basicsalary,
                    Allowances = allowanceVMs,
                    BeneFits = benefitsVMs,
                    TotalSalary = totalSalaryEarning,
                    SalaryInWords = NumberToWordsConverter.NumberToWords((int)totalSalaryEarning) + " Only",
                    TotalBonus = (allowanceVMs?.Sum(a => a.AllowanceSalary ?? 0) ?? 0)+ (benefitsVMs?.Sum(b => b.BenefitsSalary ?? 0) ?? 0),
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
                await userInfoService.ActionLogExceptionAsync("Pay Slip according to EmpoyeeID", ex, id, ActionName.Error);
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

        public async Task<byte[]> GeratePdf(int id)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            try
            {
                var payslipEntity = await paySlipsRepository.AllActive().Where(x=>x.PaySlipID==id).Select(x=>x.EmployeeID).FirstOrDefaultAsync();

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
                                    .FontSize(14).Bold();

                                // Payment Info
                                col.Item().PaddingVertical(5).BorderBottom(1).Row(row =>
                                {
                                    row.RelativeItem().Text(text =>
                                    {
                                        text.Span("Payslip No. ").Bold();
                                        text.Span(id.ToString());
                                    });

                                    row.RelativeItem().AlignRight().Text(text =>
                                    {
                                        text.Span("Payment Date ").Bold();
                                        text.Span($"{DateTime.Now:dd-MMM-yyyy hh:mm tt}");
                                    });
                                });

                                // Organization & Employee
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

                                        left.Item().Text(data.OrganizationName).Bold();
                                        left.Item().Text(data.OrganizationAddress);
                                        left.Item().Text(data.OrganizationEmailAddress);
                                    });

                                    row.RelativeItem().AlignRight().Column(right =>
                                    {
                                        right.Item().Text("Payment To").SemiBold();
                                        right.Item().Text(data.EmployeeName).Bold();
                                        right.Item().Text(data.EmployeeAddress);
                                        right.Item().Text(data.EmployeeEmail);
                                    });
                                });


                                // Earnings & Deductions
                                col.Item().Row(row =>
                                {
                                    // Earnings
                                    row.RelativeItem().Column(earn =>
                                    {
                                        earn.Item().Text("Earnings").Bold();

                                        earn.Item().Table(table =>
                                        {
                                            table.ColumnsDefinition(c =>
                                            {
                                                c.RelativeColumn();
                                                c.ConstantColumn(80);
                                            });

                                            // Basic
                                            table.Cell().Text("Basic");
                                            table.Cell().AlignRight().Text($"{data.BasicSalary?.ToString("N2")}");

                                            // Allowances
                                            foreach (var a in data.Allowances)
                                            {
                                                table.Cell().Text($"{a.Type} ({a.DisplayValue})");
                                                table.Cell().AlignRight().Text(Math.Floor(a.AllowanceSalary ?? 0).ToString("N2"));
                                            }

                                            // Benefits
                                            foreach (var b in data.BeneFits)
                                            {
                                                table.Cell().Text($"{b.Type} ({b.DisplayValue})");
                                                table.Cell().AlignRight().Text(Math.Floor(b.BenefitsSalary ?? 0).ToString("N2"));
                                            }

                                            // Total Earnings
                                            table.Cell().Text("Total Earnings").Bold();
                                            table.Cell().AlignRight().Text(data.TotalSalary.ToString("N2") ?? "0.00").Bold();
                                        });
                                    });

                                    // Deductions (static for now)
                                    row.RelativeItem().Column(ded =>
                                    {
                                        ded.Item().Text("Deductions").Bold();

                                        ded.Item().Table(table =>
                                        {
                                            table.ColumnsDefinition(c =>
                                            {
                                                c.RelativeColumn();
                                                c.ConstantColumn(80);
                                            });

                                            void AddDeduction(string label, decimal amount)
                                            {
                                                table.Cell().Text(label);
                                                table.Cell().AlignRight().Text(amount.ToString("N2"));
                                            }

                                            AddDeduction("Provident Fund", 1000);
                                            AddDeduction("Professional Tax", 2000);
                                            AddDeduction("ESI", 0);
                                            AddDeduction("Home Loan", 20000);
                                            AddDeduction("TDS", 10000);

                                            table.Cell().Text("Total Deductions").Bold();
                                            table.Cell().AlignRight().Text("35000.00").Bold();
                                        });
                                    });
                                });

                                // Net Pay
                                col.Item().PaddingTop(10).Text($"Net Pay: {(data.TotalSalary - 35000):N2}")
                                    .Bold().FontSize(12);

                                col.Item().Text(data.SalaryInWords);

                                // Signature
                                col.Item().PaddingTop(30).AlignRight().Column(c =>
                                {
                                    c.Item().Text("For Priya Jain").Bold();
                                    c.Item().PaddingTop(15).Text("Authorised Signatory");
                                });


                                // Footer
                                col.Item().AlignCenter().PaddingTop(20).Text("This is a system generated payslip.")
                                    .FontSize(9).Italic();
                            });
                        });
                    }).GeneratePdf(stream);

                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("PDF Generation Error: " + ex.Message);
                await userInfoService.ActionLogExceptionAsync("Pay Slip according to EmpoyeeID", ex, id, ActionName.Error);
                throw new Exception("Error while generating Pay Slip PDF.");
            }
        }



    }
}
