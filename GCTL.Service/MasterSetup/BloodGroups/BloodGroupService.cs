using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.BloodGroup;
using GCTL.Core.ViewModels;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.MasterSetup.BloodGroups
{
    public class BloodGroupService : AppService<BloodGroup>, IBloodGroupService
    {
        #region Repositories & Services
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<BloodGroup> _genericRepository;

        public BloodGroupService(IGenericRepository<BloodGroup> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(BloodGroupVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.BloodGroupName == model.BloodGroupName && b.DeletedAt != null);
                if (existingEntity.Any())
                {
                    var entityToRestore = existingEntity.FirstOrDefault();

                    entityToRestore.BloodGroupName = model.BloodGroupName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    await _userInfoService.ActionLogAsync("Blood Group", ActionName.DataAdd, null, entityToRestore, entityToRestore.BloodGroupID, model);
                }
                else
                {
                    BloodGroup entity = new BloodGroup();
                    entity.BloodGroupName = model.BloodGroupName;
                    entity.CreatedAt = DateTime.Now;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Blood Group", ActionName.DataAdd, null, entity, entity.BloodGroupID, model);
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
        public async Task<bool> UpdateAsync(BloodGroupVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.BloodGroupID);
                if (entity == null)
                {
                    return false;
                }

                var beforeEntity = JsonConvert.DeserializeObject<BloodGroupVM>(JsonConvert.SerializeObject(entity));

                entity.BloodGroupName = model.BloodGroupName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<BloodGroupVM>(JsonConvert.SerializeObject(entity));
                await _userInfoService.ActionLogAsync("Blood Group", ActionName.DataUpdated, beforeEntity, afterEntity, entity.BloodGroupID, model);

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


        #region Get
        public async Task<BloodGroupVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new BloodGroupVM
                {
                    BloodGroupID = data.BloodGroupID,
                    BloodGroupName = data.BloodGroupName,
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
            var existingName = await _genericRepository.FindAsync(b => b.BloodGroupName == name && b.DeletedAt == null);
            return !existingName.Any();
        }
        #endregion


        #region Soft Delete
        public async Task<BloodGroupVM> SoftDeleteAsync(BaseViewModel model, List<int> ids)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => ids.Contains(x.BloodGroupID));
                if (data == null || data.Count == 0)
                {
                    return new BloodGroupVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<BloodGroupVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.BloodGroupID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = model.DeletedBy;
                    item.LIP = model.LIP;
                    item.LMAC = model.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("Blood Group", ActionName.DataDeleted, null, beforeEntity, targetIds, model);

                await _genericRepository.CommitTransactionAsync();

                return new BloodGroupVM
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
        public async Task<PaginationService<BloodGroup, BloodGroupVM>.PaginationResult<BloodGroupVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "BloodGroupID", string sortOrder = "desc")
        {
            var query = _genericRepository.All();
            query = query.Where(x => x.DeletedAt == null);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "BloodGroupID" => sortOrder == "desc" ? query.OrderByDescending(x => x.BloodGroupID) : query.OrderBy(x => x.BloodGroupID),
                    "BloodGroupName" => sortOrder == "desc" ? query.OrderByDescending(x => x.BloodGroupName) : query.OrderBy(x => x.BloodGroupName),
                    _ => query.OrderBy(x => x.BloodGroupID)
                };
            }

            return await PaginationService<BloodGroup, BloodGroupVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.BloodGroupName, $"%{term}%"),
                x => new BloodGroupVM
                {
                    BloodGroupID = x.BloodGroupID,
                    BloodGroupName = x.BloodGroupName ?? "-",
                });
        }
        #endregion
    }
}
