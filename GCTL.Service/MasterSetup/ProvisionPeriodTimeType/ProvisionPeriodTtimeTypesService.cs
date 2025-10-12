using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.Grade;
using GCTL.Core.ViewModels.MasterSetup.ProvisionPeriodTtimeTypes;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.MasterSetup.ProvisionPeriodTimeType
{
    public class ProvisionPeriodTtimeTypesService : AppService<ProvisionPeriodTtimeTypes>, IProvisionPeriodTtimeTypesService
    {
        #region Services & Repositories
        private readonly IGenericRepository<ProvisionPeriodTtimeTypes> _genericRepository;
        private readonly IUserInfoService _userInfoService;

        public ProvisionPeriodTtimeTypesService(IGenericRepository<ProvisionPeriodTtimeTypes> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(ProvisionPeriodTtimeTypesVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.ProvisionPeriodTtimeTypeName == model.ProvisionPeriodTtimeTypeName && b.DeletedAt != null);
                if (existingEntity.Any())
                {

                    var entityToRestore = existingEntity.FirstOrDefault();
                    if (entityToRestore == null) return false;

                    entityToRestore.ProvisionPeriodTtimeTypeName = model.ProvisionPeriodTtimeTypeName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;
                    entityToRestore.UpdatedBy = model.UpdatedBy ?? null;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    var afterEntity = JsonConvert.DeserializeObject<ProvisionPeriodTtimeTypesVM>(JsonConvert.SerializeObject(entityToRestore));
                    await _userInfoService.ActionLogAsync("Provision Period", ActionName.DataAdd, null, entityToRestore, entityToRestore.ProvisionPeriodTtimeTypeID, model);
                }
                else
                {
                    ProvisionPeriodTtimeTypes entity = new ProvisionPeriodTtimeTypes();
                    entity.ProvisionPeriodTtimeTypeName = model.ProvisionPeriodTtimeTypeName;
                    entity.CreatedAt = DateTime.Now;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;
                    entity.CreatedBy = model.CreatedBy ?? null;
                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Provision Period", ActionName.DataAdd, null, entity, entity.ProvisionPeriodTtimeTypeID, model);
                }

                await _genericRepository.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                throw new Exception(ex.Message, ex);
                //return false;
            }
        }
        #endregion


        #region Update
        public async Task<bool> UpdateAsync(ProvisionPeriodTtimeTypesVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.ProvisionPeriodTtimeTypeID);
                if (entity == null)
                {
                    return false;
                }
                var beforeEntity = JsonConvert.DeserializeObject<ProvisionPeriodTtimeTypesVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                entity.ProvisionPeriodTtimeTypeName = model.ProvisionPeriodTtimeTypeName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;
                entity.UpdatedBy = model.UpdatedBy ?? null;
                await _genericRepository.UpdateAsync(entity);
                var afterEntity = JsonConvert.DeserializeObject<ProvisionPeriodTtimeTypesVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                await _userInfoService.ActionLogAsync("Provision Period", ActionName.DataUpdated, beforeEntity, afterEntity, entity.ProvisionPeriodTtimeTypeID, model);
                await _genericRepository.CommitTransactionAsync();

                return true;
            }
            catch
            {
                await _genericRepository.RollbackTransactionAsync();
                return false;
            }
        }
        #endregion


        #region GetByIdAsync
        public async Task<ProvisionPeriodTtimeTypesVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new ProvisionPeriodTtimeTypesVM
                {
                    ProvisionPeriodTtimeTypeID = data.ProvisionPeriodTtimeTypeID,
                    ProvisionPeriodTtimeTypeName = data.ProvisionPeriodTtimeTypeName,
                };
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., to a file or logging service)
                throw; // Rethrow or return an error-specific response
            }
        }
        #endregion


        #region IsNameUniqueAsync
        public async Task<bool> IsNameUniqueAsync(string name)
        {
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.ProvisionPeriodTtimeTypeName != null);

            var nameList = existingNames.Select(b => b.ProvisionPeriodTtimeTypeName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region SoftDeleteAsync
        public async Task<ProvisionPeriodTtimeTypesVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.ProvisionPeriodTtimeTypeID));
                if (data == null || data.Count == 0)
                {
                    return new ProvisionPeriodTtimeTypesVM
                    {
                        Message = "No data found to delete."
                    };
                }
                var beforeEntity = JsonConvert.DeserializeObject<List<ProvisionPeriodTtimeTypesVM>>(JsonConvert.SerializeObject(data, JsonSettings.IgnoreReferenceLoop));
                var targetIds = data.Select(x => (int?)x.ProvisionPeriodTtimeTypeID).ToList();
                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                    item.DeletedBy = requestVM.DeletedBy ?? null;
                }

                await _genericRepository.UpdateRangeAsync(data);
                await _userInfoService.ActionLogDeleteAsync("Provision Period", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);
                await _genericRepository.CommitTransactionAsync();

                return new ProvisionPeriodTtimeTypesVM
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
        public async Task<PaginationService<ProvisionPeriodTtimeTypes, ProvisionPeriodTtimeTypesVM>.PaginationResult<ProvisionPeriodTtimeTypesVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "ProvisionPeriodTtimeTypeID", string sortOrder = "desc")
        {
            var query = _genericRepository.AllActive();

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "ProvisionPeriodTtimeTypeID" => sortOrder == "desc" ? query.OrderByDescending(x => x.ProvisionPeriodTtimeTypeID) : query.OrderBy(x => x.ProvisionPeriodTtimeTypeID),
                    "ProvisionPeriodTtimeTypeName" => sortOrder == "desc" ? query.OrderByDescending(x => x.ProvisionPeriodTtimeTypeName) : query.OrderBy(x => x.ProvisionPeriodTtimeTypeName),
                    _ => query.OrderBy(x => x.ProvisionPeriodTtimeTypeID)
                };
            }

            var result = await PaginationService<ProvisionPeriodTtimeTypes, ProvisionPeriodTtimeTypesVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.ProvisionPeriodTtimeTypeName, $"%{term}%"),
                x => new ProvisionPeriodTtimeTypesVM
                {
                    ProvisionPeriodTtimeTypeID = x.ProvisionPeriodTtimeTypeID,
                    ProvisionPeriodTtimeTypeName = x.ProvisionPeriodTtimeTypeName ?? "-",
                });

            return result;
        }
        #endregion
    }
}
