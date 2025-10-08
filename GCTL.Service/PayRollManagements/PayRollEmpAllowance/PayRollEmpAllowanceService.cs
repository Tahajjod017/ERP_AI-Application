using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmployeeUpdateVM;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpAllowance;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpSalary;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.MasterSetup.Gender;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollEmpAllowance
{
    public class PayRollEmpAllowanceService : AppService<EmployeeAllowances>, IPayRollEmpAllowanceService
    {
        private readonly IGenericRepository<EmployeeAllowances> empAllowance;
        private readonly IUserInfoService userInfoService;
        private readonly IGenericRepository<CalculationTypes> calculationTypesRepository;
        private readonly IGenericRepository<EmployeeAllowanceSetup> empAlowanceSetup;
        private readonly IGenericRepository<EmployeeAllowanceTypes> empalowanceTypesRepository;
        private readonly IGenericRepository<BenefitTypes> benefitTypesRepository;
        private readonly IGenericRepository<Benefits> benefitsRepository;
        private readonly IGenericRepository<BenefitSetups> benefitSetupsRepository;
        public PayRollEmpAllowanceService(IGenericRepository<EmployeeAllowances> empAllowance, IUserInfoService userInfoService, IGenericRepository<CalculationTypes> calculationTypesRepository, IGenericRepository<EmployeeAllowanceSetup> empAlowanceSetup, IGenericRepository<EmployeeAllowanceTypes> empalowanceTypesRepository, IGenericRepository<BenefitTypes> benefitTypesRepository , IGenericRepository<Benefits> benefitsRepository , IGenericRepository<BenefitSetups> benefitSetupsRepository ) : base(empAllowance)
        {
            this.empAllowance = empAllowance;
            this.userInfoService = userInfoService;
            this.calculationTypesRepository = calculationTypesRepository;
            this.empAlowanceSetup = empAlowanceSetup;
            this.empalowanceTypesRepository = empalowanceTypesRepository;
            this.benefitTypesRepository = benefitTypesRepository;
            this.benefitsRepository = benefitsRepository;
            this.benefitSetupsRepository = benefitSetupsRepository;
        }




        #region Save Data 
    
        public async Task<CommonReturnViewModel> SavePayRollEmpAllowance(PayRollEmpAllowanceSaveVM entityVM)
        {
            var result = new CommonReturnViewModel();

            if (entityVM == null || entityVM.OrganizationID <= 0)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Employee allowance record not found!"
                };
            }

            bool hasInsert = false;
            bool hasUpdate = false;
            EmployeeAllowances benefit;

            await empAllowance.BeginTransactionAsync();
            try
            {
                foreach (var benefitVM in entityVM.Allowances)
                {
                    if (benefitVM.EmployeeAllowanceID > 0)
                    {
                        // 🔹 Update existing allowance
                        benefit = await empAllowance.FirstOrDefaultAsync(x => x.EmployeeAllowanceID == benefitVM.EmployeeAllowanceID);

                        if (benefit != null)
                        {
                            hasUpdate = true;
                            benefit.EmployeeAllowanceTypeID = benefitVM.EmployeeAllowanceTypeID;
                            benefit.IsActive = benefitVM.IsActive;
                            benefit.EffectiveDate = benefitVM.EffectiveDate;
                            benefit.LIP = entityVM.LIP;
                            benefit.LMAC = entityVM.LMAC;
                            benefit.UpdatedAt = DateTime.UtcNow;
                            benefit.UpdatedBy = entityVM.UpdatedBy;

                            await empAllowance.UpdateAsync(benefit);
                        }
                        else
                        {
                            // Treat as new insert if not found
                            benefit = new EmployeeAllowances
                            {
                                OrganizationID = entityVM.OrganizationID,
                                EmployeeAllowanceTypeID = benefitVM.EmployeeAllowanceTypeID,
                                IsActive = benefitVM.IsActive,
                                EffectiveDate = benefitVM.EffectiveDate,
                                LIP = entityVM.LIP,
                                LMAC = entityVM.LMAC,
                                CreatedAt = DateTime.Now,
                                CreatedBy = entityVM.CreatedBy
                            };
                            await empAllowance.AddAsync(benefit);
                            hasInsert = true;
                        }

                        // 🔹 Handle setups
                        var existingSetups = await empAlowanceSetup.AllActive()
                            .Where(x => x.EmployeeAllowanceID == benefit.EmployeeAllowanceID)
                            .ToListAsync();

                        var setupIdsInVM = benefitVM.AllowanceSetups?.Select(x => x.EmployeeAllowanceSetupID).ToList() ?? new List<int>();

                        // Delete removed setups
                        var setupsToDelete = existingSetups.Where(x => !setupIdsInVM.Contains(x.EmployeeAllowanceSetupID)).ToList();
                        if (setupsToDelete.Any())
                            await empAlowanceSetup.DeleteRangeAsync(setupsToDelete);

                        // Insert or update setups
                        foreach (var setupVM in benefitVM.AllowanceSetups ?? new List<AllowanceSetupVM>())
                        {
                            var setup = existingSetups.FirstOrDefault(x => x.EmployeeAllowanceSetupID == setupVM.EmployeeAllowanceSetupID);

                            if (setup == null)
                            {
                                var newSetup = new EmployeeAllowanceSetup
                                {
                                    EmployeeAllowanceID = benefit.EmployeeAllowanceID,
                                    CalculationTypeID = setupVM.CalculationTypeID,
                                    SalaryMax = setupVM.SalaryMax,
                                    SalaryMin = setupVM.SalaryMin,
                                    Value = setupVM.Value,
                                    LIP = entityVM.LIP,
                                    LMAC = entityVM.LMAC,
                                    CreatedAt = DateTime.UtcNow,
                                    CreatedBy = entityVM.CreatedBy
                                };
                                await empAlowanceSetup.AddAsync(newSetup); // ✅ Correct repository
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
                                setup.UpdatedAt = DateTime.UtcNow;
                                setup.UpdatedBy = entityVM.CreatedBy;

                                await empAlowanceSetup.UpdateAsync(setup);
                                hasUpdate = true;
                            }
                        }
                    }
                    else
                    {
                        // 🔹 Insert new allowance
                        benefit = new EmployeeAllowances
                        {
                            OrganizationID = entityVM.OrganizationID,
                            EmployeeAllowanceTypeID = benefitVM.EmployeeAllowanceTypeID,
                            IsActive = benefitVM.IsActive,
                            EffectiveDate = benefitVM.EffectiveDate,
                            LIP = entityVM.LIP,
                            LMAC = entityVM.LMAC,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = entityVM.CreatedBy
                        };
                        await empAllowance.AddAsync(benefit);
                        hasInsert = true;

                        // Insert setups
                        foreach (var setupVM in benefitVM.AllowanceSetups ?? new List<AllowanceSetupVM>())
                        {
                            var newSetup = new EmployeeAllowanceSetup
                            {
                                EmployeeAllowanceID = benefit.EmployeeAllowanceID,
                                CalculationTypeID = setupVM.CalculationTypeID,
                                SalaryMax = setupVM.SalaryMax,
                                SalaryMin = setupVM.SalaryMin,
                                Value = setupVM.Value,
                                LIP = entityVM.LIP,
                                LMAC = entityVM.LMAC,
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = entityVM.CreatedBy
                            };
                            await empAlowanceSetup.AddAsync(newSetup); 
                        }
                    }
                }

                await empAllowance.CommitTransactionAsync();

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
                await empAllowance.RollbackTransactionAsync();
                result.Success = false;
                result.Message = "An error occurred while saving.";
                result.Errors.Add(ex.Message);
                await userInfoService.ActionLogExceptionAsync("Organization allowance", ex, entityVM.Allowances?.FirstOrDefault()?.EmployeeAllowanceID, ActionName.Error);
            }

            return result;
        }




        #endregion


        #region Get Allowance Type 

        public async Task<List<CommonSelectVMM>> SelectAsync(int id)
        {
            //var result = new List<CommonSelectVMM>();
            try
            {
                // Get all allowance types for the organization
                var allowanceTypes = await empalowanceTypesRepository
                    .AllActive()
                    .Where(x => x.OrganizationID == id && x.IsApplyOnGrossSalary == true)
                    .ToListAsync();

               
                var empAllowances3 = await empAllowance 
                    .AllActive()
                    .Include(a => a.EmployeeAllowanceSetup)
                    .Include(a => a.EmployeeAllowanceType)
                    .Where(a => a.OrganizationID == id)
                    .ToListAsync();

                // Structure: AllowanceType -> EmployeeAllowances -> EmployeeAllowanceSetups
                var result = allowanceTypes.Select(at => new CommonSelectVMM
                {
                    Id = at.EmployeeAllowanceTypeID,
                    Name = at.EmployeeAllowanceTypeName,
                    EmpAllowanceVMM = empAllowances3
                        .Where(ea => ea.EmployeeAllowanceTypeID == at.EmployeeAllowanceTypeID)
                        .Select(ea => new EmpAllowanceVMM
                        {
                            EmployeeAllowanceID = ea.EmployeeAllowanceID,
                            OrganizationID = ea.OrganizationID,
                            EmployeeAllowanceTypeID = ea.EmployeeAllowanceTypeID,
                            EmployeeAllowanceTypeName = ea.EmployeeAllowanceType?.EmployeeAllowanceTypeName ?? "",
                            IsActive = ea.IsActive,
                            EffectiveDate = ea.EffectiveDate, 
                            EmployeeAllowanceSetups = ea.EmployeeAllowanceSetup
                                .Select(s => new EmpAllowanceSetupVMM
                                {
                                    EmployeeAllowanceSetupID = s.EmployeeAllowanceSetupID,
                                    SalaryMin = s.SalaryMin,
                                    SalaryMax = s.SalaryMax,
                                    CalculationTypeID = s.CalculationTypeID,
                                    Value = s.Value,
                                }).ToList()
                        }).ToList()
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                // Log the exception
                await userInfoService.ActionLogExceptionAsync("Employee Allowance", ex, id, ActionName.Error);

                Console.WriteLine(ex);
                return new List<CommonSelectVMM>();
            }
        }

       
        #endregion
    }
}
