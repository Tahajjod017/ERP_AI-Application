using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.EmploymentNature;
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

namespace GCTL.Service.MasterSetup.EmploymentNatures
{
    public class EmploymentNatureService : AppService<EmploymentNature>, IEmploymentNatureService
    {
        #region Repositories & Services
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<EmploymentNature> _genericRepository;

        public EmploymentNatureService(IGenericRepository<EmploymentNature> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(EmploymentNatureVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.EmploymentNatureName == model.EmploymentNatureName && b.DeletedAt != null);
                if (existingEntity.Any())
                {
                    var entityToRestore = existingEntity.FirstOrDefault();

                    entityToRestore.EmploymentNatureName = model.EmploymentNatureName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    await _userInfoService.ActionLogAsync("Employment Nature", ActionName.DataAdd, null, entityToRestore, entityToRestore.EmploymentNatureID, model);
                }
                else
                {
                    EmploymentNature entity = new EmploymentNature();
                    entity.EmploymentNatureName = model.EmploymentNatureName;
                    entity.CreatedAt = DateTime.Now;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Employment Nature", ActionName.DataAdd, null, entity, entity.EmploymentNatureID, model);
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
        public async Task<bool> UpdateAsync(EmploymentNatureVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.EmploymentNatureID);
                if (entity == null)
                {
                    return false;
                }

                var beforeEntity = JsonConvert.DeserializeObject<EmploymentNatureVM>(JsonConvert.SerializeObject(entity));

                entity.EmploymentNatureName = model.EmploymentNatureName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<EmploymentNatureVM>(JsonConvert.SerializeObject(entity));
                await _userInfoService.ActionLogAsync("Employment Nature", ActionName.DataUpdated, beforeEntity, afterEntity, entity.EmploymentNatureID, model);

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
        public async Task<EmploymentNatureVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new EmploymentNatureVM
                {
                    EmploymentNatureID = data.EmploymentNatureID,
                    EmploymentNatureName = data.EmploymentNatureName,
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
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.EmploymentNatureName != null);

            var nameList = existingNames.Select(b => b.EmploymentNatureName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region Soft Delete
        public async Task<EmploymentNatureVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.EmploymentNatureID));
                if (data == null || data.Count == 0)
                {
                    return new EmploymentNatureVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<EmploymentNatureVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.EmploymentNatureID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("Employment Nature", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new EmploymentNatureVM
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
        public async Task<PaginationService<EmploymentNature, EmploymentNatureVM>.PaginationResult<EmploymentNatureVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "EmploymentNatureName", string sortOrder = "asc")
        {
            var query = _genericRepository.All();
            query = query.Where(x => x.DeletedAt == null);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "EmploymentNatureID" => sortOrder == "desc" ? query.OrderByDescending(x => x.EmploymentNatureID) : query.OrderBy(x => x.EmploymentNatureID),
                    "EmploymentNatureName" => sortOrder == "desc" ? query.OrderByDescending(x => x.EmploymentNatureName) : query.OrderBy(x => x.EmploymentNatureName),
                    _ => query.OrderBy(x => x.EmploymentNatureID)
                };
            }

            return await PaginationService<EmploymentNature, EmploymentNatureVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.EmploymentNatureName, $"%{term}%"),
                x => new EmploymentNatureVM
                {
                    EmploymentNatureID = x.EmploymentNatureID,
                    EmploymentNatureName = x.EmploymentNatureName ?? "-",
                });
        }
        #endregion
    }
}
