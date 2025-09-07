using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.LeadSource;
using GCTL.Core.ViewModels.MasterSetup.Priority;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.MasterSetup.LeadSource;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.MasterSetup.Priority
{
    public class PriorityService : AppService<Priorities>, IPriorityService
    {
        #region Repositories & Priorities
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Priorities> _genericRepository;
        public PriorityService(IGenericRepository<Priorities> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion

        #region add
        public async Task<bool> AddAsync(PriorityVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.PriorityName == model.PriorityName && b.DeletedAt != null);
                if (existingEntity.Any())
                {
                    var entityToRestore = existingEntity.FirstOrDefault();

                    entityToRestore.PriorityName = model.PriorityName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    //entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.DeletedBy = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    await _userInfoService.ActionLogAsync("Priorities", ActionName.DataAdd, null, entityToRestore, entityToRestore.PriorityID, model);
                }
                else
                {
                    Priorities entity = new Priorities();
                    entity.PriorityName = model.PriorityName;
                    entity.CreatedAt = DateTime.Now;
                    entity.CreatedBy = model.CreatedBy;
                    //entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Priorities", ActionName.DataAdd, null, entity, entity.PriorityID, model);
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

        #region GetAllAsync
        public async Task<PaginationService<Priorities, PriorityVM>.PaginationResult<PriorityVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "PriorityName", string sortOrder = "asc")
        {
            var query = _genericRepository.All();
            query = query.Where(x => x.DeletedAt == null);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "PriorityID" => sortOrder == "desc" ? query.OrderByDescending(x => x.PriorityID) : query.OrderBy(x => x.PriorityID),
                    "PriorityName" => sortOrder == "desc" ? query.OrderByDescending(x => x.PriorityName) : query.OrderBy(x => x.PriorityName),
                    _ => query.OrderBy(x => x.PriorityID)
                };
            }

            return await PaginationService<Priorities, PriorityVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.PriorityName, $"%{term}%"),
                x => new PriorityVM
                {
                    PriorityID = x.PriorityID,
                    PriorityName = x.PriorityName ?? "-",
                });
        }
        #endregion

        #region IsNameUniqueAsync
        public async Task<bool> IsNameUniqueAsync(string name)
        {
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.PriorityName != null);

            var nameList = existingNames.Select(b => b.PriorityName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion;


        #region Get
        public async Task<PriorityVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new PriorityVM
                {
                    PriorityID = data.PriorityID,
                    PriorityName = data.PriorityName,
                };
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., to a file or logging service)
                throw; // Rethrow or return an error-specific response
            }
        }
        #endregion


        #region Update
        public async Task<bool> UpdateAsync(PriorityVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.PriorityID);
                if (entity == null)
                {
                    return false;
                }

                //var beforeEntity = JsonConvert.DeserializeObject<PriorityVM>(JsonConvert.SerializeObject(entity));

                entity.PriorityName = model.PriorityName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                //entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                //var afterEntity = JsonConvert.DeserializeObject<PriorityVM>(JsonConvert.SerializeObject(entity));
                //await _userInfoService.ActionLogAsync("Service", ActionName.DataUpdated, beforeEntity, afterEntity, entity.PriorityID, model);

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

        #region Soft Delete
        public async Task<PriorityVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.PriorityID));
                if (data == null || data.Count == 0)
                {
                    return new PriorityVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                //var beforeEntity = JsonConvert.DeserializeObject<List<PriorityVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.PriorityID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = requestVM.DeletedBy;
                    //item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                //await _userInfoService.ActionLogDeleteAsync("Service", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new PriorityVM
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
    }
}
