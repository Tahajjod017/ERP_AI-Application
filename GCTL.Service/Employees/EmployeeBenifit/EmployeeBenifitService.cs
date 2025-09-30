using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Entity;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeBenifit;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmployeeUpdateVM;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.Employees.EmployeeBenifit
{
    public class EmployeeBenifitService : IEmployeeBenifitService
    {
        private readonly IGenericRepository<EmployeeSalarySettings> _employeeSalaryRepository;
        private readonly IGenericRepository<EmployeeBaseBenefits> _employeeBenifitRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOfficialRepository;
        private readonly IGenericRepository<Benefits> benefits;
        private readonly IGenericRepository<BenefitTypes> benefitTypesRepository;
        private readonly IGenericRepository<BenefitSetups> benefitSetupRepository;
        private readonly IUserInfoService userInfoService;
        public EmployeeBenifitService(IGenericRepository<EmployeeBaseBenefits> employeeBenifitRepository, IGenericRepository<Data.Models.Employees> employeeRepository, IGenericRepository<EmployeeSalarySettings> employeeSalaryRepository, IGenericRepository<EmployeeOfficeInfo> employeeOfficialRepository, IGenericRepository<Benefits> benefits = null, IGenericRepository<BenefitTypes> benefitTypesRepository = null, IGenericRepository<BenefitSetups> benefitSetupRepository = null, IUserInfoService userInfoService = null)
        {
            _employeeBenifitRepository = employeeBenifitRepository;
            _employeeRepository = employeeRepository;
            _employeeSalaryRepository = employeeSalaryRepository;
            _employeeOfficialRepository = employeeOfficialRepository;
            this.benefits = benefits;
            this.benefitTypesRepository = benefitTypesRepository;
            this.benefitSetupRepository = benefitSetupRepository;
            this.userInfoService = userInfoService;
        }



        public async Task<EmployeeBenifitPostViewModel> GetEmployeeBenefitsAsync(string employeeId)
        {
            if (!int.TryParse(employeeId, out var id))
                return null;

            var benefits = await (from eb in _employeeBenifitRepository.AllActive()
                                  join emp in _employeeRepository.AllActive()
                                  on eb.EmployeeID equals emp.EmployeeID into empGroup 
                                  from emp in empGroup.DefaultIfEmpty()

                                  join empSaley in _employeeSalaryRepository.AllActive()
                                  on emp.EmployeeID equals empSaley.EmployeeID into empSaleyGroup 
                                  from empSaley in empSaleyGroup.DefaultIfEmpty()

                                  join empOff in _employeeOfficialRepository.AllActive()
                                    on emp.EmployeeID equals empOff.EmployeeID into empOffGroup
                                  from empOff in empOffGroup.DefaultIfEmpty()

                                  where eb.EmployeeID == id

                                  select new EmployeeBenifitPostViewModel
                                  {
                                      EmployeeBaseBenefitID = eb.EmployeeBaseBenefitID,
                                      EmployeePersonalId = (int)eb.EmployeeID,
                                      OrganizationID = empOff.OrganizationID,

                                      PersonalEmail = emp.Email ?? "N/A",
                                      PersonalPhone = emp.MobileNumber ?? "N/A",
                                      //IsBenifitEnabled = true,
                                      HealthInsurance = eb.HealthInsurance ?? 0,
                                      IsHealthInsuranceEnabled = eb.IsHealthInsuranceEnabled,
                                      PerformanceBonus = eb.PerformanceBonus ?? 0,
                                      IsPerformanceBonusEnabled = eb.IsPerformanceBonusEnabled,
                                      YearlyEndBonusTypeID = eb.YearlyEndBonusTypeID ?? 0,
                                      IsYearlyEndBonusTypeIDEnabled = eb.IsYearlyEndBonusTypeIDEnabled,
                                      FastivalBonusPercentage = eb.FastivalBonusPercentage ?? 0,
                                      IsFastivalBonusPercentageEnabled = eb.IsFastivalBonusPercentageEnabled,
                                      ProvidantFundEmployeePercentage = eb.ProvidantFundEmployeePercentage ?? 0,
                                      ProvidantFundOrganizationPercentage = eb.ProvidantFundOrganizationPercentage ?? 0,
                                      IsProvidantFundEnabled = eb.IsProvidantFundEnabled,
                                      ServiceYearID = eb.ServiceYearID ?? 0,
                                      IsBenifitEnabled = empSaley.IsBenefitsEnabled
                                  }).FirstOrDefaultAsync();


            if (benefits != null)
            {
                return benefits;
            }
            else
            {
                var benefits2 = await (from  emp in _employeeRepository.AllActive()
                                     

                                    
                                      join empOff in _employeeOfficialRepository.AllActive()
                                        on emp.EmployeeID equals empOff.EmployeeID into empOffGroup
                                      from empOff in empOffGroup.DefaultIfEmpty()

                                      where emp.EmployeeID == id

                                      select new EmployeeBenifitPostViewModel
                                      {
                                         
                                          EmployeePersonalId = (int)emp.EmployeeID,
                                          OrganizationID = empOff.OrganizationID,

                                          PersonalEmail = emp.Email ?? "N/A",
                                          PersonalPhone = emp.MobileNumber ?? "N/A",
                                          
                                       
                                      }).FirstOrDefaultAsync();

                return benefits2;
            }

            
        }

        public async Task<CommonReturnViewModel> SaveOrUpdateEmployeeBenefitsAsync(EmployeeBenifitPostViewModel model)
        {
            try
            {
                var existingBenefit = await _employeeBenifitRepository.AllActive()
                .FirstOrDefaultAsync(e => e.EmployeeID == model.EmployeePersonalId);

                if (existingBenefit == null)
                {

                    var newBenefit = new EmployeeBaseBenefits
                    {
                        //EmployeeBaseBenefitID = model.EmployeeBaseBenefitID,
                        EmployeeID = model.EmployeePersonalId,

                        HealthInsurance = model.HealthInsurance,
                        IsHealthInsuranceEnabled = model.IsHealthInsuranceEnabled,
                        PerformanceBonus = model.PerformanceBonus,
                        IsPerformanceBonusEnabled = model.IsPerformanceBonusEnabled,
                        YearlyEndBonusTypeID = model.YearlyEndBonusTypeID,
                        IsYearlyEndBonusTypeIDEnabled = model.IsYearlyEndBonusTypeIDEnabled,
                        FastivalBonusPercentage = model.FastivalBonusPercentage,
                        IsFastivalBonusPercentageEnabled = model.IsFastivalBonusPercentageEnabled,
                        ProvidantFundEmployeePercentage = model.ProvidantFundEmployeePercentage,
                        ProvidantFundOrganizationPercentage = model.ProvidantFundOrganizationPercentage,
                        IsProvidantFundEnabled = model.IsProvidantFundEnabled,
                        ServiceYearID = model.ServiceYearID
                    };


                    var empSalary = await _employeeSalaryRepository.AllActive().FirstOrDefaultAsync(e => e.EmployeeID == model.EmployeePersonalId);

                    if (empSalary == null)
                    {
                        empSalary = new EmployeeSalarySettings()
                        {
                            EmployeeID = model.EmployeePersonalId,
                            IsBenefitsEnabled = model.IsBenifitEnabled,
                            //CreatedBy = model.CreatedBy,
                            //CreatedAt = DateTime.UtcNow,
                            //LIP = model.LIP,
                            //LMAC = model.LMAC
                        };
                        await _employeeSalaryRepository.AddAsync(empSalary, model);
                    }
                    else
                    {
                        empSalary.IsBenefitsEnabled = model.IsBenifitEnabled;
                        await _employeeSalaryRepository.UpdateAsync(empSalary, model);
                    }

                    EntityHelper.Create(newBenefit, model);

                    await _employeeBenifitRepository.AddAsync(newBenefit);
                }
                else
                {
                  
                    existingBenefit.HealthInsurance = model.HealthInsurance;
                    existingBenefit.IsHealthInsuranceEnabled = model.IsHealthInsuranceEnabled;
                    existingBenefit.PerformanceBonus = model.PerformanceBonus;
                    existingBenefit.IsPerformanceBonusEnabled = model.IsPerformanceBonusEnabled;
                    existingBenefit.YearlyEndBonusTypeID = model.YearlyEndBonusTypeID;
                    existingBenefit.IsYearlyEndBonusTypeIDEnabled = model.IsYearlyEndBonusTypeIDEnabled;
                    existingBenefit.FastivalBonusPercentage = model.FastivalBonusPercentage;
                    existingBenefit.IsFastivalBonusPercentageEnabled = model.IsFastivalBonusPercentageEnabled;
                    existingBenefit.ProvidantFundEmployeePercentage = model.ProvidantFundEmployeePercentage;
                    existingBenefit.ProvidantFundOrganizationPercentage = model.ProvidantFundOrganizationPercentage;
                    existingBenefit.IsProvidantFundEnabled = model.IsProvidantFundEnabled;
                    existingBenefit.ServiceYearID = model.ServiceYearID;

                    var empSalary = await _employeeSalaryRepository.AllActive().FirstOrDefaultAsync(e => e.EmployeeID == model.EmployeePersonalId);

                    if (empSalary == null)
                    {
                        empSalary = new EmployeeSalarySettings()
                        {
                            EmployeeID = model.EmployeePersonalId,
                            IsBenefitsEnabled = model.IsBenifitEnabled,
                           
                        };
                        await _employeeSalaryRepository.AddAsync(empSalary, model);
                    }
                    else
                    {
                        empSalary.IsBenefitsEnabled = model.IsBenifitEnabled;
                        await _employeeSalaryRepository.UpdateAsync(empSalary , model);
                    }

                    EntityHelper.Update(existingBenefit, model);

                    await _employeeBenifitRepository.UpdateAsync(existingBenefit);
                }

                return new CommonReturnViewModel()
                {
                    Success = true,
                    Message = "Employee benefits saved successfully.",
                    Data = model.EmployeePersonalId
                };
            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel()
                {
                    Success = false,
                    Message = ex.Message
                };
            }
            
            
        }

        

        public async Task<List<CommonSelectVMM>> SelectAsync(int id)
        {
            List<CommonSelectVMM> result = new List<CommonSelectVMM>();
            try
            {
                
                // Get all benefit types for the organization
                var benefitTypes = await benefitTypesRepository
                    .AllActive()
                    .Where(x => x.OrganizationID == id && x.IsApplyOnGrossSalary == true)
                    .ToListAsync();

                // Get all benefits with their setups
                var benefits = await this.benefits
                    .AllActive()
                    .Include(b => b.BenefitSetups)
                    .Include(b => b.BenefitType)
                    .Where(b => b.OrganizationID == id)
                    .ToListAsync();

                // Structure: BenefitType -> Benefits -> BenefitSetups
                 result = benefitTypes.Select(bt => new CommonSelectVMM
                {
                    Id = bt.BenefitTypeID,
                    Name = bt.BenefitTypeName,
                    EmpBenefitVMM = benefits
                        .Where(b => b.BenefitTypeID == bt.BenefitTypeID)
                        .Select(b => new EmpBenefitVMM
                        {
                            BenefitID = b.BenefitID,
                            OrganizationID = b.OrganizationID,
                            BenefitTypeID = b.BenefitTypeID,
                            BenefitTypeName = b.BenefitType != null ? b.BenefitType.BenefitTypeName : "",
                            IsActive = b.IsActive,
                            EffectiveDate = b.EffectiveDate,
                            BenefitSetups = b.BenefitSetups.Select(s => new EmpBenefitSetupVMM
                            {
                                BenefitSetupID = s.BenefitSetupID,
                                SalaryMin = s.SalaryMin,
                                SalaryMax = s.SalaryMax,
                                CalculationTypeID = s.CalculationTypeID,
                                Value = s.Value
                            }).ToList()
                        }).ToList()
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                // Optional: log the exception using your logging service
                 await userInfoService.ActionLogExceptionAsync("Employee Benefit", ex, id, new BaseViewModel(),ActionName.Error);

                // Return empty list or rethrow depending on your choice
                Console.WriteLine(ex); // for debugging
                return new List<CommonSelectVMM>();
            }
        }


    }
}
