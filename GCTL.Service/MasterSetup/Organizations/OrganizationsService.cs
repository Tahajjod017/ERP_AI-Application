using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.Grade;
using GCTL.Core.ViewModels.MasterSetup.Organizations;
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

namespace GCTL.Service.MasterSetup.Organizations
{
    public class OrganizationsService : AppService<Organization>, IOrganizationsService
    {
        #region Services & Repositories
        private readonly IGenericRepository<Organization> _genericRepository;
        private readonly IUserInfoService _userInfoService;

        public OrganizationsService(IGenericRepository<Organization> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(OrganizationsVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.OrganizationName == model.OrganizationName && b.DeletedAt != null);
                if (existingEntity.Any())
                {

                    var entityToRestore = existingEntity.FirstOrDefault();
                    if (entityToRestore == null) return false;

                    entityToRestore.OrganizationName = model.OrganizationName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;
                    entityToRestore.UpdatedBy = model.UpdatedBy ?? null;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    var afterEntity = JsonConvert.DeserializeObject<OrganizationsVM>(JsonConvert.SerializeObject(entityToRestore));
                    await _userInfoService.ActionLogAsync("Organization", ActionName.DataAdd, null, entityToRestore, entityToRestore.OrganizationID, model);
                }
                else
                {
                    Organization entity = new Organization();
                    entity.OrganizationName = model.OrganizationName;
                    entity.CreatedAt = DateTime.Now;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;
                    entity.CreatedBy = model.CreatedBy ?? null;
                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Organization", ActionName.DataAdd, null, entity, entity.OrganizationID, model);
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
        public async Task<bool> UpdateAsync(OrganizationsVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.OrganizationID);
                if (entity == null)
                {
                    return false;
                }
                var beforeEntity = JsonConvert.DeserializeObject<OrganizationsVM>(JsonConvert.SerializeObject(entity));
                entity.OrganizationName = model.OrganizationName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;
                entity.UpdatedBy = model.UpdatedBy ?? null;
                await _genericRepository.UpdateAsync(entity);
                var afterEntity = JsonConvert.DeserializeObject<OrganizationsVM>(JsonConvert.SerializeObject(entity));
                await _userInfoService.ActionLogAsync("Organization", ActionName.DataUpdated, beforeEntity, afterEntity, entity.OrganizationID, model);
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
        public async Task<OrganizationsVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new OrganizationsVM
                {
                    OrganizationID = data.OrganizationID,
                    OrganizationName = data.OrganizationName,
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
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.OrganizationName != null);

            var nameList = existingNames.Select(b => b.OrganizationName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region SoftDeleteAsync
        public async Task<OrganizationsVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.OrganizationID));
                if (data == null || data.Count == 0)
                {
                    return new OrganizationsVM
                    {
                        Message = "No data found to delete."
                    };
                }
                var beforeEntity = JsonConvert.DeserializeObject<List<OrganizationsVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.OrganizationID).ToList();
                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                    item.DeletedBy = requestVM.DeletedBy ?? null;
                }

                await _genericRepository.UpdateRangeAsync(data);
                await _userInfoService.ActionLogDeleteAsync("Organizations", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);
                await _genericRepository.CommitTransactionAsync();

                return new OrganizationsVM
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
        public async Task<PaginationService<Organization, OrganizationsVM>.PaginationResult<OrganizationsVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "OrganizationID", string sortOrder = "desc")
        {
            var query = _genericRepository.AllActive();

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "OrganizationID" => sortOrder == "desc" ? query.OrderByDescending(x => x.OrganizationID) : query.OrderBy(x => x.OrganizationID),
                    "OrganizationName" => sortOrder == "desc" ? query.OrderByDescending(x => x.OrganizationName) : query.OrderBy(x => x.OrganizationName),
                    _ => query.OrderBy(x => x.OrganizationID)
                };
            }

            var result = await PaginationService<Organization, OrganizationsVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.OrganizationName, $"%{term}%"),
                x => new OrganizationsVM
                {
                    OrganizationID = x.OrganizationID,
                    OrganizationName = x.OrganizationName ?? "-",
                });

            return result;
        }
        #endregion
    }
}
