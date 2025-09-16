using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.PayrollManagements.PayRollOrganizationBenefitsType;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmpAllowanceOrganization;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using GCTL.Service.PayRollManagements.EmpAllowanceTypeOrgaization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollOrgaBenefitsType
{
    public class PayRollOrgaBenefitsTypeService : AppService<BenefitTypes>, IPayRollOrgaBenefitsTypeService
    {
        private readonly IGenericRepository<BenefitTypes> benefitYpe;
        private readonly IGenericRepository<Organization> organization;
         private readonly IUserInfoService _userInfoService;
        public PayRollOrgaBenefitsTypeService(IGenericRepository<BenefitTypes> genericRepository, IGenericRepository<BenefitTypes> benefitYpe, IGenericRepository<Organization> organization, IUserInfoService userInfoService) : base(genericRepository)
        {
            this.benefitYpe = benefitYpe;
            this.organization = organization;
            _userInfoService = userInfoService;
        }

        public async Task<List<CommonSelectVM>> GetAllEmploye()
        {
            try
            {
                var result = await organization.AllActive().Select(e => new CommonSelectVM
                {
                    Id = e.OrganizationID,
                    Name = e.OrganizationName
                }).ToListAsync();

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<CommonReturnViewModel> Save(OrgaBenefitsTypeSaveVM model)
        {
            var response = new CommonReturnViewModel();

            try
            {
                if (model == null)
                {
                    response.Success = false;
                    response.Message = "Model cannot be null.";
                    response.Errors.Add("Invalid input: Model is null.");
                    return response;
                }

               
                
                if (string.IsNullOrWhiteSpace(model.BenefitTypeName))
                {
                    response.Success = false;
                    response.Message = "Benefit type name is required.";
                    response.Errors.Add("BenefitTypeName cannot be empty.");
                    return response;
                }
                // Check for duplicate benefit type
                //var existingBenefitType = await benefitYpe
                //    .FindAsync(b => b.OrganizationID == model.OrganizatonID
                //        && b.BenefitTypeName.ToLower() == model.BenefitTypeName.ToLower() && b.BenefitTypeID != model.BenefitTypeID);



                //if (existingBenefitType != null)
                //{
                //    response.Success = false;
                //    response.Message = "Benefit type already exists.";
                //    response.Errors.Add($"A benefit type with name '{model.BenefitTypeName}' already exists for this organization.");
                //    return response;
                //}

                await benefitYpe.BeginTransactionAsync();
                foreach(var item in model.OrganizatonIDs)
                {
                    var entity = new BenefitTypes
                    {
                        OrganizationID = item,
                        BenefitTypeName = model.BenefitTypeName.Trim(),

                        //ApplyOnGrossSalary = model.ApplyOnGrossSalary,
                        //ApplyOnBasicSalary = model.ApplyOnBasicSalary,

                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = model.CreatedBy,
                        LIP = model.LIP,
                        LMAC = model.LMAC,
                    };

                    await benefitYpe.AddAsync(entity);

                }
              
                await benefitYpe.CommitTransactionAsync();

                response.Success = true;
                response.Message = "Benefit type saved successfully.";
            }
            catch (Exception ex)
            {
                await benefitYpe.RollbackTransactionAsync();
                response.Success = false;
                response.Message = "Failed to save benefit type.";
                response.Errors.Add(ex.Message);

            }

            return response;
        }

        public async Task<CommonReturnViewModel> Update(OrgaBenefitsTypeSaveVM model)
        {
            try
            {
                await benefitYpe.BeginTransactionAsync();

                var entity = await benefitYpe.GetByIdAsync(model.BenefitTypeID);

                if (entity == null)
                {
                    await benefitYpe.RollbackTransactionAsync();
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Benefit type not found."
                    };
                }
                foreach (var item in model.OrganizatonIDs) {
                    entity.BenefitTypeName = model.BenefitTypeName;
                    entity.OrganizationID = item;

                    //entity.ApplyOnBasicSalary = model.ApplyOnBasicSalary;
                    //entity.ApplyOnGrossSalary = model.ApplyOnGrossSalary;

                    entity.UpdatedAt = DateTime.UtcNow;
                    entity.UpdatedBy = model.UpdatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;
                    await benefitYpe.UpdateAsync(entity);
                }
                // update fields
             

                
                await benefitYpe.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Benefit type updated successfully.",
                    Data = entity
                };
            }
            catch (Exception ex)
            {
                await benefitYpe.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }


        #region Get All Data
        public async Task<PaginationService<BenefitTypes, OrgaBenefitsTypeGetAllVM>.PaginationResult<OrgaBenefitsTypeGetAllVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "", string sortOrder = "desc")
        {
            try
            {
                IQueryable<BenefitTypes> query = benefitYpe.AllActive()
                    .Include(x => x.Organization);


                if (!string.IsNullOrEmpty(sortColumn))
                {
                    query = sortColumn switch
                    {
                        "EmployeeAllowanceTypeID" => sortOrder == "desc" ? query.OrderByDescending(x => x.BenefitTypeID) : query.OrderBy(x => x.BenefitTypeID),
                        "OrganizationName" => sortOrder == "desc"
                            ? query.OrderByDescending(x => x.Organization != null ? x.Organization.OrganizationName : null)
                            : query.OrderBy(x => x.Organization != null ? x.Organization.OrganizationName : null),
                        "EmployeeAllowanceTypeName" => sortOrder == "desc" ? query.OrderByDescending(x => x.BenefitTypeName) : query.OrderBy(x => x.BenefitTypeName),
                        _ => query.OrderBy(x => x.BenefitTypeID)
                    };
                }

                if (pageSize == 0)
                {
                    pageSize = await query.CountAsync();
                    pageNumber = 1;
                }

                var result = await PaginationService<BenefitTypes, OrgaBenefitsTypeGetAllVM>.GetPaginatedData(
                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    sortColumn,
                    sortOrder,
                    term => x => (x.Organization != null && EF.Functions.Like(x.Organization.OrganizationName, $"%{term}%")) ||
                    EF.Functions.Like(x.BenefitTypeName.ToString(), $"%{term}%"),

                    x => new OrgaBenefitsTypeGetAllVM
                    {
                        BenefitTypeID = x.BenefitTypeID,
                        OrganizationName = x.Organization != null ? x.Organization.OrganizationName ?? "-" : "-",
                        BenefitTypeName = x.BenefitTypeName,
                        IsApplyOnGrossSalary = x.IsApplyOnGrossSalary == true ? "Yes" : "No",

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
        public async Task<CommonReturnViewModel> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await benefitYpe.BeginTransactionAsync();
            try
            {
                var data = await benefitYpe.FindAsync(x => requestVM.Ids.Contains(x.BenefitTypeID));
                if (data == null || data.Count == 0)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "No data found to soft delete."
                    };
                }


                var beforeEntity = JsonConvert.DeserializeObject<List<OrgaBenefitsTypeSaveVM>>(JsonConvert.SerializeObject(data, JsonSettings.IgnoreReferenceLoop));
                var targetIds = data.Select(x => (int?)x.BenefitTypeID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await benefitYpe.UpdateRangeAsync(data);
                await _userInfoService.ActionLogDeleteAsync("Benfits  Type", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await benefitYpe.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Deleted successfully."
                };
            }
            catch (Exception ex)
            {
                await benefitYpe.RollbackTransactionAsync();
                throw new Exception("Error occurred during the deletion of data.", ex);
            }
        }

        public async Task<OrgaBenefitsTypeSaveVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await benefitYpe.GetByIdAsync(id);
                var result = new OrgaBenefitsTypeSaveVM
                {
                    BenefitTypeID = data.BenefitTypeID,
                    OrganizatonID = data.OrganizationID,
                    BenefitTypeName = data.BenefitTypeName,

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
    }
}
