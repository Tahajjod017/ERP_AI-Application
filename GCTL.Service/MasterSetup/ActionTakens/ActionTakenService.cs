using GCTL.Core.Repository;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.MasterSetup.ActionTakens;
using static Dapper.SqlMapper;
using GCTL.Service.Pagination;
using GCTL.Core.ViewModels;
using GCTL.Service.ActionLogAudit;
using GCTL.Core.Helpers;
using Newtonsoft.Json;

namespace GCTL.Service.MasterSetup.ActionTakens
{
    public class ActionTakenService : AppService<ActionTaken>, IActionTakenService
    {
        #region Repositories
        private readonly IGenericRepository<ActionTaken> _genericRepository;
        private readonly IUserInfoService userInfoService;
        public ActionTakenService(IGenericRepository<ActionTaken> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            this.userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(ActionTakenVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.ActionTakenName == model.ActionTakenName && b.DeletedAt != null);
                if (existingEntity.Any())
                {

                    var entityToRestore = existingEntity.FirstOrDefault();
                    if (entityToRestore == null) return false;
                   
                    entityToRestore.ActionTakenName = model.ActionTakenName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;

                    entityToRestore.LIP = model.LIP;


         

                    entityToRestore.LMAC = model.LMAC;
                    entityToRestore.UpdatedBy = model.UpdatedBy ?? null;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    var afterEntity = JsonConvert.DeserializeObject<ActionTakenVM>(JsonConvert.SerializeObject(entityToRestore));
                    await userInfoService.ActionLogAsync("Action Taken", ActionName.DataAdd, null, entityToRestore, entityToRestore.ActionTakenID, model);
                }
                else
                {
                    ActionTaken entity = new ActionTaken();
                    entity.ActionTakenName = model.ActionTakenName;
                    entity.CreatedAt = DateTime.Now;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;
                    entity.CreatedBy = model.CreatedBy ?? null;
                    await _genericRepository.AddAsync(entity);
                    await userInfoService.ActionLogAsync("Action Taken", ActionName.DataAdd, null, entity, entity.ActionTakenID, model);
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
        public async Task<bool> UpdateAsync(ActionTakenVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.ActionTakenID);
                if (entity == null)
                {
                    return false;
                }
                var beforeEntity = JsonConvert.DeserializeObject<ActionTakenVM>(JsonConvert.SerializeObject(entity));
                entity.ActionTakenName = model.ActionTakenName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC =model.LMAC;
                entity.UpdatedBy=model.UpdatedBy ?? null;
                await _genericRepository.UpdateAsync(entity);
                var afterEntity = JsonConvert.DeserializeObject<ActionTakenVM>(JsonConvert.SerializeObject(entity));
                await userInfoService.ActionLogAsync("Action Taken", ActionName.DataUpdated, beforeEntity, afterEntity, entity.ActionTakenID, model);
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
        public async Task<ActionTakenVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new ActionTakenVM
                {
                    ActionTakenID = data.ActionTakenID,
                    ActionTakenName = data.ActionTakenName,
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
            var existingName = await _genericRepository.FindAsync(b => b.ActionTakenName == name && b.DeletedAt == null);
            return !existingName.Any();
        }
        #endregion


        #region SoftDeleteAsync
        public async Task<ActionTakenVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.ActionTakenID));
                if (data == null || data.Count == 0)
                {
                    return new ActionTakenVM
                    {
                        Message = "No data found to delete."
                    };
                }

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                    item.DeletedBy = requestVM.DeletedBy ?? null;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _genericRepository.CommitTransactionAsync();

                return new ActionTakenVM
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
        public async Task<PaginationService<ActionTaken, ActionTakenVM>.PaginationResult<ActionTakenVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "ActionTakenID", string sortOrder = "desc")
        {
            var query = _genericRepository.AllActive();

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "ActionTakenID" => sortOrder == "desc" ? query.OrderByDescending(x => x.ActionTakenID) : query.OrderBy(x => x.ActionTakenID),
                    "ActionTakenName" => sortOrder == "desc" ? query.OrderByDescending(x => x.ActionTakenName) : query.OrderBy(x => x.ActionTakenName),
                    _ => query.OrderBy(x => x.ActionTakenID)
                };
            }

            var result = await PaginationService<ActionTaken, ActionTakenVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.ActionTakenName, $"%{term}%"),
                x => new ActionTakenVM
                {
                    ActionTakenID = x.ActionTakenID,
                    ActionTakenName = x.ActionTakenName ?? "-",
                });

            return result;
        }
        #endregion
    }
}
