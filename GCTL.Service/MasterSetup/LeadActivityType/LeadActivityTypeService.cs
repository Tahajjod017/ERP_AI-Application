using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.LeadActivityType;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static Dapper.SqlMapper;

namespace GCTL.Service.MasterSetup.LeadActivityType
{
    public class LeadActivityTypeService : AppService<LeadActivityTypes>, ILeadActivityTypeService
    {
        #region Repositories & Services
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<LeadActivityTypes> _genericRepository;

        public LeadActivityTypeService(IGenericRepository<LeadActivityTypes> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region Add
        public async Task<bool> AddAsync(LeadActivityTypeVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.LeadActivityName == model.LeadActivityName && b.DeletedAt != null);
                if (existingEntity.Any())
                {
                    var entityToRestore = existingEntity.FirstOrDefault();

                    entityToRestore.LeadActivityName = model.LeadActivityName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.UseFor = model.UseFor;
                    entityToRestore.LeadActivityIcon = model.LeadActivityIcon;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.DeletedBy = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    await _userInfoService.ActionLogAsync("LeadActivityTypes", ActionName.DataAdd, null, entityToRestore, entityToRestore.LeadActivityTypeID, model);
                }
                else
                {
                    LeadActivityTypes entity = new LeadActivityTypes();
                    entity.LeadActivityName = model.LeadActivityName;
                    entity.UseFor = model.UseFor;
                    entity.LeadActivityIcon = model.LeadActivityIcon;
                    entity.CreatedAt = DateTime.Now;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("LeadActivityTypes", ActionName.DataAdd, null, entity, entity.LeadActivityTypeID, model);
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
        public async Task<bool> UpdateAsync(LeadActivityTypeVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.LeadActivityTypeID);
                if (entity == null)
                {
                    return false;
                }

                var beforeEntity = JsonConvert.DeserializeObject<LeadActivityTypeVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));

                entity.LeadActivityName = model.LeadActivityName;
                entity.UseFor = model.UseFor;
                entity.LeadActivityIcon = model.LeadActivityIcon;
                entity.CreatedBy = model.CreatedBy;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<LeadActivityTypeVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                await _userInfoService.ActionLogAsync("LeadActivityTypes", ActionName.DataUpdated, beforeEntity, afterEntity, entity.LeadActivityTypeID, model);

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
        public async Task<LeadActivityTypeVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new LeadActivityTypeVM
                {
                    LeadActivityTypeID = data.LeadActivityTypeID,
                    LeadActivityName = data.LeadActivityName,
                    LeadActivityIcon = data.LeadActivityIcon,
                    UseFor = data.UseFor,
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
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.LeadActivityName != null);

            var nameList = existingNames.Select(b => b.LeadActivityName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region Soft Delete
        public async Task<LeadActivityTypeVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.LeadActivityTypeID));
                if (data == null || data.Count == 0)
                {
                    return new LeadActivityTypeVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<LeadActivityTypeVM>>(JsonConvert.SerializeObject(data, JsonSettings.IgnoreReferenceLoop));
                var targetIds = data.Select(x => (int?)x.LeadActivityTypeID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("LeadActivityTypes", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new LeadActivityTypeVM
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
        public async Task<PaginationService<LeadActivityTypes, LeadActivityTypeVM>.PaginationResult<LeadActivityTypeVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "LeadActivityName", string sortOrder = "asc")
        {
            var query = _genericRepository.All();
            query = query.Where(x => x.DeletedAt == null);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "LeadActivityTypeID" => sortOrder == "desc" ? query.OrderByDescending(x => x.LeadActivityTypeID) : query.OrderBy(x => x.LeadActivityTypeID),
                    "LeadActivityName" => sortOrder == "desc" ? query.OrderByDescending(x => x.LeadActivityName) : query.OrderBy(x => x.LeadActivityName),
                    _ => query.OrderBy(x => x.LeadActivityTypeID)
                };
            }

            return await PaginationService<LeadActivityTypes, LeadActivityTypeVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.LeadActivityName, $"%{term}%"),
                x => new LeadActivityTypeVM
                {
                    LeadActivityTypeID = x.LeadActivityTypeID,
                    LeadActivityName = x.LeadActivityName ?? "-",
                    LeadActivityIcon = x.LeadActivityIcon,
                    UseFor = x.UseFor,
                });
        }
        #endregion
    }
}
