
using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.LeadSource;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.MasterSetup.LeadStatuses;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GCTL.Service.MasterSetup.LeadSource
{
    public class LeadSourceService : AppService<GCTL.Data.Models.LeadSources>, ILeadSourceService
    {
        #region Repositories & GCTL.Data.Models.LeadSources
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<GCTL.Data.Models.LeadSources> _genericRepository;
        public LeadSourceService(IGenericRepository<GCTL.Data.Models.LeadSources> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion

        #region add
        public async Task<bool> AddAsync(LeadSourceVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.LeadSourceName == model.LeadSourceName && b.DeletedAt != null && b.OrganizationID == model.OrganizationID);
                if (existingEntity.Any())
                {
                    var entityToRestore = existingEntity.FirstOrDefault();

                    entityToRestore.LeadSourceName = model.LeadSourceName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    //entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.DeletedBy = null;
                    entityToRestore.UpdatedAt = DateTime.Now;
                    entityToRestore.OrganizationID = model.OrganizationID;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    await _userInfoService.ActionLogAsync("LeadSource", ActionName.DataAdd, null, entityToRestore, entityToRestore.LeadSourceID, model);
                }
                else
                {
                    GCTL.Data.Models.LeadSources entity = new GCTL.Data.Models.LeadSources();
                    entity.LeadSourceName = model.LeadSourceName;
                    entity.CreatedAt = DateTime.Now;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LMAC = model.LMAC;
                    entity.OrganizationID = model.OrganizationID;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("LeadSource", ActionName.DataAdd, null, entity, entity.LeadSourceID, model);
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
        public async Task<PaginationService<GCTL.Data.Models.LeadSources, LeadSourceVM>.PaginationResult<LeadSourceVM>> GetAllAsync(int organizationID, int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "LeadSourceName", string sortOrder = "asc")
        {
            var query = _genericRepository.All();
            query = query.Where(x => x.DeletedAt == null && x.OrganizationID == organizationID);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "LeadSourceID" => sortOrder == "desc" ? query.OrderByDescending(x => x.LeadSourceID) : query.OrderBy(x => x.LeadSourceID),
                    "LeadSourceName" => sortOrder == "desc" ? query.OrderByDescending(x => x.LeadSourceName) : query.OrderBy(x => x.LeadSourceName),
                    _ => query.OrderBy(x => x.LeadSourceID)
                };
            }

            return await PaginationService<GCTL.Data.Models.LeadSources, LeadSourceVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.LeadSourceName, $"%{term}%"),
                x => new LeadSourceVM
                {
                    LeadSourceID = x.LeadSourceID,
                    LeadSourceName = x.LeadSourceName ?? "-",
                });
        }
        #endregion

        #region IsNameUniqueAsync
        public async Task<bool> IsNameUniqueAsync(int organizationID, string name)
        {
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.LeadSourceName != null && b.OrganizationID == organizationID);

            var nameList = existingNames.Select(b => b.LeadSourceName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion;

        #region Get
        public async Task<LeadSourceVM> GetByIdAsync(int organizationID,  int id)
        {
            try
            {
                var data = await _genericRepository.FirstOrDefaultAsync(u => u.LeadSourceID == id && u.OrganizationID == organizationID);
                if (data == null) return null;

                return new LeadSourceVM
                {
                    LeadSourceID = data.LeadSourceID,
                    LeadSourceName = data.LeadSourceName,
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
        public async Task<bool> UpdateAsync(LeadSourceVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.LeadSourceID);
                if (entity == null)
                {
                    return false;
                }

                var beforeEntity = JsonConvert.DeserializeObject<LeadSourceVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));

                entity.LeadSourceName = model.LeadSourceName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                //entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;
                entity.OrganizationID = model.OrganizationID;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<LeadSourceVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                await _userInfoService.ActionLogAsync("LeadSource", ActionName.DataUpdated, beforeEntity, afterEntity, entity.LeadSourceID, model);

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
        public async Task<LeadSourceVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.LeadSourceID) && x.OrganizationID == requestVM.OrganizationID);
                if (data == null || data.Count == 0)
                {
                    return new LeadSourceVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<LeadSourceVM>>(JsonConvert.SerializeObject(data, JsonSettings.IgnoreReferenceLoop));
                var targetIds = data.Select(x => (int?)x.LeadSourceID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = requestVM.DeletedBy;
                    //item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("LeadSource", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new LeadSourceVM
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
