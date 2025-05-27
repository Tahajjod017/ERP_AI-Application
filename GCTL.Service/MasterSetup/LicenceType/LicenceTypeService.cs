using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.Grade;
using GCTL.Core.ViewModels.MasterSetup.LicenceType;
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

namespace GCTL.Service.MasterSetup.LicenceType
{
    public class LicenceTypeService : AppService<LicenceTypes>, ILicenceTypeService
    {
        #region Services & Repositories
        private readonly IGenericRepository<LicenceTypes> _genericRepository;
        private readonly IUserInfoService _userInfoService;

        public LicenceTypeService(IGenericRepository<LicenceTypes> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(LicenceTypeVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.LicenceTypeName == model.LicenceTypeName && b.DeletedAt != null);
                if (existingEntity.Any())
                {

                    var entityToRestore = existingEntity.FirstOrDefault();
                    if (entityToRestore == null) return false;

                    entityToRestore.LicenceTypeName = model.LicenceTypeName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;
                    entityToRestore.UpdatedBy = model.UpdatedBy ?? null;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    var afterEntity = JsonConvert.DeserializeObject<LicenceTypeVM>(JsonConvert.SerializeObject(entityToRestore));
                    await _userInfoService.ActionLogAsync("Licence Type", ActionName.DataAdd, null, entityToRestore, entityToRestore.LicenceTypeID, model);
                }
                else
                {
                    LicenceTypes entity = new LicenceTypes();
                    entity.LicenceTypeName = model.LicenceTypeName;
                    entity.CreatedAt = DateTime.Now;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;
                    entity.CreatedBy = model.CreatedBy ?? null;
                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Licence Type", ActionName.DataAdd, null, entity, entity.LicenceTypeID, model);
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
        public async Task<bool> UpdateAsync(LicenceTypeVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.LicenceTypeID);
                if (entity == null)
                {
                    return false;
                }
                var beforeEntity = JsonConvert.DeserializeObject<LicenceTypeVM>(JsonConvert.SerializeObject(entity));
                entity.LicenceTypeName = model.LicenceTypeName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;
                entity.UpdatedBy = model.UpdatedBy ?? null;
                await _genericRepository.UpdateAsync(entity);
                var afterEntity = JsonConvert.DeserializeObject<LicenceTypeVM>(JsonConvert.SerializeObject(entity));
                await _userInfoService.ActionLogAsync("Licence Type", ActionName.DataUpdated, beforeEntity, afterEntity, entity.LicenceTypeID, model);
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
        public async Task<LicenceTypeVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new LicenceTypeVM
                {
                    LicenceTypeID = data.LicenceTypeID,
                    LicenceTypeName = data.LicenceTypeName,
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
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.LicenceTypeName != null);

            var nameList = existingNames.Select(b => b.LicenceTypeName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region SoftDeleteAsync
        public async Task<LicenceTypeVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.LicenceTypeID));
                if (data == null || data.Count == 0)
                {
                    return new LicenceTypeVM
                    {
                        Message = "No data found to delete."
                    };
                }
                var beforeEntity = JsonConvert.DeserializeObject<List<LicenceTypeVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.LicenceTypeID).ToList();
                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                    item.DeletedBy = requestVM.DeletedBy ?? null;
                }

                await _genericRepository.UpdateRangeAsync(data);
                await _userInfoService.ActionLogDeleteAsync("Licence Type", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);
                await _genericRepository.CommitTransactionAsync();

                return new LicenceTypeVM
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
        public async Task<PaginationService<LicenceTypes, LicenceTypeVM>.PaginationResult<LicenceTypeVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "LicenceTypeID", string sortOrder = "desc")
        {
            var query = _genericRepository.AllActive();

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "LicenceTypeID" => sortOrder == "desc" ? query.OrderByDescending(x => x.LicenceTypeID) : query.OrderBy(x => x.LicenceTypeID),
                    "LicenceTypeName" => sortOrder == "desc" ? query.OrderByDescending(x => x.LicenceTypeName) : query.OrderBy(x => x.LicenceTypeName),
                    _ => query.OrderBy(x => x.LicenceTypeID)
                };
            }

            var result = await PaginationService<LicenceTypes, LicenceTypeVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.LicenceTypeName, $"%{term}%"),
                x => new LicenceTypeVM
                {
                    LicenceTypeID = x.LicenceTypeID,
                    LicenceTypeName = x.LicenceTypeName ?? "-",
                });

            return result;
        }
        #endregion
    }
}
