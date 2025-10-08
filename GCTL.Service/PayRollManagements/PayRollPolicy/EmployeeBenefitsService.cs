using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveSettings;
using GCTL.Core.ViewModels.MasterSetup.Statuses;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmployeeUpdateVM;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.AttendanceManagement.LeaveManagements;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollPolicy
{
    public class EmployeeBenefitsService : AppService<Benefits>, IEmployeeBenefitsService
    {
        private readonly IGenericRepository<EmployeeBenefits> empBenefits;
        private readonly IUserInfoService userInfoService;
        private readonly IGenericRepository<Benefits>  benefits;
        private readonly IGenericRepository<BenefitTypes> benefitTypesRepository;
        private readonly IGenericRepository<BenefitSetups> benefitSetupRepository;
        private readonly IGenericRepository<CalculationTypes> calculationTypesRepository;
        public EmployeeBenefitsService(IGenericRepository<EmployeeBenefits> empBenefits, IUserInfoService userInfoService, IGenericRepository<Benefits> benefits, IGenericRepository<BenefitTypes> benefitTypesRepository, IGenericRepository<BenefitSetups> benefitSetupRepository, IGenericRepository<CalculationTypes> calculationTypesRepository) : base(benefits)
        {
            this.empBenefits = empBenefits;
            this.userInfoService = userInfoService;
            this.benefits = benefits;
            this.benefitTypesRepository = benefitTypesRepository;
            this.benefitSetupRepository = benefitSetupRepository;
            this.calculationTypesRepository = calculationTypesRepository;
        }


        #region Get Benefits Type 



        public async Task<CommonReturnViewModel> SaveEmployeeBenefitsAsync(EmployeeBenefitsVM entityVM)
        {
            var result = new CommonReturnViewModel();

            if (entityVM == null || entityVM.OrganizationID <= 0)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Employee Benefit record not found!"
                };
            }

            bool hasInsert = false;
            bool hasUpdate = false;
            Benefits benefit;
            await benefits.BeginTransactionAsync();
            try
            {
                foreach (var benefitVM in entityVM.Benefits)
                {
                    

                    if (benefitVM.BenefitID > 0)
                    {
                        // 🔹 Update case
                        benefit = await benefits.FirstOrDefaultAsync(x => x.BenefitID == benefitVM.BenefitID);

                        if (benefit != null)
                        {
                            hasUpdate = true; // track update
                            benefit.BenefitTypeID = benefitVM.BenefitTypeID;
                            benefit.IsActive = benefitVM.IsActive;
                            benefit.EffectiveDate = benefitVM.EffectiveDate;
                            benefit.LIP = entityVM.LIP;
                            benefit.LMAC = entityVM.LMAC;
                            benefit.UpdatedAt = DateTime.UtcNow;
                            benefit.UpdatedBy = entityVM.UpdatedBy;

                            await benefits.UpdateAsync(benefit);
                        }
                        else
                        {
                            // treat as new insert
                            benefit = new Benefits
                            {
                                OrganizationID = entityVM.OrganizationID,
                                BenefitTypeID = benefitVM.BenefitTypeID,
                                IsActive = benefitVM.IsActive,
                                EffectiveDate = benefitVM.EffectiveDate,
                                LIP = entityVM.LIP,
                                LMAC = entityVM.LMAC,
                                CreatedAt = DateTime.Now,
                                CreatedBy = entityVM.CreatedBy
                            };
                            await benefits.AddAsync(benefit);
                            hasInsert = true; // track insert
                        }

                        // handle setups
                        var existingSetups = await benefitSetupRepository.AllActive()
                            .Where(x => x.BenefitID == benefit.BenefitID)
                            .ToListAsync();

                        var setupIdsInVM = benefitVM.BenefitSetups.Select(x => x.BenefitSetupID).ToList();

                        // delete removed setups
                        var setupsToDelete = existingSetups.Where(x => !setupIdsInVM.Contains(x.BenefitSetupID)).ToList();
                        if (setupsToDelete.Any())
                        {
                            await benefitSetupRepository.DeleteRangeAsync(setupsToDelete);
                        }

                        // insert/update setups
                        foreach (var setupVM in benefitVM.BenefitSetups)
                        {
                            var setup = existingSetups.FirstOrDefault(x => x.BenefitSetupID == setupVM.BenefitSetupID);

                            if (setup == null)
                            {
                                var newSetup = new BenefitSetups
                                {
                                    BenefitID = benefit.BenefitID,
                                    CalculationTypeID = setupVM.CalculationTypeID,
                                    SalaryMax = setupVM.SalaryMax,
                                    SalaryMin = setupVM.SalaryMin,
                                    Value = setupVM.Value,
                                    LIP = entityVM.LIP,
                                    LMAC = entityVM.LMAC,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = entityVM.CreatedBy
                                };
                                await benefitSetupRepository.AddAsync(newSetup);
                                hasInsert = true;
                            }
                            else
                            {
                                setup.CalculationTypeID = setupVM.CalculationTypeID;
                                setup.SalaryMax = setupVM.SalaryMax;
                                setup.SalaryMin = setupVM.SalaryMin;
                                setup.Value = setupVM.Value;
                                setup.LIP = entityVM.LIP;
                                setup.LMAC = entityVM.LMAC;
                                setup.UpdatedAt = DateTime.Now;
                                setup.UpdatedBy = entityVM.CreatedBy;

                                await benefitSetupRepository.UpdateAsync(setup);
                                hasUpdate = true;
                            }
                        }
                    }
                    else
                    {
                        // 🔹 Insert case
                        benefit = new Benefits
                        {
                            OrganizationID = entityVM.OrganizationID,
                            BenefitTypeID = benefitVM.BenefitTypeID,
                            IsActive = benefitVM.IsActive,
                            EffectiveDate = benefitVM.EffectiveDate,
                            LIP = entityVM.LIP,
                            LMAC = entityVM.LMAC,
                            CreatedAt = DateTime.Now,
                            CreatedBy = entityVM.CreatedBy
                        };
                        await benefits.AddAsync(benefit);
                        hasInsert = true;

                        var setups = benefitVM.BenefitSetups.Select(setupVM => new BenefitSetups
                        {
                            BenefitID = benefit.BenefitID,
                            CalculationTypeID = setupVM.CalculationTypeID,
                            SalaryMax = setupVM.SalaryMax,
                            SalaryMin = setupVM.SalaryMin,
                            Value = setupVM.Value,
                            LIP = entityVM.LIP,
                            LMAC = entityVM.LMAC,
                            CreatedAt = DateTime.Now,
                            CreatedBy = entityVM.CreatedBy
                        }).ToList();

                        await benefitSetupRepository.AddRangeAsync(setups);
                    }
                }

                await benefits.CommitTransactionAsync();

                result.Success = true;
                if (hasInsert && hasUpdate)
                    result.Message = "Saved and Updated Successfully";
                else if (hasInsert)
                    result.Message = "Saved Successfully";
                else if (hasUpdate)
                    result.Message = "Updated Successfully";
                else
                    result.Message = "No changes made!";
            }
            catch (Exception ex)
            {
                await benefits.RollbackTransactionAsync();
                result.Success = false;
                result.Message = "An error occurred while saving.";
                result.Errors.Add(ex.Message);
                await userInfoService.ActionLogExceptionAsync("Organization Benefit", ex, entityVM.Benefits.FirstOrDefault()?.BenefitID, ActionName.Error);
            }

            return result;
        }

        #endregion
        public async Task<List<CommonSelectVMM>> SelectAsync(int id)
        {

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
                var result = benefitTypes.Select(bt => new CommonSelectVMM
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
                await userInfoService.ActionLogExceptionAsync("Organization Benefit", ex, id, ActionName.Error);
                throw;
            }
           
        }
       

    }

}

