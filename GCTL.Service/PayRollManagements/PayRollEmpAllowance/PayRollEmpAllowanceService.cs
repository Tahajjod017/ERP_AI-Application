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
        public PayRollEmpAllowanceService(IGenericRepository<EmployeeAllowances> empAllowance, IUserInfoService userInfoService, IGenericRepository<CalculationTypes> calculationTypesRepository , IGenericRepository<EmployeeAllowanceSetup> empAlowanceSetup ) : base(empAllowance)
        {
            this.empAllowance = empAllowance;
            this.userInfoService = userInfoService;
            this.calculationTypesRepository = calculationTypesRepository;
            this.empAlowanceSetup = empAlowanceSetup;
        }


        #region Get All Dataum

        public async Task<PaginationService<EmployeeAllowances, PayRollEmpAllowanceGetAll>.PaginationResult<PayRollEmpAllowanceGetAll>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", int? organizationId = null)
        {
#nullable disable

            try
            {
                // 🔹 Step 3: Base query with includes
                var query = empAllowance.AllActive().Include(x=>x.Organization).Include(h=>h.HRentDependsOnSalaryType).Include(x=>x.MediAllowDepOnSalaryType).Include(c=>c.ConAllowDepOnSalaryType).OrderByDescending(x => x.EmployeeAllowanceID).AsQueryable();
                if (query == null)
                {
                    throw new InvalidOperationException("query source is null.");
                }

                if (organizationId.HasValue)
                {

                    query = query.Where(x => x.OrganizationID == organizationId);
                }
                Expression<Func<EmployeeAllowances, object>> orderByExpression = currentSortColumn?.ToLower() switch
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
                    _ => x => x.EmployeeAllowanceID
                };

                query = currentSortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(orderByExpression)
                    : query.OrderBy(orderByExpression);

                // For approver Step


                //
                var result = await PaginationService<EmployeeAllowances, PayRollEmpAllowanceGetAll>.GetPaginatedData(
                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    currentSortColumn,
                    currentSortOrder,

                   term => b =>
                      string.IsNullOrEmpty(term) ||
                      EF.Functions.Like(b.EmployeeAllowanceID.ToString(), $"%{term}%") ||
                      EF.Functions.Like((b.MobileInternetAllowance ?? 0).ToString(), $"%{term}%") ||
                      (b.Organization != null && EF.Functions.Like(b.Organization.OrganizationName, $"%{term}%")) ||
                      (b.HRentDependsOnSalaryType != null && EF.Functions.Like(b.MediAllowDepOnSalaryType.SalaryTypeName, $"%{term}%")) ||
                      (b.ConAllowDepOnSalaryType != null && EF.Functions.Like(b.ConAllowDepOnSalaryType.SalaryTypeName, $"%{term}%")) ,

                    b => new PayRollEmpAllowanceGetAll
                    {
                        EmployeeAllowanceID = b.EmployeeAllowanceID,
                        OrganizationName = b.Organization?.OrganizationName ?? string.Empty,
                        MobileInternetAllowance = b.MobileInternetAllowance ?? 0,
                        ShiftAllowance = b.ShiftAllowance ?? 0,
                        HouseRentAllowanceRate = b.HouseRentAllowanceRate ?? 0,
                        ConveyanceAllowanceRate = b.ConveyanceAllowanceRate ?? 0,
                        HRentDependsOnSalaryTypeIDName = b.HRentDependsOnSalaryType?.SalaryTypeName ?? string.Empty,
                        MediAllowDepOnSalaryTypeIDName = b.MediAllowDepOnSalaryType?.SalaryTypeName ?? string.Empty,
                        MedicalAllowanceRate= b.MedicalAllowanceRate ?? 0,
                        ConAllowDepOnSalaryTypeIDName = b.ConAllowDepOnSalaryType?.SalaryTypeName ?? string.Empty,
                       
                    });

                return result;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");

                return new PaginationService<EmployeeAllowances, PayRollEmpAllowanceGetAll>.PaginationResult<PayRollEmpAllowanceGetAll>
                {
                    Data = new List<PayRollEmpAllowanceGetAll>(),
                    TotalCount = 0
                };
            }
        }

        #endregion

        #region Save Data 
        public async Task<CommonReturnViewModel> SavePayRollEmpAllowance(PayRollEmpAllowanceSaveVM entityVM)
        {
            var result = new CommonReturnViewModel();

            try
            {
                if (entityVM == null || entityVM.OrganizationID == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Employee Allowance record not found!"
                    };
                }

                await empAllowance.BeginTransactionAsync();

                var entity = new EmployeeAllowances
                {
                    OrganizationID = entityVM.OrganizationID,
                    IsConveyanceAllowanceEnabled = entityVM.IsConveyanceAllowanceEnabled,
                    IsMobileInternetAllowanceEnabled = entityVM.IsMobileInternetAllowanceEnabled,
                    IsMedicalAllowanceEnabled = entityVM.IsMedicalAllowanceEnabled,
                    MobileInternetAllowance = entityVM.MobileInternetAllowance,
                    IsHouseRentAllowanceEnabled = entityVM.IsHouseRentAllowanceEnabled,
                    HouseRentAllowanceRate = entityVM.HouseRentAllowanceRate,
                    IsShiftAllowanceEnabled = entityVM.IsShiftAllowanceEnabled,
                    ShiftAllowance = entityVM.ShiftAllowance,
                    HRentDependsOnSalaryTypeID = entityVM.HRentDependsOnSalaryTypeID,
                    MediAllowDepOnSalaryTypeID = entityVM.MediAllowDepOnSalaryTypeID,
                    MedicalAllowanceRate = entityVM.MedicalAllowanceRate,
                    ConAllowDepOnSalaryTypeID = entityVM.ConAllowDepOnSalaryTypeID,
                    ConveyanceAllowanceRate = entityVM.ConveyanceAllowanceRate,
                    LIP = entityVM.LIP,
                    LMAC = entityVM.LMAC,
                    CreatedAt = DateTime.Now,
                    CreatedBy = entityVM.CreatedBy,
                };

                await empAllowance.AddAsync(entity);
                var empAllowanceSetups = entityVM.HouseRentAllowances.Select(item => new EmployeeAllowanceSetup
                {
                    OrganizationID = entityVM.OrganizationID,
                    SalaryMax = item.SalaryMax,
                    SalaryMin = item.SalaryMin,
                    EffectiveDate = item.EffectiveDate
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
        public async Task<CommonReturnViewModel> UpdatePayRollEmpAllowance([FromBody]PayRollEmpAllowanceUpdate entityVM)
        {
            await empAllowance.BeginTransactionAsync();
            try
            {
                // 1. Get existing entity
                var entity = await empAllowance.GetByIdAsync(entityVM.EmployeeAllowanceID);
                if (entity == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Employee Allowance record not found!"
                    };
                }
                // 2. Update fields
                entity.OrganizationID = entityVM.OrganizationIDEdit;
                entity.IsConveyanceAllowanceEnabled = entityVM.IsConveyanceAllowanceEnabledEdit;
                entity.IsMobileInternetAllowanceEnabled = entityVM.IsMobileInternetAllowanceEnabledEdit;
                entity.IsMedicalAllowanceEnabled = entityVM.IsMedicalAllowanceEnabledEdit;
                entity.MobileInternetAllowance = entityVM.MobileInternetAllowanceEdit;
                entity.IsHouseRentAllowanceEnabled = entityVM.IsHouseRentAllowanceEnabledEdit;
                entity.HouseRentAllowanceRate = entityVM.HouseRentAllowanceRateEdit;
                entity.IsShiftAllowanceEnabled = entityVM.IsShiftAllowanceEnabledEdit;
                entity.ShiftAllowance = entityVM.ShiftAllowanceEdit;
                entity.HRentDependsOnSalaryTypeID = entityVM.HRentDependsOnSalaryTypeIDEdit;
                entity.MediAllowDepOnSalaryTypeID = entityVM.MediAllowDepOnSalaryTypeIDEdit;
                entity.MedicalAllowanceRate = entityVM.MedicalAllowanceRateEdit;
                entity.ConAllowDepOnSalaryTypeID = entityVM.ConAllowDepOnSalaryTypeIDEdit;
                entity.ConveyanceAllowanceRate = entityVM.ConveyanceAllowanceRateEdit;
                entity.LIP = entityVM.LIP;
                entity.LMAC = entityVM.LMAC;
                entity.UpdatedAt = DateTime.Now;
                 entity.UpdatedBy = entityVM.UpdatedBy;
                // 3. Update DB
                await empAllowance.UpdateAsync(entity);
                await empAllowance.CommitTransactionAsync();

                // 4. Return success
                return new CommonReturnViewModel
                {
                    Success = true,
                    Data = entity.EmployeeAllowanceID,
                    Message="Updated Successfully"
                };
            }
            catch (Exception ex)
            {
                await empAllowance.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Error updating Employee Allowance: " + ex.Message
                };
            }
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
                    IsConveyanceAllowanceEnabledEdit = entity.IsConveyanceAllowanceEnabled,
                    IsMobileInternetAllowanceEnabledEdit = entity.IsMobileInternetAllowanceEnabled,
                    IsMedicalAllowanceEnabledEdit = entity.IsMedicalAllowanceEnabled,
                    MobileInternetAllowanceEdit = entity.MobileInternetAllowance,
                    IsHouseRentAllowanceEnabledEdit = entity.IsHouseRentAllowanceEnabled,
                    HouseRentAllowanceRateEdit = entity.HouseRentAllowanceRate,
                    IsShiftAllowanceEnabledEdit = entity.IsShiftAllowanceEnabled,
                    ShiftAllowanceEdit = entity.ShiftAllowance,
                    HRentDependsOnSalaryTypeIDEdit = entity.HRentDependsOnSalaryTypeID,
                    MediAllowDepOnSalaryTypeIDEdit = entity.MediAllowDepOnSalaryTypeID,
                    MedicalAllowanceRateEdit = entity.MedicalAllowanceRate,
                    ConAllowDepOnSalaryTypeIDEdit = entity.ConAllowDepOnSalaryTypeID,
                    ConveyanceAllowanceRateEdit = entity.ConveyanceAllowanceRate,
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


    }
}
