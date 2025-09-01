using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmployeeBenefitsVM;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpAllowance;
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
        public PayRollEmpAllowanceService(IGenericRepository<EmployeeAllowances> empAllowance, IUserInfoService userInfoService, IGenericRepository<CalculationTypes> calculationTypesRepository, IGenericRepository<EmployeeAllowanceSetup> empAlowanceSetup, IGenericRepository<EmployeeAllowanceTypes> empalowanceTypesRepository) : base(empAllowance)
        {
            this.empAllowance = empAllowance;
            this.userInfoService = userInfoService;
            this.calculationTypesRepository = calculationTypesRepository;
            this.empAlowanceSetup = empAlowanceSetup;
            this.empalowanceTypesRepository = empalowanceTypesRepository;
        }

        public async Task<CommonReturnViewModel> GetPayRollEmpAllowanceByIdAsync()
        {
            var result = new CommonReturnViewModel();
            try
            {
                var allowanceEntities = await empAllowance.AllActive().Include(x => x.EmployeeAllowanceSetup).Include(x=>x.EmployeeAllowanceType).ToListAsync();

                if (!allowanceEntities.Any())
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "No Employee Allowance records found!"
                    };
                }

                var allowanceDataList = allowanceEntities.Select(entity => new PayRollEmpAllowanceGetAll
                {
                    EmployeeAllowanceID = entity.EmployeeAllowanceID,
                    OrganizationID = entity.OrganizationID,
                    EmployeeAllowanceTypeID = entity.EmployeeAllowanceTypeID,
                    EmployeeAllowanceTypeName=entity.EmployeeAllowanceType.EmployeeAllowanceTypeName,
                    IsActive = entity.IsActive,
                    EffectiveDate = entity.EmployeeAllowanceSetup.FirstOrDefault()?.EffectiveDate,
                    HouseRentAllowances = entity.EmployeeAllowanceSetup.Select(x => new HouseRentAllowanceDetailGetVM
                    {
                        SalaryMin = x.SalaryMin,
                        SalaryMax = x.SalaryMax,
                        Value = x.Value,
                        CalculationTypeID = x.CalculationTypeID,
                        CalculationType = x.CalculationTypeID == 1 ? "Fixed" : "Percentage",
                        EffectiveDate = x.EffectiveDate
                    }).ToList()
                }).ToList();

                result.Success = true;
                result.Data = allowanceDataList;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "An error occurred while retrieving data.";
                result.Errors.Add(ex.Message);
            }

            return result;
        }


        #region Save Data 
        public async Task<CommonReturnViewModel> SavePayRollEmpAllowance(PayRollEmpAllowanceSaveVM entityVM)
        {
            var result = new CommonReturnViewModel();

            try
            {
                if (entityVM == null || entityVM?.OrganizationID is null or <= 0)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Employee Allowance record not found!"
                    };
                }
                var empallowanceType = await empAllowance.FirstOrDefaultAsync(x=>x.EmployeeAllowanceTypeID==entityVM.EmployeeAllowanceTypeID);
                if (empallowanceType !=null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Already Exists Allowance Type"
                    };
                }
                
                await empAllowance.BeginTransactionAsync();
                int EmployeeAllowanceID = 0;
                var entity = new EmployeeAllowances
                {
                    OrganizationID = entityVM.OrganizationID,
                    IsActive = entityVM.IsActive,
                    EmployeeAllowanceTypeID=entityVM.EmployeeAllowanceTypeID,
                    LIP = entityVM.LIP,
                    LMAC = entityVM.LMAC,
                    CreatedAt = DateTime.Now,
                    CreatedBy = entityVM.CreatedBy,
                };

                await empAllowance.AddAsync(entity);
                EmployeeAllowanceID=entity.EmployeeAllowanceID;
                var empAllowanceSetups = entityVM.HouseRentAllowances.Select(item => new EmployeeAllowanceSetup
                { 
                    CalculationTypeID =item.CalculationTypeID,
                    SalaryMax = item.SalaryMax,
                    SalaryMin = item.SalaryMin,
                    EffectiveDate = entityVM.EffectiveDate,
                    Value = item.Value,
                    EmployeeAllowanceID= EmployeeAllowanceID,
                    LIP= entityVM.LIP,
                    LMAC= entityVM.LMAC,
                    CreatedAt= DateTime.Now,
                    CreatedBy= entityVM.CreatedBy,
                    
                }).ToList();

                await empAlowanceSetup.AddRangeAsync(empAllowanceSetups);


                // You may need to commit the transaction here
                await empAllowance.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Data = entity,
                    Message="Saved Successfully"
                };
            }
            catch (Exception ex)
            {
                // Rollback in case of error
                await empAllowance.RollbackTransactionAsync();

                result.Success = false;
                result.Message = "An error occurred while saving.";
                result.Errors.Add(ex.Message);
            }

            return result;
        }



        #endregion
        #region Update By ID

        public async Task<CommonReturnViewModel> UpdatePayRollEmpAllowance(PayRollEmpAllowanceUpdate entityVM)
        {
            var result = new CommonReturnViewModel();

            if (entityVM == null || entityVM.OrganizationID <= 0 || entityVM.EmployeeAllowanceID <= 0)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Invalid Employee Allowance data!"
                };
            }

            try
            {
                await empAllowance.BeginTransactionAsync();

                // 1. Get existing EmployeeAllowance
                var entity = await empAllowance.GetByIdAsync(entityVM.EmployeeAllowanceID);
                if (entity == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Employee Allowance record not found!"
                    };
                }

                // Optional: Check for duplicate EmployeeAllowanceTypeID (excluding current)
                var exists = await empAllowance.FirstOrDefaultAsync(x =>
                    x.EmployeeAllowanceTypeID == entityVM.EmployeeAllowanceTypeID &&
                    x.EmployeeAllowanceID != entityVM.EmployeeAllowanceID);

                if (exists != null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Already exists allowance type!"
                    };
                }

                // 2. Update main EmployeeAllowance entity
                entity.OrganizationID = entityVM.OrganizationID;
                entity.IsActive = entityVM.IsActive;
                entity.EmployeeAllowanceTypeID = entityVM.EmployeeAllowanceTypeID;
                entity.LIP = entityVM.LIP;
                entity.LMAC = entityVM.LMAC;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = entityVM.UpdatedBy;

                await empAllowance.UpdateAsync(entity);

                // 3. Update HouseRentAllowances
                // First remove existing setups
                // Get all active setups for this EmployeeAllowanceID
                var hasExistingSetups = await empAlowanceSetup.AllActive()
                               .AnyAsync(x => x.EmployeeAllowanceID == entityVM.EmployeeAllowanceID);

                if (hasExistingSetups)
                {
                    await empAlowanceSetup.RemoveRangeAsync(x => x.EmployeeAllowanceID == entityVM.EmployeeAllowanceID);
                }

                // Add new setups
                var newSetups = entityVM.HouseRentAllowances.Select(item => new EmployeeAllowanceSetup
                {
                    EmployeeAllowanceID = entity.EmployeeAllowanceID,
                    CalculationTypeID = item.CalculationTypeID,
                    SalaryMin = item.SalaryMin,
                    SalaryMax = item.SalaryMax,
                    Value = item.Value,
                    EffectiveDate = entityVM.EffectiveDate,
                    LIP = entityVM.LIP,
                    LMAC = entityVM.LMAC,
                    CreatedAt = DateTime.Now,
                    CreatedBy = entityVM.UpdatedBy
                }).ToList();

                await empAlowanceSetup.AddRangeAsync(newSetups);

                // 4. Commit transaction
                await empAllowance.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Data = entity,
                    Message = "Updated Successfully"
                };
            }
            catch (Exception ex)
            {
                await empAllowance.RollbackTransactionAsync();
                result.Success = false;
                result.Message = "An error occurred while updating.";
                result.Errors.Add(ex.Message);
            }

            return result;
        }


        

        #endregion

        #region Get By Id 
        public async Task<CommonReturnViewModel> GetByIdPayRollEmpAllowance(int employeeAllowanceID)
        {
            try
            {
                var entity = await empAllowance.GetByIdAsync(employeeAllowanceID);
                var result = new PayRollEmpAllowanceGetById
                {
                    EmployeeAllowanceID = entity.EmployeeAllowanceID,
                    OrganizationIDEdit = entity.OrganizationID,
                   
                };
                return new CommonReturnViewModel
                {
                    Success = true,
                    Data = result,
                };

            }
            catch (Exception)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Employee Benefits Does not find"
                };
            }
        }

        #endregion
        #region Soft Delete 
        public async Task<CommonReturnViewModel> SoftDeletePayRollEmpAllowance(DeleteRequestVM deleteRequestVM)
        {
            await empAllowance.BeginTransactionAsync();
            try
            {
                var data = await empAllowance.FindAsync(x => deleteRequestVM.Ids.Contains(x.EmployeeAllowanceID));
                if (data == null || data.Count == 0)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "No data found to delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<PayRollEmpAllowanceSaveVM>>(
             JsonConvert.SerializeObject(data, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
                var targetIds = data.Select(x => (int?)x.EmployeeAllowanceID).ToList();
                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.LIP = deleteRequestVM.LIP;
                    item.LMAC = deleteRequestVM.LMAC;
                    item.DeletedBy = deleteRequestVM.DeletedBy ?? null;
                }

                await empAllowance.UpdateRangeAsync(data);
                await userInfoService.ActionLogDeleteAsync("PayRoll Employee Allowance", ActionName.DataDeleted, null, beforeEntity, targetIds, deleteRequestVM);
                await empAllowance.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = $"Deleted Successfully."

                };
            }
            catch (Exception ex)
            {
                await empAllowance.RollbackTransactionAsync();
                throw new Exception("Error occurred during the deletion of data.", ex);
            }
        }




        #endregion

        #region Get Employee TYpe Name

        public async Task<List<AllowanceTypeNameVM>> GetEmpAllowanceType()
        {
            var data = await empalowanceTypesRepository.AllActive()
                .Select(x => new AllowanceTypeNameVM
                {
                    EmployeeAllowanceTypeID = x.EmployeeAllowanceTypeID,
                    EmployeeAllowanceTypeName = x.EmployeeAllowanceTypeName,
                    
                })
                .ToListAsync();

            return data;
        }



        #endregion

        #region Get Allowance Type 

        public async Task<List<CommonSelectVM>> SelectAsync(int id)
        {
            try
            {
                var data = await empalowanceTypesRepository
                    .AllActive()
                    .Where(x => x.OrganizationID == id).ToListAsync(); 

                if (data == null || !data.Any())
                {
                    return new List<CommonSelectVM>(); 
                }

                var result = data.Select(x => new CommonSelectVM
                {
                    Id = x.EmployeeAllowanceTypeID, // Assuming this is the unique ID
                    Name = x.EmployeeAllowanceTypeName
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        
        #endregion
    }
}
