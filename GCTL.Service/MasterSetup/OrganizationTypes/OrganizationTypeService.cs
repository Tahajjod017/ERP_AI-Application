

using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.OrganizationTypes;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GCTL.Service.MasterSetup.CompanyTypes
{
    public class OrganizationTypeService : AppService<OrganizationTypes>, IOrganizationTypeService
    {
        #region Repositories & Services
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<OrganizationTypes> _genericRepository;

        public OrganizationTypeService(IGenericRepository<OrganizationTypes> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(OrganizationTypeVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.OrganizationTypeName == model.TypeName && b.DeletedAt != null);
                if (existingEntity.Any())
                {
                    var entityToRestore = existingEntity.FirstOrDefault();

                    entityToRestore.OrganizationTypeName = model.TypeName;
                    entityToRestore.OrganizationID = model.OrganizationID;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    await _userInfoService.ActionLogAsync("OrganizationTypes", ActionName.DataAdd, null, entityToRestore, entityToRestore.OrganizationTypeID, model);
                }
                else
                {
                    OrganizationTypes entity = new OrganizationTypes();
                    entity.OrganizationTypeName = model.TypeName;
                    entity.OrganizationID = model.OrganizationID;
                    entity.CreatedAt = DateTime.Now;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("OrganizationTypes", ActionName.DataAdd, null, entity, entity.OrganizationTypeID, model);
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
        public async Task<bool> UpdateAsync(OrganizationTypeVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.Id);
                if (entity == null)
                {
                    return false;
                }

                var beforeEntity = JsonConvert.DeserializeObject<OrganizationTypeVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));

                entity.OrganizationTypeName = model.TypeName;
                entity.OrganizationID = model.OrganizationID;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<OrganizationTypeVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                await _userInfoService.ActionLogAsync("Gender", ActionName.DataUpdated, beforeEntity, afterEntity, entity.OrganizationTypeID, model);

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
        public async Task<OrganizationTypeVM> GetByIdAsync(int organizationID, int id)
        {
            try
            {
                var data = await _genericRepository.FirstOrDefaultAsync(q=> q.OrganizationTypeID == id && q.OrganizationTypeID == organizationID);
                if (data == null) return null;

                return new OrganizationTypeVM
                {
                    Id = data.OrganizationTypeID,
                    TypeName = data.OrganizationTypeName,
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
        public async Task<bool> IsNameUniqueAsync(int organizationID, string name)
        {
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.OrganizationTypeName != null && b.OrganizationID == organizationID);

            var nameList = existingNames.Select(b => b.OrganizationTypeName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region Soft Delete
        public async Task<OrganizationTypeVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.OrganizationTypeID) && x.OrganizationID == requestVM.OrganizationID);
                if (data == null || data.Count == 0)
                {
                    return new OrganizationTypeVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<OrganizationTypeVM>>(JsonConvert.SerializeObject(data, JsonSettings.IgnoreReferenceLoop));
                var targetIds = data.Select(x => (int?)x.OrganizationTypeID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("Gender", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new OrganizationTypeVM
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
        public async Task<PaginationService<OrganizationTypes, OrganizationTypeVM>.PaginationResult<OrganizationTypeVM>> GetAllAsync(int organizationID, int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "TypeName", string sortOrder = "asc")
        {
            var query = _genericRepository.All();
            query = query.Where(x => x.DeletedAt == null && x.OrganizationID == organizationID);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "Id" => sortOrder == "desc" ? query.OrderByDescending(x => x.OrganizationTypeID) : query.OrderBy(x => x.OrganizationTypeID),
                    "TypeName" => sortOrder == "desc" ? query.OrderByDescending(x => x.OrganizationTypeName) : query.OrderBy(x => x.OrganizationTypeName),
                    _ => query.OrderBy(x => x.OrganizationTypeID)
                };
            }

            return await PaginationService<OrganizationTypes, OrganizationTypeVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.OrganizationTypeName, $"%{term}%"),
                x => new OrganizationTypeVM
                {
                    Id = x.OrganizationTypeID,
                    TypeName = x.OrganizationTypeName ?? "-",
                });
        }
        #endregion
    }
}
