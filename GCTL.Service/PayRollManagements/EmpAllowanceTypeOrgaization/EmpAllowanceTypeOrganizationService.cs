using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.Grade;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmpAllowanceOrganization;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollSettings;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace GCTL.Service.PayRollManagements.EmpAllowanceTypeOrgaization
{
    public class EmpAllowanceTypeOrganizationService:AppService<EmployeeAllowanceTypes>, IEmpAllowanceTypeOrganizationService
    {
        #region Repositories & Services
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<EmployeeAllowanceTypes> empAllowanceTypes;
        public EmpAllowanceTypeOrganizationService(IGenericRepository<EmployeeAllowanceTypes> empAllowanceTypes, IUserInfoService userInfoService) : base(empAllowanceTypes)
        {
            this.empAllowanceTypes = empAllowanceTypes;
            _userInfoService = userInfoService;
        }
        #endregion


        #region Get All Data
        public async Task<PaginationService<EmployeeAllowanceTypes, GetAllTable>.PaginationResult<GetAllTable>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "", string sortOrder = "desc")
        {
            try
            {
                IQueryable<EmployeeAllowanceTypes> query = empAllowanceTypes.AllActive()
                    .Include(x => x.Organization)
                    .Where(x => x.DeletedAt == null);

                if (!string.IsNullOrEmpty(sortColumn))
                {
                    query = sortColumn switch
                    {
                        "EmployeeAllowanceTypeID" => sortOrder == "desc" ? query.OrderByDescending(x => x.EmployeeAllowanceTypeID) : query.OrderBy(x => x.EmployeeAllowanceTypeID),
                        "OrganizationName" => sortOrder == "desc"
                            ? query.OrderByDescending(x => x.Organization != null ? x.Organization.OrganizationName : null)
                            : query.OrderBy(x => x.Organization != null ? x.Organization.OrganizationName : null),
                        "EmployeeAllowanceTypeName" => sortOrder == "desc" ? query.OrderByDescending(x => x.EmployeeAllowanceTypeName) : query.OrderBy(x => x.EmployeeAllowanceTypeName),
                        _ => query.OrderBy(x => x.EmployeeAllowanceTypeID)
                    };
                }

                if (pageSize == 0)
                {
                    pageSize = await query.CountAsync();
                    pageNumber = 1;
                }

                var result = await PaginationService<EmployeeAllowanceTypes, GetAllTable>.GetPaginatedData(
                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    sortColumn,
                    sortOrder,
                    term => x => (x.Organization != null && EF.Functions.Like(x.Organization.OrganizationName, $"%{term}%")) ||
                    EF.Functions.Like(x.EmployeeAllowanceTypeName.ToString(), $"%{term}%"),

                    x => new GetAllTable
                    {
                        EmployeeAllowanceTypeID = x.EmployeeAllowanceTypeID,
                        OrganizationName = x.Organization != null ? x.Organization.OrganizationName ?? "-" : "-",
                        EmployeeAllowanceTypeName = x.EmployeeAllowanceTypeName,
                        IsApplyOnGrossSalary=x.IsApplyOnGrossSalary==true ? "Yes":"No"
                    });

                return result;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error in GetAllAsync: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw; // Optionally wrap in a custom exception
            }
        }
        #endregion

        #region Save Data
        public async Task<CommonReturnViewModel> SaveAsync(EmpAllowanceTypeOrganizationSaveVM EntityVM)
        {
            // Validate input
            if (EntityVM == null)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Invalid request data."
                };
            }
            if (EntityVM.OrganizationIDs == null || !EntityVM.OrganizationIDs.Any())
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Organization is required."
                };
            }

            if (string.IsNullOrWhiteSpace(EntityVM.EmployeeAllowanceTypeName))
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Allowance Type Name is required."
                };
            }



            // Check for duplicates
            var exists = await empAllowanceTypes.AllActive().AnyAsync(x =>
      EntityVM.OrganizationIDs.Contains((int)x.OrganizationID) &&
      x.EmployeeAllowanceTypeName.Trim() == EntityVM.EmployeeAllowanceTypeName.Trim() &&
      x.EmployeeAllowanceTypeID != EntityVM.EmployeeAllowanceTypeID);

            if (exists)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "This organization and allowance type already exists."
                };
            }

            try
            {
                await empAllowanceTypes.BeginTransactionAsync();
                var entities = EntityVM.OrganizationIDs.Select(orgId => new EmployeeAllowanceTypes
                {
                    OrganizationID = orgId,
                    EmployeeAllowanceTypeName = EntityVM.EmployeeAllowanceTypeName.Trim(),
                    IsApplyOnGrossSalary = EntityVM.IsApplyOnGrossSalary,   
                    LIP = EntityVM.LIP,
                    LMAC = EntityVM.LMAC,
                    CreatedAt = DateTime.Now,
                    CreatedBy = EntityVM.CreatedBy
                }).ToList();
                await empAllowanceTypes.AddRangeAsync(entities);
                await empAllowanceTypes.CommitTransactionAsync();
                foreach (var entity in entities)
                {
                    await _userInfoService.ActionLogAsync(
                        "Allowance Type",
                        ActionName.DataAdd,
                        null,
                        entity,
                        entity.EmployeeAllowanceTypeID,
                        EntityVM
                    );
                }
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Saved successfully.",
                    Data = EntityVM
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await empAllowanceTypes.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "An error occurred while saving. Please try again."
                };
            }
        }
        #endregion

        #region Get by Id Data
        public async Task<EmpAllowanceTypeOrganizationSaveVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await empAllowanceTypes.GetByIdAsync(id);
                var result = new EmpAllowanceTypeOrganizationSaveVM
                {
                    EmployeeAllowanceTypeID = data.EmployeeAllowanceTypeID,
                    OrganizationID = data.OrganizationID,
                    EmployeeAllowanceTypeName = data.EmployeeAllowanceTypeName,

                    //ApplyOnGrossSalary = data.ApplyOnGrossSalary,
                    //ApplyOnBasicSalary = data.ApplyOnBasicSalary,


                };
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Update Data


        public async Task<CommonReturnViewModel> UpdateAsync(EmpAllowanceTypeOrganizationSaveVM EntityVM)
        {


            var exists = await empAllowanceTypes.AllActive().AnyAsync(x =>
    EntityVM.OrganizationIDs.Contains((int)x.OrganizationID) &&
    x.EmployeeAllowanceTypeName.Trim() == EntityVM.EmployeeAllowanceTypeName.Trim() &&
    x.EmployeeAllowanceTypeID != EntityVM.EmployeeAllowanceTypeID);


            if (exists)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Organization and Allowance already exists"
                };
            }

            // Additional validation
            if (string.IsNullOrWhiteSpace(EntityVM.EmployeeAllowanceTypeName))
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Employee Allowance Type Name is required"
                };
            }
            try
            {
                await empAllowanceTypes.BeginTransactionAsync();
                var entity = await empAllowanceTypes.GetByIdAsync(EntityVM.EmployeeAllowanceTypeID);
                var beforeEntity = JsonConvert.DeserializeObject<EmpAllowanceTypeOrganizationSaveVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                if (entity == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Record not found"
                    };
                }
                foreach (var item in EntityVM.OrganizationIDs)
                {
                    entity.OrganizationID = item;
                    entity.EmployeeAllowanceTypeName = EntityVM.EmployeeAllowanceTypeName.Trim();
                    entity.IsApplyOnGrossSalary = EntityVM.IsApplyOnGrossSalary;
                    entity.LIP = EntityVM.LIP;
                    entity.LMAC = EntityVM.LMAC;
                    entity.UpdatedAt = DateTime.UtcNow;
                    entity.UpdatedBy = EntityVM.UpdatedBy;
                }
                await empAllowanceTypes.UpdateAsync(entity);
                var afterEntity = JsonConvert.DeserializeObject<EmpAllowanceTypeOrganizationSaveVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                await _userInfoService.ActionLogAsync("Allowance Type", ActionName.DataUpdated, beforeEntity, afterEntity, entity.EmployeeAllowanceTypeID, EntityVM);
                await empAllowanceTypes.CommitTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Updated Successfully",
                    Data = entity
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await _userInfoService.ActionLogExceptionAsync("Allowance Type", ex, EntityVM.EmployeeAllowanceTypeID, EntityVM, ActionName.Error);
                await empAllowanceTypes.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "An error occurred while updating. Please try again."
                };
            }
        }




        #endregion

        #region Delete 

        public async Task<CommonReturnViewModel> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await empAllowanceTypes.BeginTransactionAsync();
            try
            {
                var data = await empAllowanceTypes.FindAsync(x => requestVM.Ids.Contains(x.EmployeeAllowanceTypeID));
                if (data == null || data.Count == 0)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "No data found to soft delete."
                    };
                }


                var beforeEntity = JsonConvert.DeserializeObject<List<EmpAllowanceTypeOrganizationSaveVM>>(JsonConvert.SerializeObject(data, JsonSettings.IgnoreReferenceLoop));
                var targetIds = data.Select(x => (int?)x.EmployeeAllowanceTypeID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.UtcNow;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await empAllowanceTypes.UpdateRangeAsync(data);
                await _userInfoService.ActionLogDeleteAsync("Allowance Type", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await empAllowanceTypes.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Deleted successfully."
                };
            }
            catch (Exception ex)
            {
                await empAllowanceTypes.RollbackTransactionAsync();
                throw new Exception("Error occurred during the deletion of data.", ex);
            }
        }

        #endregion
        #region Get Allowance Type 

        public async Task<CommonSelectVM> SelectAsync(int id)
        {
            try
            {
                var data=await empAllowanceTypes.AllActive().Where(x=>x.OrganizationID==id).FirstOrDefaultAsync();
                if (data ==null)
                {
                    return new CommonSelectVM();
                }
                var result = new CommonSelectVM
                {
                    Id=data.OrganizationID,
                    Name=data.EmployeeAllowanceTypeName
                };
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
