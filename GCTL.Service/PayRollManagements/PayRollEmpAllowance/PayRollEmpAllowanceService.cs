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
        private readonly IGenericRepository<EmployeeAllowanceTypes> empalowanceTypesRepository;
        public PayRollEmpAllowanceService(IGenericRepository<EmployeeAllowances> empAllowance, IUserInfoService userInfoService, IGenericRepository<CalculationTypes> calculationTypesRepository, IGenericRepository<EmployeeAllowanceSetup> empAlowanceSetup, IGenericRepository<EmployeeAllowanceTypes> empalowanceTypesRepository) : base(empAllowance)
        {
            this.empAllowance = empAllowance;
            this.userInfoService = userInfoService;
            this.calculationTypesRepository = calculationTypesRepository;
            this.empAlowanceSetup = empAlowanceSetup;
            this.empalowanceTypesRepository = empalowanceTypesRepository;
        }


        #region Get All Dataum

        public async Task<PaginationService<EmployeeAllowances, PayRollEmpAllowanceGetAll>.PaginationResult<PayRollEmpAllowanceGetAll>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", int? organizationId = null)
        {
#nullable disable

            try
            {
                // 🔹 Step 3: Base query with includes
                var query = empAllowance.AllActive().Include(x=>x.Organization).OrderByDescending(x => x.EmployeeAllowanceID).AsQueryable();
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
                     
                      (b.Organization != null && EF.Functions.Like(b.Organization.OrganizationName, $"%{term}%")) ,
                     

                    b => new PayRollEmpAllowanceGetAll
                    {
                        EmployeeAllowanceID = b.EmployeeAllowanceID,
                        OrganizationName = b.Organization?.OrganizationName ?? string.Empty,
                        
                       
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
                entity.OrganizationID = entityVM.OrganizationIDEdit;
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

        #region Get By Data
        public Task<List<PayRollEmpAllowanceGetAll>> GetDataByID(int employeeAllowanceID)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
