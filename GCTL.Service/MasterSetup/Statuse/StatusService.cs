using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.Statuses;
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
using GCTL.Core.Helpers.Jsonserialize;

namespace GCTL.Service.MasterSetup.Statuse
{
    public class StatusService : AppService<Statuses>, IStatusService
    {
        #region Repositories & Services
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Statuses> _genericRepository;

        public StatusService(IGenericRepository<Statuses> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(StatusVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.StatusName == model.StatusName && b.DeletedAt != null);
                if (existingEntity.Any())
                {
                    var entityToRestore = existingEntity.FirstOrDefault();

                    entityToRestore.StatusName = model.StatusName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    await _userInfoService.ActionLogAsync("Status", ActionName.DataAdd, null, entityToRestore, entityToRestore.StatusID, model);
                }
                else
                {
                    Statuses entity = new Statuses();
                    entity.StatusName = model.StatusName;
                    entity.CreatedAt = DateTime.Now;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Status", ActionName.DataAdd, null, entity, entity.StatusID, model);
                }

                await _genericRepository.CommitTransactionAsync();

                return true;
            }
            catch (Exception)
            {
                await _genericRepository.RollbackTransactionAsync();
                //throw ex;
                return false;
            }
        }
        #endregion


        #region Update
        public async Task<bool> UpdateAsync(StatusVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.StatusID);
                if (entity == null)
                {
                    return false;
                }

                var beforeEntity = JsonConvert.DeserializeObject<StatusVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));

                entity.StatusName = model.StatusName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<StatusVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                await _userInfoService.ActionLogAsync("Status", ActionName.DataUpdated, beforeEntity, afterEntity, entity.StatusID, model);

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
        public async Task<StatusVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new StatusVM
                {
                    StatusID = data.StatusID,
                    StatusName = data.StatusName,
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
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.StatusName != null);

            var nameList = existingNames.Select(b => b.StatusName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region Soft Delete
        public async Task<StatusVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.StatusID));
                if (data == null || data.Count == 0)
                {
                    return new StatusVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<StatusVM>>(JsonConvert.SerializeObject(data,JsonSettings.IgnoreReferenceLoop));
                var targetIds = data.Select(x => (int?)x.StatusID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("Status", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new StatusVM
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
        public async Task<PaginationService<Statuses, StatusVM>.PaginationResult<StatusVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "StatusID", string sortOrder = "desc")
        {
            var query = _genericRepository.All();
            query = query.Where(x => x.DeletedAt == null);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "StatusID" => sortOrder == "desc" ? query.OrderByDescending(x => x.StatusID) : query.OrderBy(x => x.StatusID),
                    "StatusName" => sortOrder == "desc" ? query.OrderByDescending(x => x.StatusName) : query.OrderBy(x => x.StatusName),
                    _ => query.OrderBy(x => x.StatusID)
                };
            }

            return await PaginationService<Statuses, StatusVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.StatusName, $"%{term}%"),
                x => new StatusVM
                {
                    StatusID = x.StatusID,
                    StatusName = x.StatusName ?? "-",
                });
        }


        #endregion

        #region Get StatusId

        public async Task<int> GetStatusIDAsync(string name, string type = "default")
        {
            var OpenStatus = await _genericRepository.AllActive().FirstOrDefaultAsync(e => e.StatusName.ToLower() == name.ToLower());

            if (OpenStatus == null)
            {
                OpenStatus = new Statuses()
                {
                    StatusName = name,
                    StatusType = type,
                     CreatedAt = DateTime.Now,
                };
                await _genericRepository.AddAsync(OpenStatus);
            }

            return OpenStatus.StatusID;
        }

        public int GetStatusID(string name, string type = "default")
        {
            var OpenStatus =  _genericRepository.AllActive().FirstOrDefault(e => e.StatusName.ToLower() == name.ToLower());

            if (OpenStatus == null)
            {
                OpenStatus = new Statuses()
                {
                    StatusName = name,
                    StatusType = type,
                    CreatedAt = DateTime.Now,
                    

                };
                 _genericRepository.AddAsync(OpenStatus);
            }

            return OpenStatus.StatusID;
        }

        #endregion
    }
}
