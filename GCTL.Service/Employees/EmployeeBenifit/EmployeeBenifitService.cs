using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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
using SkiaSharp;

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

                     
                    };


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
                        await _employeeSalaryRepository.UpdateAsync(empSalary, model);
                    }

                    EntityHelper.Create(newBenefit, model);

                    await _employeeBenifitRepository.AddAsync(newBenefit);
                }
                else
                {
                  
                

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



        public async Task<CommonReturnViewModel> SaveOrUpdateEmployeeBenefitsAsync1(EmployeeBenifitPostViewModel22 model)
        {
            try
            {
                foreach (var benefit in model.Benefits)
                {
                    var existing = await _employeeBenifitRepository.AllActive()
                        .FirstOrDefaultAsync(b => b.BenefitID == benefit.BenefitID && b.EmployeeID == model.EmployeeId);

                    if (existing != null)
                    {
                        // Update
                        existing.CalculationTypeID = benefit.CalculationTypeID;
                        existing.BenefitValue = benefit.Value;
                        existing.LIP = model.LIP;
                        existing.LMAC = model.LMAC;
                        existing.UpdatedAt = DateTime.UtcNow;
                        existing.UpdatedBy = model.UpdatedBy;
                        await _employeeBenifitRepository.UpdateAsync(existing);
                    }
                    else
                    {
                        // Insert new
                        var newBenefit = new EmployeeBaseBenefits
                        {
                            EmployeeID = model.EmployeeId,
                            BenefitID = benefit.BenefitID,
                            CalculationTypeID = benefit.CalculationTypeID,
                            BenefitValue = benefit.Value,
                            IsBenifitEnabled = true,
                            LIP= model.LIP,
                            LMAC=model.LMAC,
                            CreatedAt=DateTime.UtcNow,
                            CreatedBy=model.CreatedBy,
                           
                          
                        };
                        await _employeeBenifitRepository.AddAsync(newBenefit);
                    }
                }
            }
            catch (Exception ex)
            {
                await userInfoService.ActionLogExceptionAsync("Employee Benefit", ex, 0, ActionName.Error);
                throw;
            }
            

          
            return new CommonReturnViewModel
            {
                Success = true,
                Message = "Benefits saved successfully."
            };
        }


        public async Task<List<CommonSelectVMMM>> SelectAsync(int id)
        {
            List<CommonSelectVMMM> result = new List<CommonSelectVMMM>();
            try
            {
                var orgaid = await _employeeOfficialRepository.AllActive()
                    .Where(x => x.EmployeeID == id)
                    .Select(x => x.OrganizationID)
                    .FirstOrDefaultAsync();

                var employeeSalary = await _employeeSalaryRepository.AllActive()
                    .Where(x => x.EmployeeID == id)
                    .Select(x => x.Salary)
                    .FirstOrDefaultAsync();

                var benefitTypes = await benefitTypesRepository
                    .AllActive()
                    .Where(x => x.OrganizationID == orgaid && x.IsApplyOnGrossSalary == true)
                    .ToListAsync();

                var benefits = await this.benefits
                    .AllActive()
                    .Include(b => b.BenefitSetups)
                    .Include(b => b.BenefitType)
                    .Where(b => b.OrganizationID == orgaid)
                    .ToListAsync();

                result = benefitTypes.Select(bt => new CommonSelectVMMM
                {
                    Id = bt.BenefitTypeID,
                    Name = bt.BenefitTypeName,
                    EmpBenefitVMM = benefits
                        .Where(b => b.BenefitTypeID == bt.BenefitTypeID)
                        .Select(b =>
                        {
                            var matchedSetup = b.BenefitSetups
                                .FirstOrDefault(s => s.SalaryMin <= employeeSalary && s.SalaryMax >= employeeSalary);

                            return new EmpBenefitVMMM
                            {
                                BenefitID = b.BenefitID,
                                OrganizationID = b.OrganizationID,
                                CalculationTypeID = matchedSetup?.CalculationTypeID ?? 0,
                                Value = matchedSetup?.Value ?? 0
                            };

                        }).ToList()
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                await userInfoService.ActionLogExceptionAsync("Employee Benefit", ex, id, ActionName.Error);
                Console.WriteLine(ex);
                return new List<CommonSelectVMMM>();
            }
        }

    }
}
