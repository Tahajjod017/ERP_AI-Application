using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Entity;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeAllowance;
using GCTL.Core.ViewModels.Employee.EmployeeBenifit;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Employees.EmployeeBenifit;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.Employees.EmployeeAllowance
{
    public class EmployeeAllowanceService : IEmployeeAllowanceService
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.EmployeeBaseAllowances> _employeeAllowancesRepository;
        private readonly IGenericRepository<EmployeeSalarySettings> _employeeSalaryRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOfficialRepository;

        private readonly IGenericRepository<EmployeeAllowanceTypes> allowanceType;
        private readonly IGenericRepository<EmployeeAllowances> empAllowance;
        private readonly IGenericRepository<EmployeeAllowanceSetup> empAllowanceSetup;
        private readonly IUserInfoService _userInfoService;
        public EmployeeAllowanceService(IGenericRepository<EmployeeBaseAllowances> employeeAllowancesRepository, IGenericRepository<Data.Models.Employees> employeeRepository, IGenericRepository<EmployeeSalarySettings> employeeSalaryRepository, IGenericRepository<EmployeeOfficeInfo> employeeOfficialRepository, IGenericRepository<EmployeeAllowanceTypes> allowanceType, IGenericRepository<EmployeeAllowances> empAllowance, IGenericRepository<EmployeeAllowanceSetup> empAllowanceSetup, IUserInfoService userInfoService)
        {
            _employeeAllowancesRepository = employeeAllowancesRepository;
            _employeeRepository = employeeRepository;
            _employeeSalaryRepository = employeeSalaryRepository;
            _employeeOfficialRepository = employeeOfficialRepository;
            this.allowanceType = allowanceType;
            this.empAllowance = empAllowance;
            this.empAllowanceSetup = empAllowanceSetup;
            _userInfoService = userInfoService;
        }

        public async Task<EmployeeAdditionalPostViewModel> GetEmployeeAllowance(int employeeId)
        {
            var allowance = await (from eb in _employeeAllowancesRepository.AllActive()
                                   join emp in _employeeRepository.AllActive()
                                   on eb.EmployeeID equals emp.EmployeeID into empGroup // First left join
                                   from emp in empGroup.DefaultIfEmpty()

                                   join empSaley in _employeeSalaryRepository.AllActive()
                                   on emp.EmployeeID equals empSaley.EmployeeID into empSaleyGroup // Second left join
                                   from empSaley in empSaleyGroup.DefaultIfEmpty()

                                   join empOfficial in _employeeOfficialRepository.AllActive()
                                   on emp.EmployeeID equals empOfficial.EmployeeID into empOfficialGroup // Third left join
                                   from empOfficial in empOfficialGroup.DefaultIfEmpty()

                                   where eb.EmployeeID == employeeId

                                   select new EmployeeAdditionalPostViewModel
                                   {
                                       EmployeePersonalId = emp.EmployeeID,
                                       PersonalEmail = emp.Email ?? "N/A",
                                       PersonalPhone = emp.MobileNumber ?? "N/A",
                                       EmployeeBaseAllowanceID = eb.EmployeeBaseAllowanceID,

                                       OrganizationID = empOfficial.OrganizationID,
                                       

                                   }).FirstOrDefaultAsync();


            if (allowance != null)
            {
                return allowance;
            }
            else
            {
                var emp = await (from eb in _employeeRepository.AllActive()


                                 join empOfficial in _employeeOfficialRepository.AllActive()
                                 on eb.EmployeeID equals empOfficial.EmployeeID into empOfficialGroup
                                 from empOfficial in empOfficialGroup.DefaultIfEmpty()

                                 where eb.EmployeeID == employeeId

                                 select new EmployeeAdditionalPostViewModel
                                 {
                                     EmployeePersonalId = eb.EmployeeID,
                                     PersonalEmail = eb.Email ?? "N/A",
                                     PersonalPhone = eb.MobileNumber ?? "N/A",


                                     OrganizationID = empOfficial.OrganizationID,


                                 }).FirstOrDefaultAsync();

                return emp;
            }
        }



        #region Save Update

        public async Task<CommonReturnViewModel> Save(EmployeeAlowancePostViewModel22 model)
        {
            bool isUpdated = false;
            bool isInserted = false;
            try
            {
                foreach (var benefit in model.Benefits)
                {
                    var existing = await _employeeAllowancesRepository.AllActive()
                        .FirstOrDefaultAsync(b => b.EmployeeAllowanceID == benefit.BenefitID && b.EmployeeID == model.EmployeeId);

                    if (existing != null)
                    {
                        // Update
                        existing.CalculationTypeID = benefit.BaseCalculationTypeID;
                        existing.AllowanceValue = benefit.BaseValue;
                        existing.LIP = model.LIP;
                        existing.LMAC = model.LMAC;
                        existing.UpdatedAt = DateTime.UtcNow;
                        existing.UpdatedBy = model.UpdatedBy;
                        await _employeeAllowancesRepository.UpdateAsync(existing);
                        isUpdated = true;
                    }
                    else
                    {
                        // Insert new
                        var newBenefit = new EmployeeBaseAllowances
                        {
                            EmployeeID = model.EmployeeId,
                            EmployeeAllowanceID = benefit.BenefitID,
                            CalculationTypeID = benefit.BaseCalculationTypeID,
                            AllowanceValue = benefit.BaseValue,
                            LIP = model.LIP,
                            LMAC = model.LMAC,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = model.CreatedBy,


                        };
                        await _employeeAllowancesRepository.AddAsync(newBenefit);
                        isInserted = true;
                    }
                }
            }
            catch (Exception ex)
            {
                await _userInfoService.ActionLogExceptionAsync("Employee Benefit", ex, 0, ActionName.Error);
                throw;
            }
            string message;
            if (isUpdated && !isInserted)
                message = "Updated successfully.";
            else if (isInserted && !isUpdated)
                message = "Saved successfully.";
            else if (isUpdated && isInserted)
                message = "Saved and updated successfully.";
            else
                message = "No changes made.";
            return new CommonReturnViewModel
            {
                Success = true,
                Message = message
            };
        }
        #endregion



        public async Task<List<CommonSelectVMMMAllowance>> SelectAsync(int id)
        {
            List<CommonSelectVMMMAllowance> result = new List<CommonSelectVMMMAllowance>();
            try
            {
                var orgaid = await _employeeOfficialRepository.AllActive().Where(x => x.EmployeeID == id).Select(x => x.OrganizationID).FirstOrDefaultAsync();

                var employeeSalary = await _employeeSalaryRepository.AllActive().Where(x => x.EmployeeID == id).Select(x => x.Salary).FirstOrDefaultAsync();

                var benefitTypes = await allowanceType.AllActive().Where(x => x.OrganizationID == orgaid && x.IsApplyOnGrossSalary == true).ToListAsync();

                var benefits = await this.empAllowance.AllActive().Include(b => b.EmployeeAllowanceSetup).Include(b => b.EmployeeAllowanceType).Where(b => b.OrganizationID == orgaid).ToListAsync();

                var empBasebenefit = await _employeeAllowancesRepository.AllActive().Where(x => x.EmployeeID == id)
      .Select(x => new
      {
          EmployeeAllowanceID = x.EmployeeAllowanceID,
          CalculationTypeID = x.CalculationTypeID,
          BenefitValue = x.AllowanceValue,
      }).ToListAsync();




                // result = benefits.Where(b => benefitTypes.Any(bt => bt.EmployeeAllowanceTypeID == b.EmployeeAllowanceTypeID)).GroupBy(b => new { b.EmployeeAllowanceTypeID, b.EmployeeAllowanceType.EmployeeAllowanceTypeName })
                //.Select(g => new CommonSelectVMMMAllowance
                //{
                //    Id = g.Key.EmployeeAllowanceTypeID,
                //    Name = g.Key.EmployeeAllowanceTypeName,
                //    EmpBenefitVMM = g.Select(b =>
                //    {
                //        var matchedSetup = b.EmployeeAllowanceSetup
                //            .FirstOrDefault(s => s.SalaryMin <= employeeSalary && s.SalaryMax >= employeeSalary);
                //        var baseBenefit = empBasebenefit.FirstOrDefault(x => x.EmployeeAllowanceID == b.EmployeeAllowanceID);

                //        return new EmpAllowanceVMMM
                //        {
                //            OrganizationID = b.OrganizationID,
                //            BenefitID = b.EmployeeAllowanceID,
                //            CalculationTypeID = matchedSetup?.CalculationTypeID ?? 0,
                //            Value = matchedSetup?.Value ?? 0,
                //            BaseBenefitID = baseBenefit?.EmployeeAllowanceID ?? b.EmployeeAllowanceID,
                //            BaseCalculationTypeID = baseBenefit?.CalculationTypeID ?? (matchedSetup?.CalculationTypeID ?? 0),
                //            BaseValue = baseBenefit?.BenefitValue ?? (matchedSetup?.Value ?? 0),
                //        };
                //    }).ToList()
                //}).ToList();

                result = benefits
    .Where(b => benefitTypes.Any(bt => bt.EmployeeAllowanceTypeID == b.EmployeeAllowanceTypeID))
    .GroupBy(b => new { b.EmployeeAllowanceTypeID, b.EmployeeAllowanceType.EmployeeAllowanceTypeName })
    .Select(g => new CommonSelectVMMMAllowance
    {
        Id = g.Key.EmployeeAllowanceTypeID,
        Name = g.Key.EmployeeAllowanceTypeName,
        EmpBenefitVMM = g.Select(b =>
        {
            var matchedSetup = b.EmployeeAllowanceSetup
                .FirstOrDefault(s => s.SalaryMin <= employeeSalary && s.SalaryMax >= employeeSalary);

            if (matchedSetup == null)
                return null;  // ✅ Ignore rows without setup

            var baseBenefit = empBasebenefit.FirstOrDefault(x => x.EmployeeAllowanceID == b.EmployeeAllowanceID);

            return new EmpAllowanceVMMM
            {
                OrganizationID = b.OrganizationID,
                BenefitID = b.EmployeeAllowanceID,
                CalculationTypeID = matchedSetup.CalculationTypeID, // ✅ no fallback
                Value = matchedSetup.Value,                        // ✅ no fallback
                BaseBenefitID = baseBenefit?.EmployeeAllowanceID ?? b.EmployeeAllowanceID,
                BaseCalculationTypeID = baseBenefit?.CalculationTypeID ?? matchedSetup.CalculationTypeID,
                BaseValue = baseBenefit?.BenefitValue ?? matchedSetup.Value,
            };
        })
        .Where(x => x != null) // remove ignored rows
        .ToList()
    })
    .Where(r => r.EmpBenefitVMM.Any()) // remove groups with no valid rows
    .ToList();


                return result;
            }


            catch (Exception ex)
            {
                await _userInfoService.ActionLogExceptionAsync("Employee Benefit", ex, id, ActionName.Error);
                Console.WriteLine(ex);
                return new List<CommonSelectVMMMAllowance>();
            }
        }

    }
}
