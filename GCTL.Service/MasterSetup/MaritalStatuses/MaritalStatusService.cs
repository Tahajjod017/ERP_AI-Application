using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.MaritalStatus;
using GCTL.Core.ViewModels;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GCTL.Data.Models;

namespace GCTL.Service.MasterSetup.MaritalStatuses
{
    public class MaritalStatusService : AppService<MaritalStatus>, IMaritalStatusService
    {
        #region Repositories & Services
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<MaritalStatus> _genericRepository;

        public MaritalStatusService(IGenericRepository<MaritalStatus> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(MaritalStatusVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.MaritalStatusName == model.MaritalStatusName && b.DeletedAt != null);
                if (existingEntity.Any())
                {
                    var entityToRestore = existingEntity.FirstOrDefault();

                    entityToRestore.MaritalStatusName = model.MaritalStatusName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    await _userInfoService.ActionLogAsync("Marital Status", ActionName.DataAdd, null, entityToRestore, entityToRestore.MaritalStatusID, model);
                }
                else
                {
                    MaritalStatus entity = new MaritalStatus();
                    entity.MaritalStatusName = model.MaritalStatusName;
                    entity.CreatedAt = DateTime.Now;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Marital Status", ActionName.DataAdd, null, entity, entity.MaritalStatusID, model);
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
        public async Task<bool> UpdateAsync(MaritalStatusVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.MaritalStatusID);
                if (entity == null)
                {
                    return false;
                }

                var beforeEntity = JsonConvert.DeserializeObject<MaritalStatusVM>(JsonConvert.SerializeObject(entity));

                entity.MaritalStatusName = model.MaritalStatusName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<MaritalStatusVM>(JsonConvert.SerializeObject(entity));
                await _userInfoService.ActionLogAsync("Marital Status", ActionName.DataUpdated, beforeEntity, afterEntity, entity.MaritalStatusID, model);

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
        public async Task<MaritalStatusVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new MaritalStatusVM
                {
                    MaritalStatusID = data.MaritalStatusID,
                    MaritalStatusName = data.MaritalStatusName,
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
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.MaritalStatusName != null);

            var nameList = existingNames.Select(b => b.MaritalStatusName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region Soft Delete
        public async Task<MaritalStatusVM> SoftDeleteAsync(BaseViewModel model, List<int> ids)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => ids.Contains(x.MaritalStatusID));
                if (data == null || data.Count == 0)
                {
                    return new MaritalStatusVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<MaritalStatusVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.MaritalStatusID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = model.DeletedBy;
                    item.LIP = model.LIP;
                    item.LMAC = model.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("Marital Status", ActionName.DataDeleted, null, beforeEntity, targetIds, model);

                await _genericRepository.CommitTransactionAsync();

                return new MaritalStatusVM
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
        public async Task<PaginationService<MaritalStatus, MaritalStatusVM>.PaginationResult<MaritalStatusVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "MaritalStatusName", string sortOrder = "asc")
        {
            var query = _genericRepository.All();
            query = query.Where(x => x.DeletedAt == null);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "MaritalStatusID" => sortOrder == "desc" ? query.OrderByDescending(x => x.MaritalStatusID) : query.OrderBy(x => x.MaritalStatusID),
                    "MaritalStatusName" => sortOrder == "desc" ? query.OrderByDescending(x => x.MaritalStatusName) : query.OrderBy(x => x.MaritalStatusName),
                    _ => query.OrderBy(x => x.MaritalStatusID)
                };
            }

            return await PaginationService<MaritalStatus, MaritalStatusVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.MaritalStatusName, $"%{term}%"),
                x => new MaritalStatusVM
                {
                    MaritalStatusID = x.MaritalStatusID,
                    MaritalStatusName = x.MaritalStatusName ?? "-",
                });
        }
        #endregion
    }
}
