using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.BloodGroup;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollSettings;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.MasterSetup.BloodGroups;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollSettings
{
    public class PayRollTaxpercentageSettingsService : AppService<PSettings>, IPayRollTaxperCentangeSettingsService
    {
       #region Repositories & Services
            private readonly IUserInfoService _userInfoService;
            private readonly IGenericRepository<PSettings> _genericRepository;

            public PayRollTaxpercentageSettingsService(IGenericRepository<PSettings> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
            {
                _genericRepository = genericRepository;
                _userInfoService = userInfoService;
            }
            #endregion


            #region AddAsync
            public async Task<bool> AddAsync(PayRollTaxPercentageSaveVM model)
            {
                await _genericRepository.BeginTransactionAsync();
                try
                {
                    var existingEntity = await _genericRepository.FindAsync(b => b.PSettingID == model.PSettingID && b.DeletedAt != null);
                    if (existingEntity.Any())
                    {
                        var entityToRestore = existingEntity.FirstOrDefault();

                        entityToRestore.OrganizationID = model.OrganizationID;
                     
                        entityToRestore.CreatedAt = DateTime.Now;
                        entityToRestore.CreatedBy = model.CreatedBy;
                        entityToRestore.LIP = model.LIP;
                        entityToRestore.LMAC = model.LMAC;

                        entityToRestore.DeletedAt = null;
                        entityToRestore.UpdatedAt = DateTime.Now;

                        await _genericRepository.UpdateAsync(entityToRestore);
                        await _userInfoService.ActionLogAsync("Tax Perceentage", ActionName.DataAdd, null, entityToRestore, entityToRestore.PSettingID, model);
                    }
                    else
                    {
                        PSettings entity = new PSettings();
                        entity.OrganizationID = model.OrganizationID;
                        entity.TaxPercentage = model.TaxPercentage;
                        entity.CreatedAt = DateTime.Now;
                        entity.CreatedBy = model.CreatedBy;
                        entity.LIP = model.LIP;
                        entity.LMAC = model.LMAC;

                        await _genericRepository.AddAsync(entity);
                        await _userInfoService.ActionLogAsync("Tax Percentage", ActionName.DataAdd, null, entity, entity.PSettingID, model);
                    }

                    await _genericRepository.CommitTransactionAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    await _genericRepository.RollbackTransactionAsync();
                    //throw ex;
                    return false;
                }
            }
            #endregion


            #region Update
            public async Task<bool> UpdateAsync(PayRollTaxpercentageUpdateVM model)
            {
                await _genericRepository.BeginTransactionAsync();
                try
                {
                    var entity = await _genericRepository.GetByIdAsync(model.PSettingID);
                    if (entity == null)
                    {
                        return false;
                    }

                    var beforeEntity = JsonConvert.DeserializeObject<PayRollTaxpercentageUpdateVM>(JsonConvert.SerializeObject(entity));

                    entity.OrganizationID = model.OrganizationID;
                    entity.TaxPercentage=model.TaxPercentage;
                    entity.UpdatedAt = DateTime.Now;
                    entity.UpdatedBy = model.UpdatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.UpdateAsync(entity);

                    var afterEntity = JsonConvert.DeserializeObject<PayRollTaxpercentageUpdateVM>(JsonConvert.SerializeObject(entity));
                    await _userInfoService.ActionLogAsync("Tax Percenatge", ActionName.DataUpdated, beforeEntity, afterEntity, entity.PSettingID, model);

                    await _genericRepository.CommitTransactionAsync();

                    return true;
                }
                catch(Exception ex)
                {
                    await _genericRepository.RollbackTransactionAsync();
                    return false;
                }
            }
            #endregion


            #region Get
            public async Task<PayRollTaxpercentageUpdateVM> GetByIdAsync(int id)
            {
                try
                {
                    var data = await _genericRepository.GetByIdAsync(id);
                    if (data == null) return null;

                    return new PayRollTaxpercentageUpdateVM
                    {  
                        PSettingID=data.PSettingID,
                        OrganizationID = data.OrganizationID,
                        TaxPercentage = data.TaxPercentage,
                    };
                }
                catch (Exception ex)
                {
                    
                    throw; // Rethrow or return an error-specific response
                }
            }
            #endregion


            #region IsNameUniqueAsync
            public async Task<bool> IsNameUniqueAsync(string name)
            {
                var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.OrganizationID != null);

                var nameList = existingNames.Select(b => b.Organization.OrganizationName);

                return !DuplicateChecker.IsDuplicate(name, nameList);
            }
            #endregion


            #region Soft Delete
            public async Task<PayRollTaxpercentageUpdateVM> SoftDeleteAsync(DeleteRequestVM requestVM)
            {
                await _genericRepository.BeginTransactionAsync();
                try
                {
                    var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.PSettingID));
                    if (data == null || data.Count == 0)
                    {
                        return new PayRollTaxpercentageUpdateVM
                        {
                            Message = "No data found to soft delete."
                        };
                    }

                    var beforeEntity = JsonConvert.DeserializeObject<List<PayRollTaxpercentageUpdateVM>>(JsonConvert.SerializeObject(data));
                    var targetIds = data.Select(x => (int?)x.PSettingID).ToList();

                    foreach (var item in data)
                    {
                        item.DeletedAt = DateTime.Now;
                        item.DeletedBy = requestVM.DeletedBy;
                        item.LIP = requestVM.LIP;
                        item.LMAC = requestVM.LMAC;
                    }

                    await _genericRepository.UpdateRangeAsync(data);

                    await _userInfoService.ActionLogDeleteAsync("Blood Group", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                    await _genericRepository.CommitTransactionAsync();

                    return new PayRollTaxpercentageUpdateVM
                    {
                        Message = $"{data.Count} data(s) deleted successfully."
                    };
                }
                catch (Exception ex)
                {
                    await _genericRepository.RollbackTransactionAsync();
                    throw new Exception("Error occurred during the deletion of data.", ex);
                }
            }
        #endregion


        #region GetAllAsync
       

        public async Task<PaginationService<PSettings, PayRollTaxpercentageGetAllVM>.PaginationResult<PayRollTaxpercentageGetAllVM>> GetAllAsync(
    int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "PSettingID", string sortOrder = "desc")
        {
            try
            {
                IQueryable<PSettings> query = _genericRepository.AllActive()
                    .Include(x => x.Organization)
                    .Where(x => x.DeletedAt == null);

                if (!string.IsNullOrEmpty(sortColumn))
                {
                    query = sortColumn switch
                    {
                        "PSettingID" => sortOrder == "desc" ? query.OrderByDescending(x => x.PSettingID) : query.OrderBy(x => x.PSettingID),
                        "OrganizationName" => sortOrder == "desc"
                            ? query.OrderByDescending(x => x.Organization != null ? x.Organization.OrganizationName : null)
                            : query.OrderBy(x => x.Organization != null ? x.Organization.OrganizationName : null),
                        "TaxPercentage" => sortOrder == "desc" ? query.OrderByDescending(x => x.TaxPercentage) : query.OrderBy(x => x.TaxPercentage),
                        _ => query.OrderBy(x => x.PSettingID)
                    };
                }

                if (pageSize == 0)
                {
                    pageSize = await query.CountAsync();
                    pageNumber = 1;
                }

                var result = await PaginationService<PSettings, PayRollTaxpercentageGetAllVM>.GetPaginatedData(
                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    sortColumn,
                    sortOrder,
                    term => x => (x.Organization != null && EF.Functions.Like(x.Organization.OrganizationName, $"%{term}%")) ||
                 EF.Functions.Like(x.TaxPercentage.ToString(), $"%{term}%"),

                    x => new PayRollTaxpercentageGetAllVM
                    {
                        PSettingID = x.PSettingID,
                        OrganizationName = x.Organization != null ? x.Organization.OrganizationName ?? "-" : "-",
                        TaxPercentage = x.TaxPercentage
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
    }
    }

