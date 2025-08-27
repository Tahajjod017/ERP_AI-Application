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
                        "PSettingID" => sortOrder == "desc" ? query.OrderByDescending(x => x.EmployeeAllowanceTypeID) : query.OrderBy(x => x.EmployeeAllowanceTypeID),
                        "OrganizationName" => sortOrder == "desc"
                            ? query.OrderByDescending(x => x.Organization != null ? x.Organization.OrganizationName : null)
                            : query.OrderBy(x => x.Organization != null ? x.Organization.OrganizationName : null),
                        "TaxPercentage" => sortOrder == "desc" ? query.OrderByDescending(x => x.EmployeeAllowanceTypeName) : query.OrderBy(x => x.EmployeeAllowanceTypeName),
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
                        EmployeeAllowanceTypeName = x.EmployeeAllowanceTypeName
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
        public async Task<CommonReturnViewModel> SaveAsync([FromBody]EmpAllowanceTypeOrganizationSaveVM EntityVM)
        {
            var result = new CommonReturnViewModel();

            // Validate input
            if (EntityVM == null)
            {
                result.Success = false;
                result.Message = "Data cannot be null";
                return result;
            }
            var exists = await empAllowanceTypes.AllActive().AnyAsync(x => x.OrganizationID == EntityVM.OrganizationID
               && x.EmployeeAllowanceTypeName == EntityVM.EmployeeAllowanceTypeName
               && x.EmployeeAllowanceTypeID != EntityVM.EmployeeAllowanceTypeID);

            if (exists)
            {
                result.Success = false;
                result.Message = "Organization and Allowance already exists";
                return result;
            }

            // Additional validation
            if (string.IsNullOrWhiteSpace(EntityVM.EmployeeAllowanceTypeName))
            {
                result.Success = false;
                result.Message = "Employee Allowance Type Name is required";
                return result;
            }

            try
            {
                await empAllowanceTypes.BeginTransactionAsync();

                EmployeeAllowanceTypes Entity = new EmployeeAllowanceTypes
                {
                    OrganizationID = EntityVM.OrganizationID,
                    EmployeeAllowanceTypeName = EntityVM.EmployeeAllowanceTypeName.Trim(),
                    LIP = EntityVM.LIP,
                    LMAC = EntityVM.LMAC,
                    CreatedAt = DateTime.Now,
                    CreatedBy = EntityVM.CreatedBy,
                };

                await empAllowanceTypes.AddAsync(Entity);
                await empAllowanceTypes.CommitTransactionAsync();

                result.Success = true;
                result.Message = "Saved Successfully";
                result.Data = Entity; // Optionally return the created entity
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                await empAllowanceTypes.RollbackTransactionAsync();
                result.Success = false;
                result.Message = "An error occurred while saving. Please try again.";
            }

            return result;
        }
        public async Task<EmpAllowanceTypeOrganizationSaveVM> GetByIdAsync(int id)
        {
            try
            {
                var data =await empAllowanceTypes.GetByIdAsync(id);
                var result = new EmpAllowanceTypeOrganizationSaveVM
                { 
                  EmployeeAllowanceTypeID = data.EmployeeAllowanceTypeID,  
                OrganizationID= data.OrganizationID,
                 EmployeeAllowanceTypeName= data.EmployeeAllowanceTypeName,
                
                };
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<CommonReturnViewModel> UpdateAsync(EmpAllowanceTypeOrganizationSaveVM EntityVM)
        {

            var result = new CommonReturnViewModel();

            // Validate input
            if (EntityVM == null)
            {
                result.Success = false;
                result.Message = "Data cannot be null";
                return result;
            }
            //var exists = await empAllowanceTypes.AllActive().AnyAsync(x => x.OrganizationID == EntityVM.OrganizationID
            //   && x.EmployeeAllowanceTypeName == EntityVM.EmployeeAllowanceTypeName
            //   && x.EmployeeAllowanceTypeID != EntityVM.EmployeeAllowanceTypeID);

            //if (exists)
            //{
            //    result.Success = false;
            //    result.Message = "Organization and Allowance already exists";
            //    return result;
            //}

            //// Additional validation
            //if (string.IsNullOrWhiteSpace(EntityVM.EmployeeAllowanceTypeName))
            //{
            //    result.Success = false;
            //    result.Message = "Employee Allowance Type Name is required";
            //    return result;
            //}

            try
            {
                await empAllowanceTypes.BeginTransactionAsync();
                var Entity= await empAllowanceTypes.GetByIdAsync(EntityVM.EmployeeAllowanceTypeID);

                Entity.OrganizationID = EntityVM.OrganizationID;
                Entity.EmployeeAllowanceTypeName = EntityVM.EmployeeAllowanceTypeName.Trim();
                Entity.LIP = EntityVM.LIP;
                Entity.LMAC = EntityVM.LMAC;
                Entity.UpdatedAt = DateTime.Now;
                Entity.UpdatedBy = EntityVM.UpdatedBy;
                

                await empAllowanceTypes.AddAsync(Entity);
                await empAllowanceTypes.CommitTransactionAsync();

                result.Success = true;
                result.Message = "Saved Successfully";
                result.Data = Entity; // Optionally return the created entity
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                await empAllowanceTypes.RollbackTransactionAsync();
                result.Success = false;
                result.Message = "An error occurred while saving. Please try again.";
            }

            return result;
        }
        public static readonly JsonSerializerSettings IgnoreReferenceLoop = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
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


                var beforeEntity = JsonConvert.DeserializeObject<List<EmpAllowanceTypeOrganizationSaveVM>>(
             JsonConvert.SerializeObject(data, JsonSettings.IgnoreReferenceLoop)
         );

                var targetIds = data.Select(x => (int?)x.EmployeeAllowanceTypeID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await empAllowanceTypes.UpdateRangeAsync(data);
                await _userInfoService.ActionLogDeleteAsync("Allowance Type", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await empAllowanceTypes.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success= true,
                    Message = $"{data.Count} data(s) deleted successfully."
                };
            }
            catch (Exception ex)
            {
                await empAllowanceTypes.RollbackTransactionAsync();
                throw new Exception("Error occurred during the deletion of data.", ex);
            }
        }

        
        #endregion
    }
}
