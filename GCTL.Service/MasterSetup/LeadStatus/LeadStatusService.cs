
using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.LeadStatuses;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.MasterSetup.LeadStatuses;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
namespace GCTL.Service.MasterSetup.LeadStatus
{
    public class LeadStatusService : AppService<GCTL.Data.Models.LeadStatuses>, ILeadStatusService
    {
        #region Repositories & GCTL.Data.Models.LeadStatuses
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<GCTL.Data.Models.LeadStatuses> _genericRepository;
        public LeadStatusService(IGenericRepository<GCTL.Data.Models.LeadStatuses> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion

        #region add
        public async Task<bool> AddAsync(LeadStatusVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.LeadStatusName == model.LeadStatusName && b.DeletedAt != null);
                if (existingEntity.Any())
                {
                    var entityToRestore = existingEntity.FirstOrDefault();

                    entityToRestore.LeadStatusName = model.LeadStatusName;
                    entityToRestore.IsSpecial = model.IsSpecial;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.DeletedBy = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    await _userInfoService.ActionLogAsync("LeadStatus", ActionName.DataAdd, null, entityToRestore, entityToRestore.LeadStatusID, model);
                }
                else
                {
                    GCTL.Data.Models.LeadStatuses entity = new GCTL.Data.Models.LeadStatuses();
                    entity.LeadStatusName = model.LeadStatusName;
                    entity.IsSpecial = model.IsSpecial;
                    entity.CreatedAt = DateTime.Now;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("LeadStatuses", ActionName.DataAdd, null, entity, entity.LeadStatusID, model);
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
        public async Task<PaginationService<GCTL.Data.Models.LeadStatuses, LeadStatusVM>.PaginationResult<LeadStatusVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "LeadStatusName", string sortOrder = "asc")
        {
            var query = _genericRepository.All();
            query = query.Where(x => x.DeletedAt == null);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "LeadStatusID" => sortOrder == "desc" ? query.OrderByDescending(x => x.LeadStatusID) : query.OrderBy(x => x.LeadStatusID),
                    "LeadStatusName" => sortOrder == "desc" ? query.OrderByDescending(x => x.LeadStatusName) : query.OrderBy(x => x.LeadStatusName),
                    _ => query.OrderBy(x => x.LeadStatusID)
                };
            }

            return await PaginationService<GCTL.Data.Models.LeadStatuses, LeadStatusVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.LeadStatusName, $"%{term}%"),
                x => new LeadStatusVM
                {
                    LeadStatusID = x.LeadStatusID,
                    LeadStatusName = x.LeadStatusName ?? "-",
                    IsSpecial = x.IsSpecial,
                });
        }
        #endregion

        #region IsNameUniqueAsync
        public async Task<bool> IsNameUniqueAsync(string name)
        {
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.LeadStatusName != null);

            var nameList = existingNames.Select(b => b.LeadStatusName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion;


        #region Get
        public async Task<LeadStatusVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new LeadStatusVM
                {
                    LeadStatusID = data.LeadStatusID,
                    LeadStatusName = data.LeadStatusName,
                    IsSpecial = data.IsSpecial,
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
        public async Task<bool> UpdateAsync(LeadStatusVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.LeadStatusID);
                if (entity == null)
                {
                    return false;
                }

                var beforeEntity = JsonConvert.DeserializeObject<LeadStatusVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));

                entity.LeadStatusName = model.LeadStatusName;
                entity.IsSpecial = model.IsSpecial;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<LeadStatusVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                await _userInfoService.ActionLogAsync("LeadStatus", ActionName.DataUpdated, beforeEntity, afterEntity, entity.LeadStatusID, model);

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
        public async Task<LeadStatusVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.LeadStatusID));
                if (data == null || data.Count == 0)
                {
                    return new LeadStatusVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<LeadStatusVM>>(JsonConvert.SerializeObject(data, JsonSettings.IgnoreReferenceLoop));
                var targetIds = data.Select(x => (int?)x.LeadStatusID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);
                await _userInfoService.ActionLogDeleteAsync("LeadStatus", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);
                await _genericRepository.CommitTransactionAsync();

                return new LeadStatusVM
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
