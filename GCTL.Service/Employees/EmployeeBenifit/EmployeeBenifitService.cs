using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeBenifit;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.Employees.EmployeeBenifit
{
    public class EmployeeBenifitService : IEmployeeBenifitService
    {
        private readonly IGenericRepository<EmployeeBaseBenefits> _employeeBenifitRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;


        public EmployeeBenifitService(IGenericRepository<EmployeeBaseBenefits> employeeBenifitRepository, IGenericRepository<Data.Models.Employees> employeeRepository)
        {
            _employeeBenifitRepository = employeeBenifitRepository;
            _employeeRepository = employeeRepository;
        }

        public Task<EmployeeBenifitGetViewModel> GetEmployeeBenifitByEmployeeIdAsync(int employeeId)
        {
            throw new NotImplementedException();
        }

        public async Task<EmployeeBenifitPostViewModel> GetEmployeeBenefitsAsync(string employeeId)
        {
            if (!int.TryParse(employeeId, out var id))
                return null;

            var benefits = await (from eb in _employeeBenifitRepository.AllActive()
                                  join emp in _employeeRepository.AllActive()
                                  on eb.EmployeeID equals emp.EmployeeID
                                  where eb.EmployeeID == id
                                  select new EmployeeBenifitPostViewModel
                                  {
                                      EmployeeBaseBenefitID = eb.EmployeeBaseBenefitID,
                                      EmployeePersonalId = (int)eb.EmployeeID,
                                     
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
                                      ServiceYearID = eb.ServiceYearID ?? 0
                                  }).FirstOrDefaultAsync();


            if (benefits != null)
            {
                return benefits;
            }
            else
            {
                var emp = await _employeeRepository.AllActive().Where(e=>e.EmployeeID == id).Select(m=> new EmployeeBenifitPostViewModel
                {
                    EmployeePersonalId = (int)m.EmployeeID,

                    PersonalEmail = m.Email ?? "N/A",
                    PersonalPhone = m.MobileNumber ?? "N/A",
                }).FirstOrDefaultAsync();

                return emp;
            }

            
        }

        public async Task<bool> SaveOrUpdateEmployeeBenefitsAsync(EmployeeBenifitPostViewModel model)
        {
            try
            {
                var existingBenefit = await _employeeBenifitRepository.AllActive()
                .FirstOrDefaultAsync(e => e.EmployeeBaseBenefitID == model.EmployeeBaseBenefitID);

                if (existingBenefit == null)
                {

                    var newBenefit = new EmployeeBaseBenefits
                    {
                        EmployeeBaseBenefitID = model.EmployeeBaseBenefitID,
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

                    await _employeeBenifitRepository.UpdateAsync(existingBenefit);
                }

                return true;
            }
            catch (Exception)
            {

                throw;
            }
            
            
        }
    }
}
