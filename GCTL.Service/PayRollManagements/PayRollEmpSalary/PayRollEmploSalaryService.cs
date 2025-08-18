using GCTL.Core.Repository;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpAllowance;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpSalary;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using GCTL.Service.PayRollManagements.PayRollEmpAllowance;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollEmpSalary
{
    public class PayRollEmploSalaryService : IPayRollEmpSalaryService
    {
        private readonly IGenericRepository<EmployeeBaseBenefits> _employeeBenefitsRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOfficeInfoRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> employee;
        private readonly IGenericRepository<EmployeeSalarySettings> employeeSalarySettingsRepository;

        public PayRollEmploSalaryService(IGenericRepository<EmployeeBaseBenefits> employeeBenefitsRepository, IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfoRepository, IGenericRepository<Data.Models.Employees> employee, IGenericRepository<EmployeeSalarySettings> employeeSalarySettingsRepository = null)
        {
            _employeeBenefitsRepository = employeeBenefitsRepository;
            _employeeOfficeInfoRepository = employeeOfficeInfoRepository;
            this.employee = employee;
            this.employeeSalarySettingsRepository = employeeSalarySettingsRepository;
        }

        public async Task<PaginationService<EmployeeBaseBenefits, PayRollEmpSalaryGetAllVM>.PaginationResult<PayRollEmpSalaryGetAllVM>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", int? organizationId = null)
        {
            try
            {
                // 🔹 Step 3: Base query with includes
                var query = _employeeBenefitsRepository.AllActive().Include(x => x.Employee).OrderByDescending(x => x.EmployeeBaseBenefitID).AsQueryable();
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

                // For approver Step


                //
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
                        EmployeeImage = !string.IsNullOrEmpty(b.Employee?.EmployeeImageFileName) ? b.Employee.EmployeeImageFileName : "",
                        EmpDepartment = _employeeOfficeInfoRepository.AllActive().Where(e => e.EmployeeID == b.EmployeeID).Include(e => e.Department).Select(m => m.Department.DepartmentName).FirstOrDefault(),
                        Salary = employeeSalarySettingsRepository.AllActive().Where(e => e.EmployeeID == b.EmployeeID).Select(x => x.Salary).FirstOrDefault(),



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
    }
}
